using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CourierAPI.Infrastructure.Serilog;
using RestSharp.Serialization.Json;
using Serilog;

namespace CourierAPI.Services.ThorWebSocket
{
    public class WebSocketHandler : IHttpHandler
    {
        // Список всех клиентов
        private static readonly Dictionary<string, WebSocket> Clients = new Dictionary<string, WebSocket>();

        // Блокировка для обеспечения потокабезопасности
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        private static readonly string AdminKey = "admin";

        private static ILogger _logger = SerilogConfigurationManager.CreateWebSocketLogger();

         
        public static async Task WebSocketRequest(WebSocketContext context)
        {    
            // Получаем сокет клиента из контекста запроса
            var socket = context.WebSocket;

            var key = await ParseKey(context.RequestUri.Query);

            if (key is null)
            {
                var msg = Encoding.UTF8.GetBytes("Access denied");
                await socket.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                _logger.Information("THOR: Connected new client with null key [Access denied], request query: {0}", context.RequestUri.Query);
                return;
            }
             
            AddClient(key, socket);
             
            _logger.Information("THOR: Connected new client with key: {0}, state: {1}", key, socket.State);

            if (key != "observer")
            {
                await SendAsync(key, "Hello son of Thor");
            } 

            // Слушаем его
            while (socket.State == WebSocketState.Open)
            {
                var receivedMessage = await ReceiveMessage(socket,key);
                if (string.IsNullOrEmpty(receivedMessage))
                {
                    continue;
                }
                _logger.Information("THOR: Received from client with key: {0}, message: {1}", key, receivedMessage);

                if (key == AdminKey)
                {
                    await AdminMessagesHandler.HandleMessageAsync(receivedMessage);
                }
                if (key == "observer")
                {
                     await SendActiveClientsToObserver();
                }
            }

            RemoveClient(key); 
        }
         
        private static async Task SendActiveClientsToObserver()
        {
            var listOfClients = new List<string>();
            lock (Clients)
            {
                listOfClients = new List<string>(Clients.Select(x => x.Key + ":" + x.Value.State)); 
            }

            await SendAsync("observer", new JsonSerializer().Serialize(listOfClients));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private static async Task<string> ReceiveMessage(WebSocket socket, string key)
        {
            var buffer = new ArraySegment<byte>(new byte[4096]);

            // Ожидаем данные от него
            var result = await socket.ReceiveAsync(buffer, CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var messageBytes = buffer.Skip(buffer.Offset).Take(result.Count).ToArray();
                var receivedMessage = Encoding.UTF8.GetString(messageBytes);
                if (!string.IsNullOrEmpty(receivedMessage))
                {
                    return receivedMessage;
                }
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                RemoveClient(key); 
            } 

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="clientKey">Значением может быть ИД курьера либо ИД Админ Панели (admin.kenguru.tj)</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task SendAsync(string clientKey, string message)
        { 
            var client = GetWebSocket(clientKey);
     
            if (client is null)
            {
                _logger.Information("THOR: Cant find client with key: {0}, to send message {1}", clientKey, message);
                return;
            }

            if (client.State == WebSocketState.Open)
            {
                var msg = Encoding.UTF8.GetBytes(message);
                await client.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                _logger.Information("THOR: Sent message to client with key {0}, content: {1}", clientKey, message);
            }
            else
            { 
                _logger.Information("THOR: Client with key: {0} state is not open, current state: {1}", clientKey, client.State);
            }
        }

        private static void AddClient(string key, WebSocket clientSocket)
        {
            RemoveClient(key);
            lock (Clients)
            {
                Clients.Add(key, clientSocket);
            } 
        }

        private static void RemoveClient(string key)
        {
            lock (Clients)
            {
                Clients.Remove(key);
            } 
            _logger.Information("THOR: Client with key: {0} removed", key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static WebSocket GetWebSocket(string key)
        {
            lock (Clients)
            {
                WebSocket socket;
                if (Clients.TryGetValue(key, out socket))
                {
                    return socket;
                }
            }

            return null;
        }


        public void ProcessRequest(HttpContext context)
        {
             
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

            // Проверяем это из админки или нет.
            if (jwToken == "admin")
            {
                return AdminKey;
            };

            //  
            if (jwToken == "observer")
            {
                return "observer";
            };


            // Проверяем это мобильное приложение курьера или нет
            var courierId = await WebSocketService.GetCourierIdAsync(jwToken);

            if (courierId.HasValue == false)
            {
                return null;
            }

            return "courier_" + courierId.ToString();
        }
    }
}