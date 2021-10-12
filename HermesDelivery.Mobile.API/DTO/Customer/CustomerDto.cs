namespace CourierAPI.DTO.Customer
{
    public class IncomingOrderInfoCustomerDto
    {
        public string Name { get; set; }
        public string DeliveryAddress { get; set; }
    }

    public class IncomingOrderDetailsCustomerDto
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string DeliveryAddress { get; set; }
    }
}