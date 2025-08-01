using Godot;
using System;
using System.Collections.Generic;


namespace Netisu.Datamodels
{

#if !IS_WORKSHOP
	public partial class RemoteEventService : Node
	{

		[MoonSharp.Interpreter.Interop.MoonSharpVisible(false), Export]
		private SerializeWorker? SerializeWorker;

		public object SendEventToServer(string eventName, params object[] args)
		{
			if (Client.Client.Instance == null || Client.Client.Instance.LocalPlayer == null)
			{
				GD.PrintErr("RemoteEventService: LocalPlayer is not ready.");
				return null;
			}

			string username = Client.Client.Instance.LocalPlayer.username;
			// Note: The authKey is no longer stored on a 'Root' node. 
			// It's handled during the initial connection. We'll send a placeholder for now.
			string authKey = "playtest_auth_key";
			Godot.Collections.Array variants = [];
			foreach (var arg in args)
			{
				object get_result_from_export_worker = (object)SerializeWorker!.SerializeObject(arg);
				if (get_result_from_export_worker is Godot.Collections.Dictionary asDict)
				{
					variants.Add(asDict);
				}
				else
				{
					if (arg is System.String s)
					{
						variants.Add(new Godot.StringName(s));
					}
				}
			}
            Client.Client.Instance.RpcId(1, "ClientSentEvent", eventName, variants, username, authKey);
			return null!;
		}

		public object PushToClient(string username, string eventName, params object[] args)
        {
            if (!Multiplayer.IsServer())
                throw new MoonSharp.Interpreter.ScriptRuntimeException("Attempt to call server-sided function on client-side");

            long targetPeerId = -1;
            foreach(var session in Client.Server.Instance.SessionPlayers)
            {
                if (session.Value.PlayerData["Name"] == username)
                {
                    targetPeerId = session.Key;
                    break;
                }
            }

            if (targetPeerId != -1)
            {
                Godot.Collections.Array variants = [];
                foreach (var arg in args)
                {
                    object get_result_from_export_worker = SerializeWorker!.SerializeObject(arg);
                    if (get_result_from_export_worker is Godot.Collections.Dictionary asDict)
                    {
                        variants.Add(asDict);
                    }
                    else if (arg is string s)
                    {
                        variants.Add(new Godot.StringName(s));
                    }
                }
                Client.Server.Instance.RpcId(targetPeerId, "ServerSentEvent", eventName, variants);
            }
            else
            {
                return username + "(Player) not found.";
            }

            return null!;
        }

		public object PushToAllClients(string eventName, params object[] args)
        {
            if (!Multiplayer.IsServer())
                throw new MoonSharp.Interpreter.ScriptRuntimeException("Attempt to call server-sided function on client-side.");

            Godot.Collections.Array variants = [];
            foreach (var arg in args)
            {
                object get_result_from_export_worker = SerializeWorker!.SerializeObject(arg);
                if (get_result_from_export_worker is Godot.Collections.Dictionary asDict)
                {
                    variants.Add(asDict);
                }
                else if (arg is string s)
                {
                    variants.Add(Godot.Variant.From(s));
                }
            }

            // The server calls the RPC on itself. Since no peer ID is specified, it broadcasts to all clients.
            Client.Server.Instance.Rpc("ServerSentEvent", eventName, variants);
            
            return null!;
        }
	}
#endif
}