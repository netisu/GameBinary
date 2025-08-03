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
		public Datamodels.Game GameDatamodel = null!;

		public static Server Instance { get; private set; } = null!;
		private NetworkManager _network;
		public Dictionary<long, PlayerSession> SessionPlayers = [];
		public static bool PlaytestMode { get; private set; } = false;
		public string MapJson = string.Empty;

		public override void _Ready()
		{
			Instance = this;
			PlaytestMode = Initializer.ProgramArguments.ContainsKey("playtest");
			_network = GetNode<NetworkManager>("/root/NetworkManager");

			// Get a reference to the Players node inside the instanced GameWorld scene.
			PlayersContainer = GetNode<Players>("Game/Players");

			// Connect to signals from the NetworkManager to drive server logic
			_network.Server_EventFired += OnServerEventFired;
			_network.Server_PlayerAuthenticated += OnPlayerAuthenticated;
			_network.Server_PlayerChatMessageReceived += OnChatMessageReceived;
			Multiplayer.PeerDisconnected += OnPlayerLeft;

			// Load map data
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

		public override void _PhysicsProcess(double delta)
		{
			// Get the Environment node from the instanced GameWorld scene
			var environmentNode = GetNode<Datamodels.Environment>("Game/Environment");
			if (environmentNode != null)
			{
				// Broadcast the current time of day to all clients
				_network.Rpc(nameof(NetworkManager.UpdateEnvironment), environmentNode.DayTime);
			}
		}
		private void OnServerEventFired(long peerId, string eventName, Godot.Collections.Array args)
		{
			GD.Print($"Server received event '{eventName}' from peer {peerId}.");
			// Add your logic here to handle the event from the client's Lua script.
		}

		public void StartServer()
		{
			if (!int.TryParse(Initializer.ProgramArguments["port"], out int Port))
			{
				Port = PlaytestMode ? 2034 : 25565;
			}

			var peer = new ENetMultiplayerPeer();
			if (peer.CreateServer(Port) != Error.Ok)
			{
				GD.PrintErr("Failed to start server.");
				GetTree().Quit();
				return;
			}
			Multiplayer.MultiplayerPeer = peer;
			GD.Print("Server is up!");
		}

		private void OnPlayerAuthenticated(long peerId, string authKey, Godot.Collections.Dictionary playerInfo)
		{
			string playerName = playerInfo.ContainsKey("Name") ? playerInfo["Name"].ToString() : $"Player_{peerId}";
			GD.Print($"Peer {peerId} authenticated as '{playerName}'.");

			var playerNode = GD.Load<PackedScene>("res://Prefabs/Player/Player_Server.tscn").Instantiate<Player>();
			playerNode.Name = peerId.ToString();
			PlayersContainer.AddChild(playerNode);

			var typedPlayerInfo = new Godot.Collections.Dictionary<string, string>();
			foreach (var key in playerInfo.Keys)
			{
				typedPlayerInfo[key.ToString()] = playerInfo[key].ToString();
			}
			SessionPlayers.Add(peerId, new PlayerSession(authKey, typedPlayerInfo, playerNode));

			_network.RpcId(peerId, nameof(NetworkManager.AuthenticationNoted), "Welcome!");

			var allPlayers = new Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>>();
			foreach (var p in SessionPlayers) allPlayers[p.Key] = p.Value.PlayerData;
			_network.RpcId(peerId, nameof(NetworkManager.PopulatePlayerList), allPlayers);

			_network.Rpc(nameof(NetworkManager.AddPlayer), (int)peerId, playerInfo);

			_network.RpcId(peerId, nameof(NetworkManager.LoadInitialMap), MapJson);
		}

		private void OnChatMessageReceived(long peerId, string message)
		{
			if (SessionPlayers.TryGetValue(peerId, out var session))
			{
				_network.Rpc(nameof(NetworkManager.ChatMessageClientRecieved), session.PlayerData["Username"], message);
			}
		}

		private void OnPlayerLeft(long peerId)
		{
			if (SessionPlayers.TryGetValue(peerId, out var session))
			{
				var playerNode = PlayersContainer.GetNodeOrNull(peerId.ToString());
				playerNode?.QueueFree();
				SessionPlayers.Remove(peerId);
				_network.Rpc(nameof(NetworkManager.OnPlayerLeft), (int)peerId);
				GD.Print($"Peer {peerId} ({session.PlayerData["Name"]}) disconnected.");
			}
		}
	}
}
