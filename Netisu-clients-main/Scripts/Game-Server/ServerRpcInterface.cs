using Godot;

public partial class ServerRpcInterface : Node
{
	// --- RPCs that are actually implemented on the server ---
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void AuthenticateUser(string authorizationKey, Godot.Collections.Dictionary<string, string> playerInfo) { /* Intentionally empty */ }

	// --- Dummy versions of the RPCs that the server calls ON the client ---
	// These must also be here to create a perfect match.
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void UpdatePlayerPosition(int peerId, Vector3 position) { /* Intentionally empty */ }
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void AuthenticationNoted(string userdata) { /* Intentionally empty */ }
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void PopulatePlayerList(Godot.Collections.Dictionary<long, Godot.Collections.Dictionary<string, string>> allPlayers) { /* Intentionally empty */ }
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void LoadInitialMap(string mapJson) { /* Intentionally empty */ }
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void AddPlayer(int peerId, Godot.Collections.Dictionary<string, string> playerInfo) { /* Intentionally empty */ }
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	public void OnPlayerLeft(int peerId, string playerName) { /* Intentionally empty */ }
}
