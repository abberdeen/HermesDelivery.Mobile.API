using AutoMapper;
using CourierAPI.DTO.PaymentSystem;
using CourierAPI.Infrastructure.Database;

namespace CourierAPI.Infrastructure.AutoMapper.Profiles
{
    public class PaymentSystemProfile : Profile
    {
        public PaymentSystemProfile()
        {
            // PaymentSystem.
            CreateMap<PaymentSystem, PaymentSystemDto>()
                .ForMember(d => d.Qr,
                    m => m.MapFrom(s => s.QRCode));
        }
    }
}