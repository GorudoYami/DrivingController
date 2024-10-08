﻿namespace DrivingController.Options;

public class TcpClientServiceOptions {
	public string ServerHost {
		get => Preferences.Default.Get(nameof(ServerHost), "192.168.0.13");
		set => Preferences.Default.Set(nameof(ServerHost), value);
	}
	public int MainServerPort {
		get => Preferences.Default.Get(nameof(MainServerPort), 6969);
		set => Preferences.Default.Set(nameof(MainServerPort), value);
	}
	public int VideoServerPort {
		get => Preferences.Default.Get(nameof(VideoServerPort), 2137);
		set => Preferences.Default.Set(nameof(VideoServerPort), value);
	}

	public int TimeoutSeconds {
		get => Preferences.Default.Get(nameof(TimeoutSeconds), 15);
		set => Preferences.Default.Set(nameof(TimeoutSeconds), value);
	}
}
