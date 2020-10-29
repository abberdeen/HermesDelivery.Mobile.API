using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HermesDMobAPI.Models.DTO.PaymentSystems
{
    public class PaymentSystemDto
    {
        public string Name { get; set; }
        public string Logo { get; set; }
        public string Qr { get; set; }
    }
}