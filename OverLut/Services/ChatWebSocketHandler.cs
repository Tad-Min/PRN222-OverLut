using Newtonsoft.Json;
using OverLut.Models.DAOs;
using OverLut.Models.DTOs;
using OverLut.Models.Repositories;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

public class ChatWebSocketHandler
{
    // Map of userId -> (connectionId -> WebSocket). Allows multiple connections per user.
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>> _users
        = new ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>>();

    public static async Task Handle(HttpContext context, WebSocket webSocket, IServiceProvider services)
    {
        // Prefer authenticated userId from claims; fall back to query parameter if necessary.
        var userId = context.User?.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            userId = context.Request.Query["userId"].ToString();
        }

        if (string.IsNullOrEmpty(userId))
        {
            // No user id - close connection politely
            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Missing userId", CancellationToken.None);
            }
            catch { }
            return;
        }

        var connectionId = Guid.NewGuid().ToString();

        // Add the connection
        var connections = _users.GetOrAdd(userId, _ => new ConcurrentDictionary<string, WebSocket>());
        connections[connectionId] = webSocket;

        var buffer = new byte[4 * 1024];
        var segment = new ArraySegment<byte>(buffer);
        var ms = new MemoryStream();

        try
        {
            while (webSocket.State == WebSocketState.Open && !context.RequestAborted.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(segment, context.RequestAborted);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    // Graceful close
                    _ = RemoveConnectionAsync(userId, connectionId);
                    var closeStatus = result.CloseStatus.GetValueOrDefault(WebSocketCloseStatus.NormalClosure);
                    var description = result.CloseStatusDescription ?? string.Empty;
                    await webSocket.CloseAsync(closeStatus, description, CancellationToken.None);
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    // ignore binary or handle if needed
                    continue;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    ms.Write(buffer, 0, result.Count);

                    if (!result.EndOfMessage)
                    {
                        // wait for remaining frames
                        continue;
                    }

                    // Full message assembled
                    ms.Seek(0, SeekOrigin.Begin);
                    var messageString = Encoding.UTF8.GetString(ms.ToArray());
                    ms.SetLength(0);

                    // Deserialize safely
                    MessageDTO msgDto = null;
                    try
                    {
                        msgDto = JsonConvert.DeserializeObject<MessageDTO>(messageString);
                    }
                    catch
                    {
                        // invalid payload; ignore or log
                        continue;
                    }

                    // Override sender id with authenticated claim (if available) to prevent spoofing
                    if (Guid.TryParse(context.User?.FindFirst("UserId")?.Value ?? userId, out var parsedUserId))
                    {
                        msgDto.UserId = parsedUserId;
                    }
                    else if (Guid.TryParse(userId, out parsedUserId))
                    {
                        msgDto.UserId = parsedUserId;
                    }
                    else
                    {
                        // invalid user id - ignore message
                        continue;
                    }

                    // Persist + broadcast using repository
                    using (var scope = services.CreateScope())
                    {
                        var chatRepo = scope.ServiceProvider.GetRequiredService<IChatRepository>();
                        // Save and broadcast (SendMessageAsync will call SendToUserAsync)
                        await chatRepo.SendMessageAsync(msgDto);
                    }
                }
            }
        }
        catch (OperationCanceledException) { /* request aborted */ }
        catch (WebSocketException) { /* socket error */ }
        catch (Exception)
        {
            // other errors
        }
        finally
        {
            // Ensure connection is removed
            await RemoveConnectionAsync(userId, connectionId);
            try { webSocket.Dispose(); } catch { }
        }
    }

    private static Task RemoveConnectionAsync(string userId, string connectionId)
    {
        if (_users.TryGetValue(userId, out var connections))
        {
            connections.TryRemove(connectionId, out _);
            if (connections.IsEmpty)
            {
                _users.TryRemove(userId, out _);
            }
        }
        return Task.CompletedTask;
    }

    public static async Task SendToUserAsync(string userId, string messageJson)
    {
        if (string.IsNullOrEmpty(userId)) return;

        if (!_users.TryGetValue(userId, out var connections)) return;

        var buffer = Encoding.UTF8.GetBytes(messageJson);
        var segment = new ArraySegment<byte>(buffer);

        var tasks = new List<Task>();

        foreach (var kv in connections)
        {
            var connId = kv.Key;
            var socket = kv.Value;

            if (socket == null) continue;

            if (socket.State != WebSocketState.Open)
            {
                // remove dead socket
                connections.TryRemove(connId, out _);
                continue;
            }

            tasks.Add(SafeSendAsync(userId, connId, socket, segment));
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }
    }

    private static async Task SafeSendAsync(string userId, string connectionId, WebSocket socket, ArraySegment<byte> segment)
    {
        try
        {
            await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch
        {
            // remove connection on error
            if (_users.TryGetValue(userId, out var connections))
            {
                connections.TryRemove(connectionId, out _);
                if (connections.IsEmpty)
                {
                    _users.TryRemove(userId, out _);
                }
            }
            try { socket.Abort(); socket.Dispose(); } catch { }
        }
    }
}