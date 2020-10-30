using AutoMapper;
using CourierAPI.Infrastructure.AutoMapper.Profiles;

namespace CourierAPI.Infrastructure.AutoMapper
{
    public class ConfigurationManager
    {
        public static MapperConfiguration CreateConfiguration()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SmsProfile>();
                cfg.AddProfile<PaymentSystemProfile>();
            });
            return config;
        }
    }
}