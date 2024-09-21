
namespace DrivingController.Interfaces;

public interface ITcpClientService {
	bool Connected { get; }
	bool VideoConnected { get; }

	Task ConnectAsync(CancellationToken cancellationToken = default);
	Task ConnectVideoAsync(CancellationToken cancellationToken = default);
	Task DisconnectAsync();
	Task DisconnectVideoAsync();
	Task<byte[]> ReadAsync(CancellationToken cancellationToken = default);
	Task<string> ReadLineAsync(CancellationToken cancellationToken = default);
	Task<byte[]> ReadVideoAsync(CancellationToken cancellationToken = default);
	Task SendAsync(byte[] data, CancellationToken cancellationToken = default);
	Task SendAsync(string data, CancellationToken cancellationToken = default);
}
