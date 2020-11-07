using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.Orders;
using CourierAPI.Services.Mock;
using CourierAPI.Services.Sms;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CourierAPI.Services.Order
{
    [Authorize]
    public class IncomingOrderService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public IncomingOrderService(ILogger logger, IMapper mapper, MessageService messageService)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Получить список заказов.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<IncomingOrderDto>> GetOrders()
        {
            return MockService.IncomingOrderResponse_getOrders();
        }

        /// <summary>
        /// Получить информацию о входящем заказе.
        /// </summary>
        /// <returns></returns>
        public async Task<IncomingOrderInfoDto> GetPending()
        {
            return MockService.IncomingOrderResponse_getPending();
        }

        /// <summary>
        /// Получить детали заказа.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<IncomingOrderDetailsDto> Details(int orderId)
        {
            return MockService.IncomingOrderResponse_getDetails();
        }

        /// <summary>
        /// Принять входящий заказ в работу.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<IncomingOrderDto> Accept(int orderId)
        {
            // mocked
            return new IncomingOrderDto()
            {
                Id = 1,
                VendorLogo = "https://admin.kenguru.tj/files/EFVUJtJcuEGrXh7yJrxljw.jpg",
                ClientName = "Азамат",
                Status = "Готовится",
                TotalCost = (decimal)84.02
            };
        }

        /// <summary>
        /// Отклонить входящий заказ.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task Reject(int orderId)
        {
            // mocked
        }
    }
}