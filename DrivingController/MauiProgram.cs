using CommunityToolkit.Maui;
using DrivingController.Interfaces;
using DrivingController.Options;
using DrivingController.Services;
using Microsoft.Extensions.Logging;

namespace DrivingController;

public static class MauiProgram {
	public static MauiApp CreateMauiApp() {
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkitMediaElement()
			.ConfigureFonts(fonts => {
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIconsRegular");
			});

		builder.Services.AddSingleton<ITcpClientService, TcpClientService>();
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddOptions<TcpClientServiceOptions>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
