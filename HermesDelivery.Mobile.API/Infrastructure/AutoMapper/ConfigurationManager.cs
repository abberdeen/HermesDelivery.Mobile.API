using AutoMapper;
using CourierAPI.Infrastructure.AutoMapper.Profiles;

namespace CourierAPI.Infrastructure.AutoMapper
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfigurationManager
    {
        public static MapperConfiguration CreateConfiguration()
        {
            var config = new MapperConfiguration(cfg =>
            {
                // SMS 
                cfg.AddProfile<SmsProfile>();

                // Payment systems
                cfg.AddProfile<PaymentSystemProfile>();

                // Work shifts
                cfg.AddProfile<CourierShiftProfile>();
                cfg.AddProfile<CourierShiftHistoryProfile>();
                cfg.AddProfile<WorkShiftPauseReasonProfile>();
            });
            return config;
        }
    }
}