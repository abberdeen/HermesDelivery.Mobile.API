using System.Collections.Generic;
using CourierAPI.Services.Order;

namespace CourierAPI.DTO.Orders
{
    /// <summary>
    ///
    /// </summary>
    public class OrderDetailsDto
    {
        public int OrderId { get; set; }
        public decimal Total { get; set; }
        public decimal DeliveryCost { get; set; }
        public decimal SubTotal { get; set; } 
        public List<OrderSupplierItemDto> OrderItems { get; set; } 
        public int? SupplierId { get; set; } 
        public SupplierInfoService.SupplierType SupplierType { get; set; }
    }

    public class OrderSupplierItemDto
    {
       public int Id { get; set; }
       public string Image { get; set; }
       public string Name { get; set; }
       public string Description { get; set; }
       public int Amount { get; set; }
       public decimal Price { get; set; }
       public decimal SubTotal { get; set; }
    }
}