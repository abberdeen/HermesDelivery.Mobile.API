using AutoMapper;
using CourierAPI.DTO.Sms;
using CourierAPI.Infrastructure.Database;

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