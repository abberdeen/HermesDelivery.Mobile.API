using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.PaymentSystems;

namespace CourierAPI.Infrastructure.AutoMapper.Profiles
{
    public class PaymentSystemProfile : Profile
    {
        public PaymentSystemProfile()
        {
            // PaymentSystem.
            CreateMap<PaymentSystem, PaymentSystemDto>();
        }
    }
}