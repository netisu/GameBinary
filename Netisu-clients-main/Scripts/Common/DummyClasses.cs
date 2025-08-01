using System;
using Godot;
using Netisu.Datamodels;

/// <summary>
/// These series of classes and file MUST not be present inside ANY other place!
/// </summary>

public class Server 
{
	internal static void Dump(string v)
	{
		throw new NotImplementedException();
	}

	public void CallDeferred(params object[] actions) {}

	internal void Rpc(string v, string eventName, Godot.Collections.Array variants)
	{
		throw new NotImplementedException();
	}

	internal void RpcId(long rpcId, string v, string eventName, Godot.Collections.Array variants)
	{
		throw new NotImplementedException();
	}
}

namespace Netisu.Project 
{
	public class Flags
	{
		public const int PlayMode = 0;
		public const bool IsServer = false;

		public const bool IsWorkshop = true;
	}
}

public partial class Root : Node
{
	public readonly object authKey;
	public readonly Player localPlayer;

	internal void AskServerToUpdatePosition(Transform3D transform, Vector3 rotation)
	{
		throw new NotImplementedException();
	}

	internal void RpcId(int v1, string v2, string eventName, Godot.Collections.Array variants, string username, object authKey)
	{
		throw new NotImplementedException();
	}

	internal void TellServerToAnimate(string v)
	{
		throw new NotImplementedException();
	}
}

public partial class SerializeWorker : Godot.Node
{
	internal object SerializeObject(object arg)
	{
		throw new NotImplementedException();
	}
}

public partial class Interface : Godot.Node
{
	public bool Busy { get; internal set; }

	public void WriteIntoConsole(string co)
	{
		throw new NotImplementedException();
	}

	public void AddMessage(string __) 
	{
		throw new NotImplementedException();
	}
}
