namespace CourierAPI.DTO.Supplier
{
    public class SupplierInfoDto
    {
        public string Logo { get; set; }
        public string Banner { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
    
    public class IncomingOrderInfoSupplierDto
    {
        public string Logo { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class IncomingOrderDetailsSupplierDto
    {
        public string Logo { get; set; }
        public string Banner { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
}