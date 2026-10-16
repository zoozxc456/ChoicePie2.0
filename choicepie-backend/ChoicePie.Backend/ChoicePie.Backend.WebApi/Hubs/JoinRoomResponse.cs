namespace ChoicePie.Backend.WebApi.Hubs;

/// <summary>
/// JoinRoom 的叫用回傳值（只回給呼叫者本人）。ReconnectToken 是重連憑證，絕不可透過任何廣播事件傳出。
/// </summary>
public sealed record JoinRoomResponse(Guid PlayerId, string ReconnectToken);
