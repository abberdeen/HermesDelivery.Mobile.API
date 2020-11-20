namespace CourierAPI.DTO.Orders
{
    public class OrderCountDto
    {
        public int Active { get; set; }
        public int Delivered { get; set; }
        public int Rejected { get; set; }
        public int Total { get; set; }
    }

    public class ShiftOrderDto
    {
        public int Id { get; set; }
        public string VendorLogo { get; set; }
        public string ClientName { get; set; }
        public string Status { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class OrderStatusChangeResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}