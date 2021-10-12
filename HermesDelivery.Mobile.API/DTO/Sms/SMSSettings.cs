namespace CourierAPI.DTO.Sms
{
    // Элемент списка.
    public class SMSSettingsListItemDto
    {
        public int Id { get; set; }
        public string BaseUrl { get; set; }
        public string Login { get; set; }
        public string PassHash { get; set; }
        public string Sender { get; set; }
        public bool IsActive { get; set; }
    }
}