using AutoMapper;
using CourierAPI.DTO;
using CourierAPI.DTO.Orders;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Infrastructure.Exceptions;
using CourierAPI.Services.Content;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourierAPI.Services.Order
{
    /// <summary>
    ///
    /// </summary>
    public class OrderInfoService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly SupplierInfoService _supplierInfoService;

        public OrderInfoService(ILogger logger, IMapper mapper, SupplierInfoService supplierInfoService)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
            _supplierInfoService = supplierInfoService;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<OrderDetailsDto> GetOrderInfoAsync(int orderId)
        {
            var orderDetails = new OrderDetailsDto { OrderId = orderId };

            var supplierType = await _supplierInfoService.GetSupplierTypeAsync(orderId);

            switch (supplierType)
            {
                case SupplierInfoService.SupplierType.Restaurant:
                    orderDetails.OrderItems = GetRestaurantMenusItemsList(orderId);

                    var restaurantQuery = _dbContext.RestaurantMenuItem_Order
                        .FirstOrDefault(x => x.OrderId == orderId);

                    orderDetails.SupplierId = restaurantQuery?.RestaurantMenuItem.RestaurantMenu.RestaurantId;
                    break;

                case SupplierInfoService.SupplierType.Store:
                    orderDetails.OrderItems = GetStoreProductsList(orderId);

                    var storeQuery = _dbContext.StoreProduct_Order
                        .FirstOrDefault(x => x.OrderId == orderId);

                    orderDetails.SupplierId = storeQuery?.StoreProduct.StoreId;
                    break;

                case SupplierInfoService.SupplierType.Undefined:
                default:
                    throw new AppException(AppMessage.UndefinedSupplier);
            }

            orderDetails.SubTotal = orderDetails.OrderItems.Sum(x => x.SubTotal);
            orderDetails.DeliveryCost = _dbContext.Orders.FirstOrDefault(x => x.Id == orderId)?.DeliveryCost ?? 0;
            orderDetails.Total = orderDetails.SubTotal + orderDetails.DeliveryCost;

            return orderDetails;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private List<OrderSupplierItemDto> GetRestaurantMenusItemsList(int orderId)
        {
             
            var orderDetails = _dbContext.RestaurantMenuItem_Order
                    .Where(x => x.OrderId == orderId)
                    .Select(x => new OrderSupplierItemDto
                    {
                        Id = x.Id,
                        Image = x.RestaurantMenuItem.Image,
                        Name = x.RestaurantMenuItem.Name,
                        Description = x.RestaurantMenuItem.Description,
                        Amount = x.Quantity,
                        Price = x.UnitPrice,
                        SubTotal = x.Quantity * x.UnitPrice
                    })
                    .ToList();

            orderDetails.ForEach(x => x.Image = FileService.GetImageUrl(x.Image));

            // Передаем выходную модель.
            return orderDetails;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private List<OrderSupplierItemDto> GetStoreProductsList(int orderId)
        { 
            var orderDetails = _dbContext.StoreProduct_Order
                .Where(x => x.OrderId == orderId)
                .Select(x => new OrderSupplierItemDto
                {
                    Id = x.Id,
                    Image = FileService.GetImageUrl(x.StoreProduct.Image),
                    Name = x.StoreProduct.Name,
                    Description = x.StoreProduct.Description,
                    Amount = x.Quantity,
                    Price = x.UnitPrice,
                    SubTotal = x.Quantity * x.UnitPrice
                })
                .ToList();

            orderDetails.ForEach(x => x.Image = FileService.GetImageUrl(x.Image));
             
            return orderDetails;
        }
    }
}