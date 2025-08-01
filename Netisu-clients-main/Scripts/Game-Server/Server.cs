using System.Collections.Generic;
using System;
using Godot;
using Netisu.Datamodels;

namespace Netisu.Client
{
	public sealed class PlayerSession(string auth, Godot.Collections.Dictionary<string, string> data, Player playerObject)
	{
		public string Authorization = auth;
		public Godot.Collections.Dictionary<string, string> PlayerData = data;
		public Player PlayerObject = playerObject;
	}

	public partial class Server : Node
	{
		[Export]
		public Players PlayersContainer = null!;
		[Export]
		public Netisu.Datamodels.Game GameDatamodel = null!;
		public static Server Instance { get; private set; } = null!;
		public static bool PlaytestMode { get; private set; } = false;

		public ENetMultiplayerPeer ServerPeer = new();
		public Dictionary<long, PlayerSession> SessionPlayers = [];
		public string MapJson = string.Empty;

		public override void _Ready()
		{
			Instance = this;
			PlaytestMode = Initializer.ProgramArguments.ContainsKey("playtest");

			if (Initializer.ProgramArguments.TryGetValue("map-path", out string path) && path == "default")
			{
				using var fileAccess = FileAccess.Open("user://Maps/playtest-map.ntsm", FileAccess.ModeFlags.Read);
				if (fileAccess != null)
				{
					MapJson = fileAccess.GetAsText();
					GD.Print("Server has loaded map data into memory.");
				}
				else
				{
					GD.PrintErr("No playtest-map.ntsm found?");
					GetTree().Quit();
				}
			}
			StartServer();
		}

		public void StartServer()
		{
			if (!int.TryParse(Initializer.ProgramArguments["port"], out int Port))
			{
				Port = PlaytestMode ? 2034 : 25565;
			}

			ServerPeer = new();
			var error = ServerPeer.CreateServer(Port);
			if (error != Error.Ok)
			{
				GD.PrintErr($"Failed to start server. Error: {error}. Is port {Port} already in use?");
				GetTree().Quit();
				return;
			}
			Multiplayer.MultiplayerPeer = ServerPeer;
			GD.Print("Server is up!");
			ServerPeer.PeerConnected += OnUserJoined;
			ServerPeer.PeerDisconnected += OnUserLeft;
		}

		private void OnUserJoined(long userPeer)
		{
			GD.Print($"Peer {userPeer} connected. Awaiting authentication.");
		}

		private void OnUserLeft(long userPeer)
		{
			if (SessionPlayers.TryGetValue(userPeer, out var session))
			{
				var playerNode = PlayersContainer.GetNodeOrNull(userPeer.ToString());
				playerNode?.QueueFree();
				SessionPlayers.Remove(userPeer);
				Rpc(nameof(Client.OnPlayerLeft), (int)userPeer, session.PlayerData["Name"]);
				GD.Print($"Peer {userPeer} ({session.PlayerData["Name"]}) disconnected and removed.");
			}
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void AuthenticateUser(string authorizationKey, Godot.Collections.Dictionary<string, string> playerInfo)
		{
			long userPeer = Multiplayer.GetRemoteSenderId();

			Player playerNode = GD.Load<PackedScene>("res://Prefabs/Player/Player_Server.tscn").Instantiate<Player>();
			playerNode.Name = userPeer.ToString(); // Use the unique ID for the node name
			PlayersContainer.AddChild(playerNode);

			SessionPlayers.Add(userPeer, new(authorizationKey, playerInfo, playerNode));
			GD.Print($"Peer {userPeer} authenticated as '{playerInfo["Name"]}'.");

			// 1. Acknowledge authentication and send the full player list to the new player
			RpcId(userPeer, nameof(Client.AuthenticationNoted), "Authentication successful.");

			// 2. Tell the new player about all existing players
			var allPlayersInfo = new Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>>();
			foreach (var session in SessionPlayers)
			{
				allPlayersInfo[session.Key] = session.Value.PlayerData;
			}
			RpcId(userPeer, nameof(Client.PopulatePlayerList), allPlayersInfo);

			// 3. Tell all OTHER players about the new player
			Rpc(nameof(Client.AddPlayer), (int)userPeer, playerInfo);

			// 4. Send the map data to the new player so they can build the world
			RpcId(userPeer, nameof(Client.LoadInitialMap), MapJson);
		}

		// --- Dummy RPCs to match the Client's script ---
		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void AuthenticationNoted(string userdata) { }

		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void PopulatePlayerList(Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>> allPlayers) { }

		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void LoadInitialMap(string mapJson) { }

		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void AddPlayer(int peerId, Godot.Collections.Dictionary<string, string> playerInfo) { }

		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void OnPlayerLeft(int peerId, string playerName) { }
	}
}
