using Godot;
using Netisu.Datamodels;

namespace Netisu.Client
{
	public partial class Client : Node
	{
		[Export]
		public Datamodels.Game GameModel = null!;
		public Players PlayersContainer = null!;
		public const string IP_ADD = "127.0.0.1";

		public static Client Instance { get; private set; }
		public Player LocalPlayer { get; private set; }

		private NetworkManager _network;
		private Godot.Collections.Dictionary<string, string> _playerInfo = new()
		{
			{ "Name", "Player" + GD.Randi() % 1000 }
		};

		public override void _Ready()
		{
			Instance = this;
			_network = GetNode<NetworkManager>("/root/NetworkManager");

			GameModel = GetNode<Datamodels.Game>("Game");
			PlayersContainer = GetNode<Players>("Game/Players");
			// Connect to signals from the NetworkManager
			_network.Client_AuthenticationNoted += OnAuthenticationNoted;
			_network.Client_PlayerListReceived += OnPlayerListReceived;
			_network.Client_MapLoadRequested += OnMapLoadRequested;
			_network.Client_PlayerAdded += OnPlayerAdded;
			_network.Client_PlayerLeft += OnPlayerLeft;
			_network.Client_ChatMessageReceived += OnChatMessageReceived;

			EstablishConnection();
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void UpdateEnvironment(float dayTime)
		{
			// Get the client's local Environment node
			var environmentNode = GetNode<Netisu.Datamodels.Environment>("Game/Environment");
			if (environmentNode != null)
			{
				// Update the DayTime property. The _Process method in Environment.cs
				// will automatically use this new value to update the sky shader.
				environmentNode.DayTime = dayTime;
			}
		}

		public void EstablishConnection()
		{
			var peer = new ENetMultiplayerPeer();
			peer.CreateClient(IP_ADD, 25565);
			Multiplayer.MultiplayerPeer = peer;

			Multiplayer.ConnectedToServer += () =>
			{
				GD.Print("Connection Succeeded. Authenticating...");
				// We now call the RPC on the global NetworkManager
				_network.RpcId(1, nameof(NetworkManager.AuthenticateUser), "playtest_auth_key", _playerInfo);
			};
			Multiplayer.ConnectionFailed += () => GD.PrintErr("Connection Failed.");
		}

		private void OnAuthenticationNoted(string message)
		{
			if (Multiplayer.GetRemoteSenderId() != 1) return;

			GD.Print($"Server says: {message}");
		}

		private void OnPlayerListReceived(Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>> allPlayers)
		{
			if (Multiplayer.GetRemoteSenderId() != 1) return;

			GD.Print("Received player list. Spawning players...");
			foreach (var entry in allPlayers)
			{
				OnPlayerAdded((int)entry.Key, entry.Value);
			}
		}

		private void OnMapLoadRequested(string mapJson)
		{
			if (Multiplayer.GetRemoteSenderId() != 1) return;

			GD.Print("Received map data. Building world...");
			var importer = new Game.Map.Importer();
			importer.Import(mapJson, GameModel);
		}

		private void OnPlayerAdded(int peerId, Godot.Collections.Dictionary<string, string> playerInfo)
		{
			if (Multiplayer.GetRemoteSenderId() != 1) return;

			if (PlayersContainer.GetNodeOrNull(peerId.ToString()) != null) return;

			GD.Print($"Spawning player for {playerInfo["Name"]} (ID: {peerId})");
			var playerScene = GD.Load<PackedScene>("res://Prefabs/Player/NPlayer.tscn");
			var playerNode = playerScene.Instantiate<Player>();
			playerNode.Name = peerId.ToString();// Use the unique ID for the node name

			// You can now use the playerInfo to set up the visual node, e.g., a nameplate
			//playerPrefab.SetNameplate(playerInfo["Name"]);
			PlayersContainer.AddChild(playerNode);

			if (peerId == Multiplayer.GetUniqueId())
			{
				LocalPlayer = playerNode;
				GD.Print("LocalPlayer reference has been set.");
			}
		}

		private void OnPlayerLeft(int peerId, string playerName)
		{
			if (Multiplayer.GetRemoteSenderId() != 1) return;

			GD.Print($"{playerName} has left.");
			var playerNode = PlayersContainer.GetNodeOrNull(peerId.ToString());
			playerNode?.QueueFree();
		}

		private void OnChatMessageReceived(string username, string message)
		{
			if (Multiplayer.GetRemoteSenderId() != 1) return;

			GD.Print($"[CHAT] {username}: {message}");
		}
	}
}
