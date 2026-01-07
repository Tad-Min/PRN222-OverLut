using OverLut.Models.DAOs;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

public class ChatWebSocketHandler
{
    // Lưu trữ danh sách các User đang online (UserID -> Socket)
    private static ConcurrentDictionary<string, WebSocket> _users = new ConcurrentDictionary<string, WebSocket>();

    public static async Task Handle(HttpContext context, WebSocket webSocket, IServiceProvider services)
    {
        // 1. Lấy UserID (từ Query string: ws://localhost:5000/ws?userId=...)
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

                    // XỬ LÝ DATABASE TẠI ĐÂY
                    using (var scope = services.CreateScope())
                    {
                        var messageDao = scope.ServiceProvider.GetRequiredService<MessageDAO>();
                        // Giả sử messageString là JSON chứa: { ToUserId, Content, ChannelId }
                        // await messageDao.CreateMessageAsync(...);
                    }

                    // Gửi lại tin nhắn cho người nhận (nếu họ đang online)
                    // Ở đây bạn cần Parse JSON để lấy TargetUserId
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
}