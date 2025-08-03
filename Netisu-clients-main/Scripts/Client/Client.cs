using Godot;
using Netisu.Datamodels;
using Netisu.Workshop;
using Netisu.Client.UI;

namespace Netisu.Client
{
	public partial class Client : Node
	{
		[Export] public Datamodels.Game GameModel = null!;
		[Export] public Players PlayersContainer = null!;
		public static Client Instance { get; private set; }
		public Player LocalPlayer { get; private set; }
		public const string IP_ADD = "127.0.0.1";
		private NetworkManager _network;
		private Godot.Collections.Dictionary<string, string> _playerInfo = new()
		{
			{ "Name", "Player" + GD.Randi() % 1000 }
		};

		public override void _Ready()
		{
			Instance = this;
			_network = GetNode<NetworkManager>("/root/NetworkManager");
			GameModel = GetParent().GetNode<Datamodels.Game>("Game");
			PlayersContainer = GetParent().GetNode<Players>("Game/Players");

			_network.Client_AuthenticationNoted += OnAuthenticationNoted;
			_network.Client_PlayerListReceived += OnPlayerListReceived;
			_network.Client_MapLoadRequested += OnMapLoadRequested;
			_network.Client_PlayerAdded += OnPlayerAdded;
			_network.Client_PlayerLeft += OnPlayerLeft;
			_network.Client_ChatMessageReceived += OnChatMessageReceived;

			EstablishConnection();
		}

		public void EstablishConnection(string auth_recieved = "playtest_auth_key")
		{
			var peer = new ENetMultiplayerPeer();
			peer.CreateClient(IP_ADD, 25565);
			Multiplayer.MultiplayerPeer = peer;

			Multiplayer.ConnectedToServer += () =>
			{
				GD.Print("Connection Succeeded. Authenticating...");
				_network.RpcId(1, nameof(NetworkManager.AuthenticateUser), auth_recieved, _playerInfo);
			};
			Multiplayer.ConnectionFailed += () => GD.PrintErr("Clients connection to the server was unsuccessful.");
		}

		// This signal is the definitive confirmation that we are connected and have a valid ID.
		private void OnAuthenticationNoted(string message)
		{
			// The server now sends the full player list first, so we don't need to spawn the local player here.
			// This signal is now just a confirmation.
			GD.Print($"Server says: {message}");
		}

		private void OnPlayerListReceived(Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>> allPlayers)
		{
			GD.Print($"Received initial list of {allPlayers.Count} other players.");
			foreach (var entry in allPlayers)
			{
				var typedPlayerInfo = new Godot.Collections.Dictionary();
				foreach (var key in entry.Value.Keys) typedPlayerInfo[key] = entry.Value[key];
				OnPlayerAdded((int)entry.Key, typedPlayerInfo);
			}
		}

		private void OnMapLoadRequested(string mapJson)
		{
			var importer = new Game.Map.Importer();
			importer.Import(mapJson, GameModel);
		}

		// This function is now ONLY for spawning remote players.
		private void OnPlayerAdded(int peerId, Godot.Collections.Dictionary playerInfo)
		{
			if (PlayersContainer.GetNodeOrNull(peerId.ToString()) != null) return;

			var playerScene = GD.Load<PackedScene>("res://Prefabs/Player/NPlayer.tscn");
			var playerNode = playerScene.Instantiate<Player>();
			playerNode.Name = peerId.ToString();
			PlayersContainer.AddChild(playerNode);

			// --- THIS IS THE FIX ---
			bool isLocal = peerId == Multiplayer.GetUniqueId();
			playerNode.Initialize(peerId, isLocal);

			if (isLocal)
			{
				LocalPlayer = playerNode;
				CallDeferred(nameof(DisableEditorCamera));
			}
		}
		private void DisableEditorCamera()
		{
			if (EngineCamera.Instance != null)
			{
				EngineCamera.Instance.Disabled = true;
				EngineCamera.Instance.Current = false;
			}
		}

		private void OnPlayerLeft(int peerId, string playerName)
		{
			var playerNode = PlayersContainer.GetNodeOrNull(peerId.ToString());
			playerNode?.QueueFree();
		}

		private void OnChatMessageReceived(string username, string message)
		{
			if (ChatManager.Instance != null)
			{
				ChatManager.Instance.MessageRecieved(username, message);
			}
		}
		private void OnPlayerLeft(int rpcId)
		{
			var relatedPlayerNode = PlayersContainer.GetNodeOrNull(rpcId.ToString());
			relatedPlayerNode?.QueueFree();
		}

		private void OnClientEventFired(string eventName, Godot.Collections.Array args)
		{
			GD.Print($"Client received event '{eventName}' from the server.");
		}
	}
}
