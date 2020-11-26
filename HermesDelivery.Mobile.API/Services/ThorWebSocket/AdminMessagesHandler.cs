using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace CourierAPI.Services.ThorWebSocket
{
    /// <summary>
    /// 
    /// </summary>
    public static class AdminMessagesHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public static async Task HandleMessageAsync(string msg)
        {
            var input = JsonConvert.DeserializeObject<AdminMessageDTO>(msg, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            if (input != null)
            {
                switch (input.Event)
                {
                    case "CourierSelected":
                        await WebSocketService.CourierSelected(input.CourierId, (int)input.OrderId);
                        break;
                    case "OrderChanged":
                        await WebSocketService.OrderChanged(input.CourierId,(int)input.OrderId);
                        break;
                    case "ShiftChanged":
                        await WebSocketService.ShiftChanged(input.CourierId);
                        break;
                    case "Ping":
                        await WebSocketService.Ping(input.CourierId);
                        break;
                }
            }
        }
    }

    class AdminMessageDTO
    {
        public string Event { get; set; }
        public int CourierId { get; set; }
        public int? OrderId { get; set; } 
    }
}