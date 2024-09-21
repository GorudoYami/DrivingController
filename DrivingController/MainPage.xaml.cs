using DrivingController.Enums;
using DrivingController.Interfaces;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Diagnostics;

namespace DrivingController;

public partial class MainPage : ContentPage {
	private readonly ITcpClientService _tcpClientService;
	private Task? _videoTask;
	private CancellationTokenSource? _cts;

	public MainPage(ITcpClientService tcpClientService) {
		InitializeComponent();
		_tcpClientService = tcpClientService;
		BindEvents();
	}

	private void BindEvents() {
		Loaded += async (s, e) => await _tcpClientService.ConnectAsync();
		buttonLeft.Clicked += async (s, e) => await SendMessageAsync(MessageType.DriveLeft, 100);
		buttonRight.Clicked += async (s, e) => await SendMessageAsync(MessageType.DriveRight, 100);
		buttonForward.Clicked += async (s, e) => await SendMessageAsync(MessageType.DriveForward, 25);
		buttonBackward.Clicked += async (s, e) => await SendMessageAsync(MessageType.DriveBackward, 25);
		buttonStop.Clicked += async (s, e) => await SendMessageAsync(MessageType.DriveStop);
		swCamera.Toggled += async (s, e) => await ToggleVideoPreview();
		swSafety.Toggled += async (s, e) => await SendMessageAsync(swSafety.IsToggled ? MessageType.SensorsEnable : MessageType.SensorsDisable);
	}

	private async Task SendMessageAsync(MessageType messageType, byte? value = null) {
		try {
			byte?[] message = [(byte)messageType, value];
			await _tcpClientService.SendAsync(message.Where(x => x != null).Cast<byte>().ToArray());
		}
		catch (Exception ex) {
			var toast = Toast.Make(ex.ToString(), ToastDuration.Long, 10);
			await toast.Show();
		}
	}

	private async Task ToggleVideoPreview() {
		bool enableCamera = swCamera.IsToggled;
		await SendMessageAsync(enableCamera ? MessageType.CameraEnable : MessageType.CameraDisable);

		if (enableCamera) {
			_cts = new CancellationTokenSource();
			_videoTask = Task.Run(async () => {
				try {
					await _tcpClientService.ConnectVideoAsync(_cts.Token);
					while (_cts.Token.IsCancellationRequested == false) {
						byte[] data = await _tcpClientService.ReadVideoAsync(_cts.Token);
						Debug.WriteLine(data.Length);
					}
				}
				finally {
					await _tcpClientService.DisconnectVideoAsync();
				}
			}, _cts.Token);
		}
		else {
			_cts!.Cancel();
			await _videoTask!;
			_videoTask.Dispose();
			_videoTask = null;
			_cts.Dispose();
			_cts = null;
		}
	}
}

