using Godot;

namespace Netisu.Datamodels
{
	public partial class RemoteEventService : Node
	{
		[Export]
		private SerializeWorker? SerializeWorker;

		private NetworkManager _network;
		private Client.Server _serverInstance;

        public override void _Ready()
        {
            // Get the global NetworkManager instance
            _network = GetNode<NetworkManager>("/root/NetworkManager");
            if (Multiplayer.IsServer())
			{
				_serverInstance = GetTree().Root.GetNode<Client.Server>("Server");
			}
		}

		public object SendEventToServer(string eventName, params object[] args)
		{
			var variants = SerializeArguments(args);
			// Call the generic event RPC on the NetworkManager
			_network.RpcId(1, nameof(NetworkManager.FireServerEvent), eventName, variants);
			return null;
		}

		public object PushToClient(string username, string eventName, params object[] args)
		{
			if (!Multiplayer.IsServer())
				throw new MoonSharp.Interpreter.ScriptRuntimeException("Attempt to call server-sided function on client-side");

            if (_serverInstance == null) return "Server instance not found.";

			long targetPeerId = -1;
			// Find the peer ID for the given username
			foreach(var session in _serverInstance.SessionPlayers)
			{
				if (session.Value.PlayerData["Name"].ToString() == username)
				{
					targetPeerId = session.Key;
					break;
				}
			}

			if (targetPeerId != -1)
			{
				var variants = SerializeArguments(args);
				_network.RpcId(targetPeerId, nameof(NetworkManager.FireClientEvent), eventName, variants);
			}
			else
			{
				return username + "(Player) not found.";
			}
			return null;
		}

		public object PushToAllClients(string eventName, params object[] args)
		{
			if (!Multiplayer.IsServer())
				throw new MoonSharp.Interpreter.ScriptRuntimeException("Attempt to call server-sided function on client-side.");

			var variants = SerializeArguments(args);
			_network.Rpc(nameof(NetworkManager.FireClientEvent), eventName, variants);
			return null;
		}

		private Godot.Collections.Array SerializeArguments(object[] args)
		{
			Godot.Collections.Array variants = [];
			if (SerializeWorker == null) return variants;

			foreach (var arg in args)
			{
				object serializedArg = SerializeWorker.SerializeObject(arg);
				variants.Add(Variant.From(serializedArg));
			}
			return variants;
		}
	}
}