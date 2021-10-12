using System;

namespace CourierAPI.DTO.OAuth
{
    public class RefreshTokenDto
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public string RemoteIp { get; set; }
        public bool IsActive { get; set; }
    }
}