using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.Customer;
using CourierAPI.Models.DTO.Orders;
using CourierAPI.Models.DTO.Supplier;
using CourierAPI.Services.Account;
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

        public IncomingOrderService(ILogger logger, IMapper mapper, UserService userService, MessageService messageService)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Получить список заказов.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<IncomingOrderDto>> GetList()
        {
            return new List<IncomingOrderDto>()
            {
                new IncomingOrderDto()
                {
                    Id = 1,
                    VendorLogo = "https://admin.kenguru.tj/files/EFVUJtJcuEGrXh7yJrxljw.jpg",
                    ClientName = "Азамат",
                    Status = "Готовится",
                    TotalCost = (decimal)84.02
                }
            };
        }

        /// <summary>
        /// Получить информацию о входящем заказе.
        /// </summary>
        /// <returns></returns>
        public async Task<IncomingOrderInfoDto> GetIncomingOrder()
        {
            return new IncomingOrderInfoDto()
            {
                Id = 24123,
                PickupTime = "15 мин.",
                TotalCost = (decimal)84.02,
                Vendor = new IncomingOrderInfoSupplierDto()
                {
                    Logo = "https://admin.kenguru.tj/files/EFVUJtJcuEGrXh7yJrxljw.jpg",
                    Name = "Vendor name",
                    Address = "Some street, some house, apt. 20"
                },
                Client = new IncomingOrderInfoCustomerDto
                {
                    Name = "Azamat",
                    DeliveryAddress = "Some street, some house, apt. 21"
                }
            };
        }

        /// <summary>
        /// Получить детали заказа.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<IncomingOrderDetailsDto> Details(int orderId)
        {
            return new IncomingOrderDetailsDto()
            {
                Id = 1,
                CreatedAt = "2020-10-10T09:22:00Z",
                TotalCost = 123.00,
                DeliveryCost = 12.00,
                Comment = "Быстрее,  ",
                NextStatus = new NextStatus()
                {
                    Id = 2,
                    Name = "Next status"
                }, // null, если нельзя сменить статус (заказ закрыт)
                Vendor = new IncomingOrderDetailsSupplierDto()
                {
                    Logo = "https://admin.kenguru.tj/files/EFVUJtJcuEGrXh7yJrxljw.jpg",
                    Banner = "https://admin.kenguru.tj/files/F7MGOiizjEqVMBbPiqqR5Q.jpg",
                    Name = "28 Monkeys",
                    Address = "пр-т И. Сомони, д. 62"
                },
                Client = new IncomingOrderDetailsCustomerDto()
                {
                    Name = "Азамат",
                    Phone = "934114400",
                    DeliveryAddress = "ул. Валоматзаде, д. 32, к. 33"
                },
                Products = new List<ProductDto>() {
                    new ProductDto()   {
                        Name =  "Колбаски из баранины с картофельными шариками",
                        Price =  (decimal) 12.01,
                        Amount =  1
                    }
                }
            };
        }

        /// <summary>
        /// Принять входящий заказ в работу.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<IncomingOrderDto> Accept(int orderId)
        {
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
        }
    }
}