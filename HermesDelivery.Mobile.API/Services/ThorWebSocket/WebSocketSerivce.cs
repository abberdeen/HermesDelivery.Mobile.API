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

namespace CourierAPI.Services.ThorWebSocket
{
    /// <summary>
    /// 
    /// </summary>
    public static class WebSocketService
    {
        private static AppDbContext _dbContext = new AppDbContext();

        private static SupplierInfoService _supplierInfoService;
        private static OrderInfoService _orderInfoService;
        private static CourierShiftService _courierShiftService;
        private static CourierShiftHistoryService _courierShiftHistoryService;
        private static IncomingOrderService _incomingOrderService;
        private static bool _isFirstInit = true;

        /// <summary>
        /// Этот класс статический, поэтому нужно загрузить сервисы по вызову. 
        /// </summary>
        public static void InitServices()
        {
            if (_isFirstInit)
            {
                var logger = SerilogConfigurationManager.CreateLogger();
                var mapper = AutoMapperConfigurationManager.CreateMapper();

                _supplierInfoService = new SupplierInfoService(logger, mapper);
                _orderInfoService = new OrderInfoService(logger, mapper, _supplierInfoService);
                _courierShiftService = new CourierShiftService(logger, mapper);
                _courierShiftHistoryService = new CourierShiftHistoryService(logger, mapper, _supplierInfoService, _orderInfoService, _courierShiftService);
                _incomingOrderService = new IncomingOrderService(logger, mapper, _supplierInfoService, _orderInfoService, _courierShiftHistoryService);
            }
            _isFirstInit = false;
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
            var output = JsonConvert.SerializeObject(messageDto);
            return output; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task CourierSelected(int courierId)
        {
            InitServices();
            var pendingOrderDto = await _incomingOrderService.GetPendingAsync(courierId);
            var message = PrepareMessage("OnNewOrder", pendingOrderDto);
            await WebSocketHandler.SendAsync(GetCourierKey(courierId), message);
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courierId"></param>
        /// <returns></returns>
        public static async Task OrderChanged(int courierId, int orderId)
        {
            InitServices();
            var courierKey = GetCourierKey(courierId);
            //
            var currentShiftDto = await _courierShiftHistoryService.GetCurrentOrNextAsync(courierId);
            var message_OnTurnChange = PrepareMessage("OnTurnChange", currentShiftDto);
            await WebSocketHandler.SendAsync(courierKey, message_OnTurnChange);

            //
            var orderDetailsDto = await _incomingOrderService.GetDetailsAsync(orderId);
            var message_OnOrderUpdate = PrepareMessage("OnOrderUpdate", orderDetailsDto);
            await WebSocketHandler.SendAsync(courierKey, message_OnOrderUpdate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courierId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static async Task ShiftChanged(int courierId, int orderId)
        {
            // Пересылаем запрос 
            await OrderChanged(courierId, orderId);
        }

        public static string GetCourierKey(int courierId)
        {
            return $"courier_{courierId.ToString()}";
        }
    }
}