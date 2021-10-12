using AutoMapper;
using CourierAPI.Infrastructure.AutoMapper.Profiles;

namespace CourierAPI.Infrastructure.AutoMapper
{
    /// <summary>
    ///
    /// </summary>
    public class AutoMapperConfigurationManager
    {
        public static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                // SMS
                cfg.AddProfile<SmsProfile>();

                // Payment systems
                cfg.AddProfile<PaymentSystemProfile>();

                // Shifts
                cfg.AddProfile<CourierShiftProfile>();
                cfg.AddProfile<CourierShiftHistoryProfile>();
                cfg.AddProfile<ShiftPauseReasonProfile>();

                // Orders
                cfg.AddProfile<SupplierInfoProfile>();
                cfg.AddProfile<OrderInfoProfile>();
            });
            return config.CreateMapper();
        }
    }
}