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
		public string MapJson = string.Empty;

		public override void _Ready()
		{
			Instance = this;
			_network = GetNode<NetworkManager>("/root/NetworkManager");

			// Get a reference to the Players node inside the instanced GameWorld scene.
			PlayersContainer = GetNode<Players>("Game/Players");

			// Connect to signals from the NetworkManager to drive server logic
			_network.Server_PlayerAuthenticated += OnPlayerAuthenticated;
			_network.Server_PlayerChatMessageReceived += OnChatMessageReceived;
			Multiplayer.PeerDisconnected += OnPlayerLeft;

			// Load map data
			using var file = FileAccess.Open("user://Maps/playtest-map.ntsm", FileAccess.ModeFlags.Read);
			if (file != null) MapJson = file.GetAsText();
			
			StartServer();
		}

		public override void _PhysicsProcess(double delta)
		{
			// Get the Environment node from the instanced GameWorld scene
			var environmentNode = GetNode<Netisu.Datamodels.Environment>("Game/Environment");
			if (environmentNode != null)
			{
				// Broadcast the current time of day to all clients
				Rpc(nameof(Client.UpdateEnvironment), environmentNode.DayTime);
			}
		}

		public void StartServer()
		{
			var peer = new ENetMultiplayerPeer();
			if (peer.CreateServer(25565) != Error.Ok)
			{
				GD.PrintErr("Failed to start server.");
				GetTree().Quit();
				return;
			}
			Multiplayer.MultiplayerPeer = peer;
			GD.Print("Server is up!");
		}

		private void OnPlayerAuthenticated(long peerId, string authKey, Godot.Collections.Dictionary<string, string> playerInfo)
		{
			GD.Print($"Peer {peerId} authenticated as {playerInfo["Name"]}");
			
			var playerNode = GD.Load<PackedScene>("res://Prefabs/Player/Player_Server.tscn").Instantiate<Player>();
			playerNode.Name = peerId.ToString();
			PlayersContainer.AddChild(playerNode);
			
			SessionPlayers.Add(peerId, new PlayerSession(authKey, playerInfo, playerNode));

			// Acknowledge authentication
			_network.RpcId(peerId, nameof(NetworkManager.AuthenticationNoted), "Welcome!");
			
			// Tell the new player about all existing players
			foreach(var session in SessionPlayers)
			{
				if (session.Key == peerId) continue;
				_network.RpcId(peerId, nameof(NetworkManager.AddPlayer), (int)session.Key, session.Value.PlayerData);
			}

			// Tell everyone else about the new player
			_network.Rpc(nameof(NetworkManager.AddPlayer), (int)peerId, playerInfo);
			
			// Send the map
			_network.RpcId(peerId, nameof(NetworkManager.LoadInitialMap), MapJson);
		}

		private void OnChatMessageReceived(long peerId, string message)
		{
			if (SessionPlayers.TryGetValue(peerId, out var session))
			{
				_network.Rpc(nameof(NetworkManager.ChatMessageClientRecieved), session.PlayerData["Name"], message);
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
