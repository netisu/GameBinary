using Godot;
using Netisu.Datamodels;

namespace Netisu.Client
{
	public partial class Client : Node
	{
		[Export]
		public Netisu.Datamodels.Game GameModel = null!;
		[Export]
		public Players PlayersContainer = null!;

		public const string PlayerScene = "res://Prefabs/Player/NPlayer.tscn";
		public const string IP_ADD = "127.0.0.1";
		public static Client Instance { get; private set; }
		public Player LocalPlayer { get; private set; }

		// This dictionary holds the local player's data, just like in the lobby example.
		// You can change "Name" from your UI before connecting.
		private Godot.Collections.Dictionary<string, string> _playerInfo = new()
		{
			{ "Name", "Player" + GD.Randi() % 1000 } // Default random name
		};

		public override void _Ready()
		{
			EstablishConnection();
		}

		public void EstablishConnection()
		{
			var clientPeer = new ENetMultiplayerPeer();
			clientPeer.CreateClient(IP_ADD, 25565);
			Multiplayer.MultiplayerPeer = clientPeer;

			Multiplayer.ConnectedToServer += () =>
			{
				GD.Print("Connection Succeeded. Authenticating...");
				var rpcProxy = new ServerRpcInterface { Name = "Server" };
				GetTree().Root.AddChild(rpcProxy);
				// When we connect, we send our player info along with the auth key.
				rpcProxy.RpcId(1, "AuthenticateUser", "playtest_auth_key", _playerInfo);

				rpcProxy.QueueFree();

			};

			Multiplayer.ConnectionFailed += () =>
			{
				GD.PrintErr("Connection Failed.");
				GetTree().Quit();
			};
		}

		// --- RPC Functions Called BY the Server ---
		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void AuthenticationNoted(string userdata)
		{
			if (Multiplayer.GetRemoteSenderId() != 1) return;
			GD.Print($"Server acknowledged: {userdata}");
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void PopulatePlayerList(Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>> allPlayers)
		{
			if (Multiplayer.GetRemoteSenderId() != 1) return;
			GD.Print("Received full player list from server.");
			foreach (var entry in allPlayers)
			{
				AddPlayer((int)entry.Key, entry.Value);
			}
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void LoadInitialMap(string mapJson)
		{
			if (Multiplayer.GetRemoteSenderId() != 1) return;
			GD.Print("Received map data from server. Building world...");
			var importer = new Netisu.Game.Map.Importer();
			importer.Import(mapJson, GameModel);
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void AddPlayer(int peerId, Godot.Collections.Dictionary<string, string> playerInfo)
		{
			if (Multiplayer.GetRemoteSenderId() != 1) return;
			if (PlayersContainer.GetNodeOrNull(peerId.ToString()) != null) return;

			GD.Print($"Spawning player for {playerInfo["Name"]} (Peer ID: {peerId})");
			Player playerPrefab = GD.Load<PackedScene>(PlayerScene).Instantiate<Player>();
			playerPrefab.Name = peerId.ToString(); // Use the unique ID for the node name
												   // You can now use the playerInfo to set up the visual node, e.g., a nameplate
			//playerPrefab.SetNameplate(playerInfo["Name"]);
			PlayersContainer.AddChild(playerPrefab);
			if (peerId == Multiplayer.GetUniqueId())
			{
				LocalPlayer = playerPrefab;
				GD.Print("LocalPlayer reference has been set.");
			}
		}

		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void OnPlayerLeft(int peerId, string playerName)
		{
			if (Multiplayer.GetRemoteSenderId() != 1) return;
			GD.Print($"{playerName} has disconnected.");
			var playerNode = PlayersContainer.GetNodeOrNull(peerId.ToString());
			playerNode?.QueueFree();
		}

		// --- Dummy RPCs to match the Server's script ---
		[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
		public void AuthenticateUser(string authorizationKey, Godot.Collections.Dictionary<string, string> playerInfo) { }
	}
}
