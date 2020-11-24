namespace CourierAPI.Services.ThorWebSocket
{
    public class WebSocketMessageDTO
    {
        public string Event { get; set; }
        public object Data { get; set; }
    }
}