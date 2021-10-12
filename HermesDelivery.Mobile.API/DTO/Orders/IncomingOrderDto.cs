using System.Collections.Generic;
using CourierAPI.DTO.Customer;
using CourierAPI.DTO.Supplier;

namespace CourierAPI.DTO.Orders
{
    /// <summary>
    /// Краткая информация о заказе.
    /// </summary>
    public class IncomingOrderDto
    {
        public int Id { get; set; }
        public string VendorLogo { get; set; }
        public string ClientName { get; set; }
        public string Status { get; set; }
        public decimal TotalCost { get; set; }
    }

    /// <summary>
    /// Ответ для: Проверить наличие входящего заказа
    /// </summary>
    public class IncomingOrderInfoDto
    {
        public int Id { get; set; }
        public string PickupTime { get; set; }
        public decimal TotalCost { get; set; }
        public IncomingOrderInfoSupplierDto Vendor { get; set; }
        public IncomingOrderInfoCustomerDto Client { get; set; }
    }

    /// <summary>
    /// Ответ для: Получить детали заказа
    /// </summary>
    public class IncomingOrderDetailsDto
    {
        public int Id { get; set; }
        public string CreatedAt { get; set; }
        public decimal TotalCost { get; set; }
        public decimal DeliveryCost { get; set; }
        public string Comment { get; set; }
        public NextStatus NextStatus { get; set; }
        public IncomingOrderDetailsSupplierDto Vendor { get; set; }
        public IncomingOrderDetailsCustomerDto Client { get; set; }
        public List<ProductDto> Products { get; set; }
    }

    public class NextStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}