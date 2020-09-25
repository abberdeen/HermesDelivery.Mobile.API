using AutoMapper;
using HermesDMobAPI.Models.DTO.Sms;

namespace HermesDMobAPI.Infrastructure.AutoMapper.Profiles
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