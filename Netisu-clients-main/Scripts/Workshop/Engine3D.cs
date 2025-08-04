using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Netisu.Workshop;
using System.IO;
using Netisu.Game.Map;
using Netisu.Datamodels;

namespace Netisu.Workshop
{
	public partial class Engine3D : Node
	{
		[Export]
		Datamodels.Environment environment = null!;

		[Export]
		Datamodels.Game game = null!;

		[Export] public Sprite2D Stop;
		[Export] public Sprite2D Play;
		[Export] private Interpreter intp;

		private string EditorPlaytestBinary = @"C:\Users\ROBLO\OneDrive\Desktop\Netisu\builds\current";

		public static Engine3D Instance { get; private set; } = null!;

		public bool PlayTest = false;

		private readonly List<int> RunningInstances = [];

		private Node embeddedClientInstance = null;

		public override void _Ready()
		{
			Instance = this;
			if (!OS.HasFeature("editor"))
			{
				EditorPlaytestBinary = OS.GetExecutablePath().Replace("Netisu.exe", string.Empty);
			}
		}

		/// <summary>
		/// Starts or stops the playtest session.
		/// </summary>
		public async void IssuePlayTest()
		{
			if (PlayTest)
			{
				ShutPlaytest();
				GetNode<RichTextLabel>("/root/Root/EngineGUI/Output/Container/main/VBoxContainer/Console").Text = "";
				EngineUI.Instance.MakeOutputVisible();
				Stop.Visible = false;
				Play.Visible = true;
			}
			else
			{
				TerminateSessions();
				EngineUI.Instance.ExitCurrentScript();
				EngineUI.Instance.MakeOutputVisible();

				string SerializedString = Game.Exporter.SerializeTheGame(environment, game);

				DirAccess.MakeDirAbsolute("user://Maps");
				using var fileAccess = Godot.FileAccess.Open("user://Maps/playtest-map.ntsm", Godot.FileAccess.ModeFlags.Write);
				fileAccess.StoreString(SerializedString);

				Stop.Visible = true;
				Play.Visible = false;
				PlayTest = true;

				RunServer();

				await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

				StartEmbeddedClient();

				HookToCluaOutput();
			}
		}

		/// <summary>
		/// Stops the playtest, terminates the server, and removes the embedded client.
		/// </summary>
		public void ShutPlaytest()
		{
			if (!PlayTest)
				return;

			Stop.Visible = false;
			Play.Visible = true;
			PlayTest = false;

			TerminateSessions();
			CleanOutputCluaFile();
			RunningInstances.Clear();

			if (embeddedClientInstance != null && IsInstanceValid(embeddedClientInstance))
			{
				embeddedClientInstance.QueueFree();
				embeddedClientInstance = null;
			}
			if (EngineCamera.Instance != null)
			{
				EngineCamera.Instance.ProcessMode = ProcessModeEnum.Inherit;
				EngineCamera.Instance.Current = true;
			}
			// Restore visibility of the workshop's 3D editing tools, using the correct paths.
			GetNodeOrNull<Node3D>("/root/Root/EngineGUI/SubViewportContainer/SubViewport/UserInte")?.Show();
			GetNodeOrNull<Node3D>("/root/Root/EngineGUI/SubViewportContainer/SubViewport/GizmoNew")?.Show();
			GetNodeOrNull<Node3D>("/root/Root/EngineGUI/SubViewportContainer/SubViewport/SkyController")?.Show();

			GD.Print("Embedded client stopped and playtest shut down.");
		}
		/// <summary>
		///Removes the cameras
		/// </summary>
		public void SwitchToPlayerCamera()
		{
			if (EngineCamera.Instance != null)
			{
				EngineCamera.Instance.ProcessMode = ProcessModeEnum.Disabled;
				EngineCamera.Instance.Current = false;
				GD.Print("Engine3D: Switched to player camera.");
			}
		}

		/// <summary>
		/// Loads and instances the client scene inside the workshop's subviewport.
		/// </summary>
		private void StartEmbeddedClient()
		{
			if (embeddedClientInstance != null && IsInstanceValid(embeddedClientInstance))
			{
				GD.PrintErr("Embedded client is already running.");
				return;
			}

			var clientScene = GD.Load<PackedScene>("res://Scenes/Client.tscn");
			if (clientScene == null)
			{
				GD.PrintErr("Failed to load Client.tscn. Cannot start embedded client.");
				ShutPlaytest();
				return;
			}

			embeddedClientInstance = clientScene.Instantiate();

			var subViewport = GetNode<SubViewport>("/root/Root/EngineGUI/SubViewportContainer/SubViewport");
			if (subViewport == null)
			{
				GD.PrintErr("Could not find SubViewport to embed the client.");
				embeddedClientInstance.QueueFree();
				embeddedClientInstance = null;
				ShutPlaytest();
				return;
			}

			subViewport.AddChild(embeddedClientInstance);

			// Hide the 3D workshop tools.
			GetNodeOrNull<Node3D>("/root/Root/EngineGUI/SubViewportContainer/SubViewport/UserInte")?.Hide();
			GetNodeOrNull<Node3D>("/root/Root/EngineGUI/SubViewportContainer/SubViewport/GizmoNew")?.Hide();
			GetNodeOrNull<Node3D>("/root/Root/EngineGUI/SubViewportContainer/SubViewport/SkyController")?.Hide();

			GD.Print("Embedded client started.");
		}

		/// <summary>
		/// Starts the external game server process.
		/// </summary>
		public void RunServer()
		{
			int pid = OS.CreateProcess("cmd.exe",
			[
				"/c",
				@$"cd {EditorPlaytestBinary} && Netisu --headless --game-server --playtest --map-path=default --port=25565"
			], OS.HasFeature("editor"));

			if (pid != -1)
				RunningInstances.Add(pid);
		}

		public static void TerminateProcess(int pid)
		{
			try
			{
				Process process = Process.GetProcessById(pid);
				if (process != null)
				{
					process.Kill(true);
					GD.Print($"Process with PID {pid} terminated.");
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"Error terminating process: {ex.Message}");
			}
		}

		public void TerminateSessions()
		{
			foreach (int item in RunningInstances)
			{
				if (OS.IsProcessRunning(item))
				{
					TerminateProcess(item);
				}
			}
			RunningInstances.Clear();
		}

		public override void _Notification(int what)
		{
			if (what == NotificationWMCloseRequest)
			{
				TerminateSessions();
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			ClientInstanceUpdate();
		}

		public void ClientInstanceUpdate()
		{
			if (!PlayTest)
				return;

			bool anyRunning = false;
			foreach (int item in RunningInstances)
			{
				if (OS.IsProcessRunning(item))
				{
					anyRunning = true;
					break;
				}
			}

			if (!anyRunning)
			{
				ShutPlaytest();
			}
		}

		public void WriteToImportConsole(string _t)
		{
			GetNode<RichTextLabel>("/root/Root/_load_main/_output/console").Text += "[Engine] " + _t + " \n";
		}

		public static void CleanOutputCluaFile()
		{
			using var file = Godot.FileAccess.Open("user://_CLUA_OUTPUT/clua.log", Godot.FileAccess.ModeFlags.Write);
			file?.StoreString("");
		}

		public static void HookToCluaOutput(bool rehook = false)
		{
			DirAccess.MakeDirAbsolute("user://_CLUA_OUTPUT");
			using var file = Godot.FileAccess.Open("user://_CLUA_OUTPUT/clua.log", Godot.FileAccess.ModeFlags.Write);
			file?.StoreString("");
		}
	}
}
