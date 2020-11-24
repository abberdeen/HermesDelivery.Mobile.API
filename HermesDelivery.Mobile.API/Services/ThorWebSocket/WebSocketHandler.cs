using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CourierAPI.Services.ThorWebSocket
{
    public class WebSocketHandler
    {
        // Список всех клиентов
        private static readonly Dictionary<string, WebSocket> Clients = new Dictionary<string, WebSocket>();

        // Блокировка для обеспечения потокабезопасности
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public static async Task WebSocketRequest(WebSocketContext context)
        {
            // Получаем сокет клиента из контекста запроса
            var socket = context.WebSocket;

            var key = await ParseKey(context.RequestUri.Query);

            if (key is null)
            {
                var msg = Encoding.UTF8.GetBytes("Access denied");
                await socket.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, CancellationToken.None);

                return;
            }

            Locker.EnterWriteLock();
            try
            {
                // Добавляем его в список клиентов
                if (Clients.ContainsKey(key))
                {
                    Clients.Remove(key);
                }

                Clients.Add(key, socket);
            }
            finally
            {
                Locker.ExitWriteLock();
            }

            // Слушаем его
            while (socket.State == WebSocketState.Open)
            {
                var buffer = new ArraySegment<byte>(new byte[4096]);

                // Ожидаем данные от него
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var messageBytes = buffer.Skip(buffer.Offset).Take(result.Count).ToArray();
                    var receivedMessage = Encoding.UTF8.GetString(messageBytes);
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    //
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="clientKey">Значением может быть ИД курьера либо ИД Админ Панели (admin.kenguru.tj)</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task SendAsync(string clientKey, string message)
        {
            var client = Clients.FirstOrDefault(x => x.Key == clientKey).Value;

            if (client is null)
            {
                return;
            }

            try
            {
                if (client.State == WebSocketState.Open)
                {
                    var msg = Encoding.UTF8.GetBytes(message);
                    await client.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch (ObjectDisposedException)
            {
                try
                {
                    Clients.Remove(clientKey);
                }
                finally
                {
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private static async Task<string> ParseKey(string query)
        {
            var jwToken = HttpUtility.ParseQueryString(query).Get("Bearer");

            if (jwToken == "admin")
            {
                return "admin";
            }

            var courierId = await WebSocketService.GetCourierIdAsync(jwToken);

            if (courierId.HasValue == false)
            {
                return null;
            }

            return "courier_" + courierId.ToString();
        }
    }
}