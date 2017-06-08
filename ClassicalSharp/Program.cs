﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using ClassicalSharp.Textures;
using OpenTK;

namespace ClassicalSharp {
	
	internal static class Program {
		
		public const string AppName = "ClassicalSharp 0.99.8";
		
		public static string AppDirectory;
		
#if !LAUNCHER
		[STAThread]
		static void Main(string[] args) {
			AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
			string logPath = Path.Combine(AppDirectory, "client.log");
			ErrorHandler.InstallHandler(logPath);
			CleanupMainDirectory();
			Configuration.SkipPerfCountersHack();
			
			Utils.LogDebug("Starting " + AppName + "..");
			string path = Path.Combine(Program.AppDirectory, "texpacks");
			if (!File.Exists(Path.Combine(path, "default.zip"))) {
				MessageDefaultZipMissing(); return;
			}
			
			bool nullContext = true;
			#if !USE_DX
			nullContext = false;
			#endif
			int width, height;
			SelectResolution(out width, out height);
				
			if (args.Length == 0 || args.Length == 1) {
				const string skinServer = "http://static.classicube.net/skins/";
				string user = args.Length > 0 ? args[0] : "Singleplayer";
				using (Game game = new Game(user, null, skinServer, nullContext, width, height))
					game.Run();
			} else if (args.Length < 4) {
				Utils.LogDebug("ClassicalSharp.exe is only the raw client. You must either use the launcher or"
				     + " provide command line arguments to start the client.");
			} else {
				RunMultiplayer(args, nullContext, width, height);
			}
		}
		
		// put in separate function, because we don't want to load winforms assembly if possible
		static void MessageDefaultZipMissing() {
			MessageBox.Show("default.zip not found, try running the launcher first.", "Missing file");
		}
		
		static void SelectResolution(out int width, out int height) {
			DisplayDevice device = DisplayDevice.Default;
			width = 640; height = 480;
			
			if (device.Width >= 1024 && device.Height >= 768) {
				width = 800; height = 600;
			}
			if (device.Width >= 1920 && device.Height >= 1080) {
				width = 1600; height = 900;
			}
		}
		
		static void RunMultiplayer(string[] args, bool nullContext, int width, int height) {
			IPAddress ip = null;
			if (!IPAddress.TryParse(args[2], out ip)) {
				Utils.LogDebug("Invalid IP \"" + args[2] + '"'); return;
			}

			int port = 0;
			if (!Int32.TryParse(args[3], out port)) {
				Utils.LogDebug("Invalid port \"" + args[3] + '"');
				return;
			} else if (port < ushort.MinValue || port > ushort.MaxValue) {
				Utils.LogDebug("Specified port " + port + " is out of valid range.");
			}

			string skinServer = args.Length >= 5 ? args[4] : "http://s3.amazonaws.com/MinecraftSkins/";
			using (Game game = new Game(args[0], args[1], skinServer, nullContext, width, height)) {
				game.IPAddress = ip;
				game.Port = port;
				game.Run();
			}
		}
#endif
		
		internal static void CleanupMainDirectory() {
			string mapPath = Path.Combine(Program.AppDirectory, "maps");
			if (!Directory.Exists(mapPath))
				Directory.CreateDirectory(mapPath);
			
			string texPath = Path.Combine(Program.AppDirectory, "texpacks");
			if (!Directory.Exists(texPath))
				Directory.CreateDirectory(texPath);
		}
	}
}