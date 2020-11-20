using AutoMapper;
using CourierAPI.DTO;
using CourierAPI.DTO.Supplier;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Infrastructure.Exceptions;
using CourierAPI.Services.Content;
using Serilog;
using System.Data.Entity;
using System.Threading.Tasks;

namespace CourierAPI.Services.Order
{
    public class SupplierInfoService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public SupplierInfoService(ILogger logger, IMapper mapper)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public async Task<SupplierInfoDto> GetSupplierInfo(int orderId)
        {
            var supplierType = await GetSupplierTypeAsync(orderId);
            switch (supplierType)
            {
                case SupplierType.Store:
                    return await GetStoreInfo(orderId);

                case SupplierType.Restaurant:
                    return await GetRestaurantInfo(orderId);

                case SupplierType.Undefined:
                default: 
                    throw new AppException(AppMessage.UndefinedSupplier); 
            }
        }

        /// <summary>
        ///
        /// </summary>
        public enum SupplierType
        {
            /// <summary>
            ///
            /// </summary>
            Undefined = 0,

            /// <summary>
            ///
            /// </summary>
            Store = 1,

            /// <summary>
            ///
            /// </summary>
            Restaurant = 2
        }

        public async Task<SupplierType> GetSupplierTypeAsync(int orderId)
        {
            //
            var isRestaurant = await _dbContext.RestaurantMenuItem_Order.AnyAsync(x => x.OrderId == orderId);
            if (isRestaurant)
            {
                return SupplierType.Restaurant;
            }

            //
            var isStore = await _dbContext.StoreProduct_Order.AnyAsync(x => x.OrderId == orderId);
            if (isStore)
            {
                return SupplierType.Store;
            }

            return SupplierType.Undefined;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<SupplierInfoDto> GetRestaurantInfo(int orderId)
        {
            var restaurant = (await _dbContext.RestaurantMenuItem_Order
                .FirstOrDefaultAsync(x => x.OrderId == orderId))
                ?.RestaurantMenuItem.RestaurantMenu.Restaurant;

            if (restaurant is null)
            {
                throw new AppException(AppMessage.CantFindRestaurant);
            }

            var restaurantInfo = new SupplierInfoDto
            {
                Logo = FileService.GetImageUrl(restaurant.Logo),
                Banner = FileService.GetImageUrl(restaurant.Banner),
                Name = restaurant.Name,
                Address = restaurant.Name
            };
            return restaurantInfo;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<SupplierInfoDto> GetStoreInfo(int orderId)
        {
            var store = (await _dbContext.StoreProduct_Order
                    .FirstOrDefaultAsync(x => x.OrderId == orderId))
                ?.StoreProduct.Store;

            if (store is null)
            {
                throw new AppException(AppMessage.CantFindStore);
            }

            var storeInfo = new SupplierInfoDto
            {
                Logo = FileService.GetImageUrl(store.Logo),
                Banner = FileService.GetImageUrl(store.Banner),
                Name = store.Name,
                Address = store.Name
            };
            return storeInfo;
        }
    }
}