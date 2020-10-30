using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.Sms;

namespace CourierAPI.Infrastructure.AutoMapper.Profiles
{
    public class SmsProfile : Profile
    {
        public SmsProfile()
        {
            // SMS Service.
            CreateMap<SMSSetting, SMSSettingsListItemDto>();
            CreateMap<SMSSendLog, SMSSendLogCreateDto>().ReverseMap();
        }
    }
}