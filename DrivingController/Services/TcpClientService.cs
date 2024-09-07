using DrivingController.Exceptions;
using DrivingController.Interfaces;
using DrivingController.Options;
using GorudoYami.Common.Streams;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Text;

namespace DrivingController.Services;

public class TcpClientService : ITcpClientService, IDisposable, IAsyncDisposable {
	public bool Connected => _mainServer?.Connected ?? false;
	public bool VideoConnected => _videoServer?.Connected ?? false;

	private readonly TcpClientServiceOptions _options;
	private TcpClient? _mainServer;
	private TcpClient? _videoServer;
	private ByteStreamReaderWriter? _mainServerReaderWriter;
	private ByteStreamReaderWriter? _videoServerReaderWriter;

	public TcpClientService(IOptions<TcpClientServiceOptions> options) {
		_options = options.Value;
		_mainServer = new TcpClient() {
			ReceiveTimeout = _options.TimeoutSeconds * 1000,
			SendTimeout = _options.TimeoutSeconds * 1000,
		};
		_videoServer = new TcpClient() {
			ReceiveTimeout = _options.TimeoutSeconds * 1000,
			SendTimeout = _options.TimeoutSeconds * 1000,
		};
	}

	public async Task ConnectAsync(CancellationToken cancellationToken = default) {
		if (_mainServer?.Connected == true) {
			await DisconnectAsync();
		}

		_mainServer = new TcpClient() {
			ReceiveTimeout = _options.TimeoutSeconds * 1000,
			SendTimeout = _options.TimeoutSeconds * 1000,
		};

		var timeoutTask = Task.Delay(_options.TimeoutSeconds * 1000, cancellationToken);
		Task connectTask = _mainServer.ConnectAsync(_options.ServerHost, _options.MainServerPort);
		await Task.WhenAny(timeoutTask, connectTask);

		if (_mainServer.Connected == false) {
			throw new InitializeCommunicationException($"Could not connect to the server at {_options.ServerHost}:{_options.MainServerPort}");
		}

		_mainServerReaderWriter = new ByteStreamReaderWriter(_mainServer.GetStream());
	}

	public async Task ConnectVideoAsync(CancellationToken cancellationToken = default) {
		if (_mainServer?.Connected != true) {
			throw new InvalidOperationException("Not connected to the main server");
		}

		if (_videoServer?.Connected == true) {
			await DisconnectVideoAsync();
		}

		_videoServer = new TcpClient() {
			ReceiveTimeout = _options.TimeoutSeconds * 1000,
			SendTimeout = _options.TimeoutSeconds * 1000,
		};

		var timeoutTask = Task.Delay(_options.TimeoutSeconds * 1000, cancellationToken);
		Task connectTask = _videoServer.ConnectAsync(_options.ServerHost, _options.VideoServerPort);
		await Task.WhenAny(timeoutTask, connectTask);

		if (_videoServer.Connected == false) {
			throw new InitializeCommunicationException($"Could not connect to the video server at {_options.ServerHost}:{_options.VideoServerPort}");
		}

		_videoServerReaderWriter = new ByteStreamReaderWriter(_videoServer.GetStream());
	}

	public async Task SendAsync(
		byte[] data,
		CancellationToken cancellationToken = default) {
		AssertConnected();
		await _mainServerReaderWriter!.WriteMessageAsync(data, cancellationToken);
	}

	public async Task SendAsync(
		string data,
		CancellationToken cancellationToken = default) {
		await SendAsync(Encoding.UTF8.GetBytes(data), cancellationToken);
	}

	public async Task<string> ReadLineAsync(CancellationToken cancellationToken = default) {
		return Encoding.UTF8.GetString(await ReadAsync(cancellationToken));
	}

	public async Task<byte[]> ReadAsync(CancellationToken cancellationToken = default) {
		AssertConnected();
		return await _mainServerReaderWriter!.ReadMessageAsync(cancellationToken);
	}

	private void AssertConnected() {
		if (_mainServer?.Connected != true) {
			throw new InvalidOperationException("Not connected to a server");
		}
	}

	public async Task DisconnectAsync() {
		if (_mainServerReaderWriter != null) {
			await _mainServerReaderWriter.DisposeAsync();
			_mainServerReaderWriter = null;
		}

		_mainServer?.Dispose();
		_mainServer = null;
	}

	public async Task DisconnectVideoAsync() {
		if (_videoServerReaderWriter != null) {
			await _videoServerReaderWriter.DisposeAsync();
			_videoServerReaderWriter = null;
		}

		_videoServer?.Dispose();
		_videoServer = null;
	}

	public void Dispose() {
		GC.SuppressFinalize(this);
		DisconnectVideoAsync().GetAwaiter().GetResult();
		DisconnectAsync().GetAwaiter().GetResult();
	}

	public async ValueTask DisposeAsync() {
		GC.SuppressFinalize(this);
		await DisconnectVideoAsync();
		await DisconnectAsync();
	}
}
