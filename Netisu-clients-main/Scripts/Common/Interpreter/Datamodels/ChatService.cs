using Godot;
using System;
using System.Collections.Generic;

namespace Netisu.Datamodels
{
	public partial class ChatService : Instance
	{
		public void PushToLocalClient(string message)
		{
			if (Netisu.Project.Flags.PlayMode == 1 || Netisu.Project.Flags.PlayMode == 2)
				GetNode<Interface>("/root/Root/Interface").AddMessage(message);
			else
				throw new MoonSharp.Interpreter.ScriptRuntimeException("Attempt to call a client-function on server.");
		}

		public void Push(string message)
		{
			if (Netisu.Project.Flags.IsServer)
				GetNode<Server>("/root/Root").CallDeferred("ServerRequestMessageControl", "ServerMessage", message);
			else
				throw new MoonSharp.Interpreter.ScriptRuntimeException("Attempt to call a server-function on client.");
		}
	}
}
