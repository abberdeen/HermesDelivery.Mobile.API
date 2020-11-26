using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourierAPI.Infrastructure.AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Infrastructure.Serilog;
using CourierAPI.Services.Order;
using CourierAPI.Services.Shift; 
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace CourierAPI.Services.ThorWebSocket
{
    /// <summary>
    /// 
    /// </summary>
    public static class WebSocketService
    {
        private static AppDbContext _dbContext = new AppDbContext();
        //private static SupplierInfoService _supplierInfoService;
        //private static OrderInfoService _orderInfoService;
        //private static CourierShiftService _courierShiftService;
        private static CourierShiftHistoryService _courierShiftHistoryService;
        private static IncomingOrderService _incomingOrderService;
        // private static bool _isFirstInit = true;
        private static ILogger _logger = SerilogConfigurationManager.CreateLogger();
        /// <summary>
        /// Этот класс статический, поэтому нужно загрузить сервисы по вызову. 
        /// </summary>
        public static void InitServices()
        { 
            var mapper = AutoMapperConfigurationManager.CreateMapper();

            var _supplierInfoService = new SupplierInfoService(_logger, mapper);
            var _orderInfoService = new OrderInfoService(_logger, mapper, _supplierInfoService);
            var _courierShiftService = new CourierShiftService(_logger, mapper);
            _courierShiftHistoryService = new CourierShiftHistoryService(_logger, mapper, _supplierInfoService, _orderInfoService, _courierShiftService);
            _incomingOrderService = new IncomingOrderService(_logger, mapper, _supplierInfoService, _orderInfoService, _courierShiftHistoryService);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jwToken"></param>
        /// <returns></returns>
        public static async Task<int?> GetCourierIdAsync(string jwToken)
        {
            var courier = await _dbContext.CourierOAuthDatas.Where(e => e.JWToken == jwToken).FirstOrDefaultAsync();
            return courier?.CourierId;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <returns></returns>
        public static string PrepareMessage(string eventName, object messageObj)
        {
            var messageDto = new WebSocketMessageDTO()
            {
                Event = eventName,
                Data = messageObj
            };
            var output = JsonConvert.SerializeObject(messageDto, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task CourierSelected(int courierId, int orderId)
        {
            InitServices();
            var currentShiftDto = await _courierShiftHistoryService.GetCurrentOrNextAsync(courierId);
            if (currentShiftDto.IsStarted)
            {
                var courierKey = GetCourierKey(courierId);
                var pendingOrderDto = await _incomingOrderService.GetOrderIfPendingAsync(courierId, orderId);
                var message = PrepareMessage("OnNewOrder", pendingOrderDto);
                await WebSocketHandler.SendAsync(courierKey, message);
                _logger.Information("THOR::CourierSelected: Trying to send  message to client {0}, content: {1}", courierKey, message);
            } 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courierId"></param>
        /// <returns></returns>
        public static async Task OrderChanged(int courierId, int orderId)
        { 
            //
            await RaiseOnTurnChange(courierId, "OrderChanged");

            //
            var courierKey = GetCourierKey(courierId);

            InitServices();
            var orderDetailsDto = await _incomingOrderService.GetDetailsAsync(orderId);
            var message_OnOrderUpdate = PrepareMessage("OnOrderUpdate", orderDetailsDto);
            await WebSocketHandler.SendAsync(courierKey, message_OnOrderUpdate);
            _logger.Information("THOR::OrderChanged_OnOrderUpdate: Trying to send  message to client {0}, content: {1}", courierKey, message_OnOrderUpdate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courierId"></param> 
        /// <returns></returns>
        public static async Task ShiftChanged(int courierId)
        {
            await  RaiseOnTurnChange(courierId, "ShiftChanged");
        }

        private static async Task RaiseOnTurnChange(int courierId, string handler)
        {
            var courierKey = GetCourierKey(courierId);
            
            InitServices();
            var currentShiftDto = await _courierShiftHistoryService.GetCurrentOrNextAsync(courierId);
            
            var message_OnTurnChange = PrepareMessage("OnTurnChange", currentShiftDto);
            await WebSocketHandler.SendAsync(courierKey, message_OnTurnChange);
            _logger.Information("THOR::"+ handler +"_OnTurnChange: Trying to send  message to courier {0}, content: {1}", courierKey, message_OnTurnChange);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courierId"></param> 
        /// <returns></returns>
        public static async Task Ping(int courierId)
        {
            InitServices();
            var courierKey = GetCourierKey(courierId);
            var message = "Ping";
            await WebSocketHandler.SendAsync(GetCourierKey(courierId), message);
            _logger.Information("THOR::ShiftChanged: Trying to send message to courier {0}, content: {1}", courierKey, message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courierId"></param>
        /// <returns></returns>
        public static string GetCourierKey(int courierId)
        {
            return $"courier_{courierId.ToString()}";
        }
    }
}