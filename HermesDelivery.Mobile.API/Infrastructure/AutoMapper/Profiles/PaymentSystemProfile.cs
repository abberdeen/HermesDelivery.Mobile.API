using AutoMapper;
using HermesDMobAPI.Infrastructure.Database;
using HermesDMobAPI.Models.DTO.PaymentSystems;

namespace HermesDMobAPI.Infrastructure.AutoMapper.Profiles
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