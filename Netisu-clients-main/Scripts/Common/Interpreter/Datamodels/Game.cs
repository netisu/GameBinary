using Godot;
using System;
using System.Collections.Generic;

namespace Netisu.Datamodels
{
	public partial class Game : Instance
	{

		public readonly int GameID = 1;

		public object GetService(string serviceName)
		{
			switch (serviceName)
			{
				case "ChatService": return GetNode<ChatService>("ChatService");
				case "TweenService": return GetNode<TweenService>("TweenService");
				case "Players": return GetNode<Players>("Players");
				case "RemoteEventService": return GetNode<RemoteEventService>("RemoteEventService");
				case "DataStoreService":
					if (Netisu.Project.Flags.IsServer)
						return GetNode<DataStoreService>("DataStoreService");
					else
						throw new MoonSharp.Interpreter.ScriptRuntimeException("Attempt to access server-sided service on client.");
				default: return null!;
			}
		}

		public override Instance Parent
		{
			get => null!;
			set => RuntimeErrors.ScriptRuntimeException("cannot modify parent property in Game");
		}

		public override void Destroy()
		{
			RuntimeErrors.ScriptRuntimeException("cannot destroy Game");
		}
	}

}
