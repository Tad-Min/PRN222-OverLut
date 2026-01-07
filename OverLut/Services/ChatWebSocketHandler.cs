using Newtonsoft.Json;
using OverLut.Models.DAOs;
using OverLut.Models.DTOs;
using OverLut.Models.Repositories;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

public class ChatWebSocketHandler
{
    private static ConcurrentDictionary<string, WebSocket> _users = new ConcurrentDictionary<string, WebSocket>();

    public static async Task Handle(HttpContext context, WebSocket webSocket, IServiceProvider services)
    {
        var userId = context.Request.Query["userId"].ToString();
        if (string.IsNullOrEmpty(userId)) return;

        _users[userId] = webSocket;
        var buffer = new byte[1024 * 4];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var messageString = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Parse JSON từ client gửi lên
                    var msgDto = JsonConvert.DeserializeObject<MessageDTO>(messageString);
                    msgDto.UserId = Guid.Parse(userId); // Đảm bảo đúng người gửi

                    using (var scope = services.CreateScope())
                    {
                        var chatRepo = scope.ServiceProvider.GetRequiredService<IChatRepository>();
                        // Lưu DB và tự động Broadcast bên trong SendMessageAsync
                        await chatRepo.SendMessageAsync(msgDto);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    _users.TryRemove(userId, out _);
                    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                }
            }
        }
        catch (Exception) { _users.TryRemove(userId, out _); }
    }

    public static async Task SendToUserAsync(string userId, string messageJson)
    {
        if (_users.TryGetValue(userId, out var socket))
        {
            if (socket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(messageJson);
                await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}