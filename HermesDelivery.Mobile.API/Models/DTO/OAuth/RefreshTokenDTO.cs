using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HermesDelivery.Mobile.API.Models.DTO.OAuth
{
    public class RefreshTokenDTO
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public string RemoteIp{ get; set; }
        public bool IsActive { get; set; }

    }
}