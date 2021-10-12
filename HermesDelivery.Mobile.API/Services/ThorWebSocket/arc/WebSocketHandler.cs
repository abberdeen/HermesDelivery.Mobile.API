using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CourierAPI.Services.ThorWebSocket.arc
{
    public class MyWebSocketHandler : IHttpHandler
    {
        private static ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> _sockets =
            new ConcurrentDictionary<string, System.Net.WebSockets.WebSocket>();

        public bool IsReusable => throw new NotImplementedException();

        public void ProcessRequest(HttpContext context)
        {
            if (context.IsWebSocketRequest)
            {
                var jwToken = context.Request.QueryString["Bearer"];
                context.AcceptWebSocketRequest(HandleWebSocket);
            }
        }

        private async Task HandleWebSocket(WebSocketContext wsContext)
        {
            var socketId = Guid.NewGuid().ToString();
            System.Net.WebSockets.WebSocket webSocket = wsContext.WebSocket;
            try
            {
                _sockets.TryAdd(socketId, webSocket);
                byte[] receiveBuffer = new byte[1024 * 4];
                while (webSocket.State == WebSocketState.Open)
                {
                    //var msg = Encoding.UTF8.GetBytes(WebSocketSerivce.GetNewOrders());
                    //await webSocket.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text,
                    //    true, CancellationToken.None);

                    WebSocketReceiveResult receiveResult =
                        await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        _sockets.TryRemove(socketId, out System.Net.WebSockets.WebSocket removedSocket);
                    }
                    else
                    {
                        foreach (var socket in _sockets.Where(x => x.Key != socketId).Select(x => x.Value))
                        {
                            await socket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count),
                                WebSocketMessageType.Text, receiveResult.EndOfMessage, CancellationToken.None);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
            finally
            {
                webSocket?.Dispose();
            }
        }
    }
}