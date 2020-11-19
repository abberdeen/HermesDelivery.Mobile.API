using System;

namespace CourierAPI.DTO.Sms
{
    // Запрос на оптпраление СМС.
    public class SMSSendRequestDto
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
    }

    // Добавление лога СМС шлюза.
    public class SMSSendLogCreateDto
    {
        public int SMSSettingId { get; set; }
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
        public Nullable<int> HandlerId { get; set; }
        public string SenderIp { get; set; }
    }
}