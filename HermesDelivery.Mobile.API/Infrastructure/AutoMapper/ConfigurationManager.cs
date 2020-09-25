using AutoMapper;
using HermesDMobAPI.Infrastructure.AutoMapper.Profiles;

namespace HermesDMobAPI.Infrastructure.AutoMapper
{
    public class ConfigurationManager
    {
        public static MapperConfiguration CreateConfiguration()
        {
            var config = new MapperConfiguration(cfg => { 
                 cfg.AddProfile<SmsProfile>();
            });
            return config;
        }
    }
}