using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CourierAPI.Infrastructure.Extensions
{
    public static class DateTimeExtension
    {
        public static string Format(this DateTime datetime)
        { 
            return datetime.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
    }
}