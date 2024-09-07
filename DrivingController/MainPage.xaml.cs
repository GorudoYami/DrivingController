using CommunityToolkit.Maui.Views;
using DrivingController.Interfaces;
using Timer = System.Timers.Timer;

namespace DrivingController;

public partial class MainPage : ContentPage {
	private readonly Stream _stream;
	private readonly ITcpClientService _tcpClientService;

	public MainPage(ITcpClientService tcpClientService) {
		InitializeComponent();
		_stream = new MemoryStream();
		_tcpClientService = tcpClientService;
		image.Source = ImageSource.FromFile("dotnet_bot_1.bmp");
	}

	public async Task ContentPage_Loaded(object? sender, EventArgs e) {
		await _tcpClientService.ConnectAsync();
	}
}

