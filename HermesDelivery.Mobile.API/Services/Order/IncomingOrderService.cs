using AutoMapper;
using CourierAPI.DTO;
using CourierAPI.DTO.Customer;
using CourierAPI.DTO.Orders;
using CourierAPI.DTO.Supplier;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Infrastructure.Exceptions;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Services.Shift;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using CourierAPI.Services.ThorWebSocket;
using OrderStatusCode = HermesDAdmin.Dto.Orders.OrderStatusCode;

namespace CourierAPI.Services.Order
{
    [Authorize]
    public class IncomingOrderService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly SupplierInfoService _supplierInfoService;
        private readonly OrderInfoService _orderInfoSrv;
        private readonly CourierShiftHistoryService _courierShiftHistoryService;

        public IncomingOrderService(
            ILogger logger,
            IMapper mapper,
            SupplierInfoService supplierInfoService,
            OrderInfoService orderInfoSrv,
            CourierShiftHistoryService courierShiftHistoryService)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
            _supplierInfoService = supplierInfoService;
            _orderInfoSrv = orderInfoSrv;
            _courierShiftHistoryService = courierShiftHistoryService;
        }

        /// <summary>
        /// Получить список заказов текущей смены.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<IncomingOrderDto>> GetOrders(int courierId)
        {
            // Выполняет запрос на получение записи элемента текущей смены.
            var courierShiftHistory = await _courierShiftHistoryService.FindCurrentOrNextAsync(courierId);

            return await GetIncomingOrderList(courierId, courierShiftHistory.Id);
        }

        public async Task<List<IncomingOrderDto>> GetIncomingOrderList(int courierId, int courierShiftHistoryId)
        {
            var orders = _dbContext.Orders.Where(x =>
                x.CourierId == courierId &&
                x.CourierShiftHistoryId == courierShiftHistoryId &&
                x.OrderStatusCode.IsFinal == true);

            var shiftOrderList = new List<IncomingOrderDto>();
            foreach (var order in orders)
            {
                var supplierInfo = await _supplierInfoService.GetSupplierInfo(order.Id);

                var orderInfo = await _orderInfoSrv.GetOrderInfoAsync(order.Id);
                var item = new IncomingOrderDto
                {
                    Id = order.Id,
                    VendorLogo = supplierInfo.Logo,
                    ClientName = order.Customer.Name,
                    Status = order.OrderStatusCode.Text,
                    TotalCost = orderInfo.SubTotal
                };
                shiftOrderList.Add(item);
            }

            return shiftOrderList;
        }

        /// <summary>
        /// Получить информацию о входящем заказе.
        /// </summary>
        /// <returns></returns>
        public async Task<IncomingOrderInfoDto> GetPendingAsync(int courierId)
        {
            // 
            var pendingOrder = await _dbContext.Orders
                .Select(x => new { x.Id, x.CourierId, x.OrderStatusCode, x.CourierShiftHistoryId })
                .Where(x =>
                    x.CourierId == courierId &&
                    x.OrderStatusCode.IsFinal == false &&
                    x.CourierShiftHistoryId.HasValue == false)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (pendingOrder is null)
            {
                return null;
            }

            return await GetOrderInfoAsync(pendingOrder.Id);
        }

        /// <summary>
        /// Получить информацию о входящем заказе.
        /// </summary>
        /// <returns></returns>
        public async Task<IncomingOrderInfoDto> GetOrderIfPendingAsync(int courierId, int orderId)
        {
            // 
            var pendingOrder = await _dbContext.Orders
                .Select(x => new { x.Id, x.CourierId, x.OrderStatusCode, x.CourierShiftHistoryId })
                .Where(x =>
                    x.Id == orderId && 
                    x.CourierId == courierId &&
                    x.OrderStatusCode.IsFinal == false &&
                    x.CourierShiftHistoryId.HasValue == false)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (pendingOrder is null)
            {
                return null;
            }

            return await GetOrderInfoAsync(pendingOrder.Id);
        }

        /// <summary>
        /// Принять входящий заказ в работу.
        /// </summary>
        /// <param name="courierId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<IncomingOrderDto> AcceptAsync(int courierId, int orderId)
        {
            var pendingOrder = await GetOrderIfPendingAsync(courierId, orderId);

            if (pendingOrder == null)
            {
                throw new AppException(new AppMessage(0, "Accept: Undefined error", HttpStatusCode.BadRequest));
            }

            var courierShiftHistory = await _courierShiftHistoryService.FindCurrentOrNextAsync(courierId);

            if (courierShiftHistory is null)
            {
                throw new AppException(AppMessage.UndefinedCourierShiftHistory);
            }

            //
            var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId);

            if (order.OrderStatusCode.IsFinal)
            {
                throw new AppException(AppMessage.OrderStatusIsCompleted);
            }

            order.CourierShiftHistoryId = courierShiftHistory.Id;
            order.OrderStatusCodeId = (int)OrderStatusCode.CourierAccepted;

            var incomingOrder = new IncomingOrderHistory()
            {
                CourierShiftHistoryId = courierShiftHistory.Id,
                OrderId = orderId,
                StatusId = (int)IncomingOrderStatuses.Accepted,
                UpdatedAt = DateTime.Now,
            };
            _dbContext.IncomingOrderHistories.Add(incomingOrder);

            await _dbContext.SaveChangesAsync();

            await WebSocketService.ShiftChanged(courierId);
             
            // mocked
            return new IncomingOrderDto()
            {
                Id = orderId,
                VendorLogo = pendingOrder.Vendor.Logo,
                ClientName = pendingOrder.Client.Name,
                Status = order.OrderStatusCode.Text,
                TotalCost = pendingOrder.TotalCost
            };
        }

        /// <summary>
        /// Отклонить входящий заказ.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task RejectAsync(int courierId, int orderId)
        {  
            var courierShiftHistory = await _courierShiftHistoryService.FindCurrentOrNextAsync(courierId);

            if (courierShiftHistory is null)
            {
                throw new AppException(AppMessage.UndefinedCourierShiftHistory);
            }

            //
            var order = await _dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId);

            if (order.OrderStatusCode.IsFinal)
            {
                throw new AppException(AppMessage.OrderStatusIsCompleted);
            }

            order.CourierShiftHistoryId = null;
            order.CourierId = null;
            order.OrderStatusCodeId = (int)OrderStatusCode.CourierSelected;

            var incomingOrder = new IncomingOrderHistory()
            {
                CourierShiftHistoryId = courierShiftHistory.Id,
                OrderId = orderId,
                StatusId = (int)IncomingOrderStatuses.Rejected,
                UpdatedAt = DateTime.Now,
            };
            _dbContext.IncomingOrderHistories.Add(incomingOrder);

            await _dbContext.SaveChangesAsync();

            await WebSocketService.ShiftChanged(courierId);
        }

        /// <summary>
        /// Получить детали заказа.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<IncomingOrderDetailsDto> GetDetailsAsync(int orderId)
        {
            var order =await _dbContext.Orders
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order is null)
            {
                throw new AppException(AppMessage.OrderNotExists);
            }
            
            // 
            var orderStatusCodeId = order.OrderStatusCodeId;
            var nextOrderStatusCode = await GetOrderStatusCodeAsync(orderStatusCodeId + 1);

            NextStatus nextStatus = null;
            if (orderStatusCodeId == (int)OrderStatusCode.CourierAccepted ||
                orderStatusCodeId == (int)OrderStatusCode.Prepared)
            {
                nextStatus = new NextStatus
                {
                    Id = nextOrderStatusCode.Id,
                    Name = nextOrderStatusCode.Text
                };
            } 

            var supplierInfo = await _supplierInfoService.GetSupplierInfo(orderId);

            var orderInfo = await _orderInfoSrv.GetOrderInfoAsync(orderId);
            
            var orderDetails = new IncomingOrderDetailsDto
            {
                Id = orderId,
                CreatedAt = order.CreatedAt.Format(),
                TotalCost = orderInfo.SubTotal,
                DeliveryCost = orderInfo.DeliveryCost,
                Comment = order.DeliveryCustomerNote,
                NextStatus = nextStatus,
                Vendor = _mapper.Map<IncomingOrderDetailsSupplierDto>(supplierInfo),
                Client = new IncomingOrderDetailsCustomerDto
                {
                    Name = order.Customer.Name,
                    Phone = order.Customer.PhoneNumber,
                    DeliveryAddress = order.DeliveryAddress
                },
                Products = _mapper.Map<List<ProductDto>>(orderInfo.OrderItems)
            };
            return orderDetails;
        }

        public async Task<NextStatus> UpdateStatus(int orderId, int statusId)
        {
            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order is null)
            {
                throw new AppException(AppMessage.OrderNotExists);
            }

            int currentOrderStatusCodeId = order.OrderStatusCodeId;
            int nextOrderStatusCodeId = 0;

            if (order.OrderStatusCodeId ==(int) OrderStatusCode.CourierAccepted)
            {
                currentOrderStatusCodeId = (int) OrderStatusCode.Prepared;
                nextOrderStatusCodeId = (int)OrderStatusCode.Delivered;
            }
            else if(order.OrderStatusCodeId == (int)OrderStatusCode.Prepared)
            {
                currentOrderStatusCodeId = (int)OrderStatusCode.Delivered;
                nextOrderStatusCodeId = -1;
            }

            order.OrderStatusCodeId = currentOrderStatusCodeId;
            await _dbContext.SaveChangesAsync();

            var nextOrderStatusCode = await GetOrderStatusCodeAsync(nextOrderStatusCodeId);

            if (nextOrderStatusCode is null)
            {
                return null;
            }

            var nextStatus = new NextStatus
            {
                Id = nextOrderStatusCode.Id,
                Name = nextOrderStatusCode.Text
            };

            await WebSocketService.ShiftChanged((int)order.CourierId);

            return nextStatus;
        }

        private async Task<Infrastructure.Database.OrderStatusCode> GetOrderStatusCodeAsync(int orderStatusCodeId)
        {
            return await _dbContext.OrderStatusCodes.FirstOrDefaultAsync(x=>x.Id == orderStatusCodeId && x.IsFinal == false);
        }

        private async Task<IncomingOrderInfoDto> GetOrderInfoAsync(int orderId)
        {
            var order = await _dbContext.Orders
                .Where(x => x.Id == orderId)
                .FirstOrDefaultAsync();

            if (order is null)
            {
                return new IncomingOrderInfoDto();
            }

            var supplierInfo = await _supplierInfoService.GetSupplierInfo(orderId);

            var orderInfo = await _orderInfoSrv.GetOrderInfoAsync(orderId);

            var pendingOrderDto = new IncomingOrderInfoDto
            {
                //
                Id = orderId,
                //
                PickupTime = FormatPickupTime(order.SupplierOrderPreparationTime),
                //
                TotalCost = orderInfo.Total,
                //
                Vendor = _mapper.Map<IncomingOrderInfoSupplierDto>(supplierInfo),
                //
                Client = new IncomingOrderInfoCustomerDto
                {
                    Name = order.Customer.Name,
                    DeliveryAddress = order.DeliveryAddress
                }
            };
            return pendingOrderDto;
        }

        private static string FormatPickupTime(int? time)
        {
            return time.HasValue
                ? $"{time?.ToString()} мин."
                : "неизвестно когда.";
        }
    }
}