using Godot;

public partial class NetworkManager : Node
{
	// --- Signals for the Server ---
	[Signal]
	public delegate void Server_PlayerAuthenticatedEventHandler(long peerId, string authKey, Godot.Collections.Dictionary playerInfo);
	[Signal]
	public delegate void Server_PlayerChatMessageReceivedEventHandler(long peerId, string message);
	[Signal]
	public delegate void Server_EventFiredEventHandler(long peerId, string eventName, Godot.Collections.Array args);


	// --- Signals for the Client ---
	[Signal]
	public delegate void Client_AuthenticationNotedEventHandler(string message);
	[Signal]
	public delegate void Client_PlayerAddedEventHandler(int peerId, Godot.Collections.Dictionary playerInfo);
	[Signal]
	public delegate void Client_PlayerLeftEventHandler(int peerId);
	[Signal]
	public delegate void Client_MapLoadRequestedEventHandler(string mapJson);
	[Signal]
	public delegate void Client_ChatMessageReceivedEventHandler(string username, string message);
	[Signal]
	public delegate void Client_UpdateEnvironmentEventHandler(float dayTime);
	[Signal]
	public delegate void Client_PlayerListReceivedEventHandler(Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>> allPlayers);
	[Signal]
	public delegate void Client_EventFiredEventHandler(string eventName, Godot.Collections.Array args);


	// --- RPCs Called BY the Client, Received BY the Server ---
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void AuthenticateUser(string authorizationKey, Godot.Collections.Dictionary playerInfo)
	{
		EmitSignal(SignalName.Server_PlayerAuthenticated, Multiplayer.GetRemoteSenderId(), authorizationKey, playerInfo);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void ChatMessageRecieved(string content)
	{
		EmitSignal(SignalName.Server_PlayerChatMessageReceived, Multiplayer.GetRemoteSenderId(), content);
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void FireServerEvent(string eventName, Godot.Collections.Array args)
	{
		EmitSignal(SignalName.Server_EventFired, Multiplayer.GetRemoteSenderId(), eventName, args);
	}


	// --- RPCs Called BY the Server, Received BY the Client ---
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void FireClientEvent(string eventName, Godot.Collections.Array args) // <-- NEW
	{
		EmitSignal(SignalName.Client_EventFired, eventName, args);
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void AuthenticationNoted(string userdata)
	{
		EmitSignal(SignalName.Client_AuthenticationNoted, userdata);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void LoadInitialMap(string mapJson)
	{
		EmitSignal(SignalName.Client_MapLoadRequested, mapJson);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void AddPlayer(int peerId, Godot.Collections.Dictionary playerInfo)
	{
		EmitSignal(SignalName.Client_PlayerAdded, peerId, playerInfo);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void OnPlayerLeft(int peerId)
	{
		EmitSignal(SignalName.Client_PlayerLeft, peerId);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void ChatMessageClientRecieved(string username, string content)
	{
		EmitSignal(SignalName.Client_ChatMessageReceived, username, content);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void UpdateEnvironment(float dayTime)
	{
		EmitSignal(SignalName.Client_UpdateEnvironment, dayTime);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void PopulatePlayerList(Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>> allPlayers)
	{
		EmitSignal(SignalName.Client_PlayerListReceived, allPlayers);
	}
}
