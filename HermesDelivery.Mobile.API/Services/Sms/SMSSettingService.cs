using AutoMapper;
using HermesDMobAPI.Infrastructure;
using HermesDMobAPI.Models.DTO.Sms;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HermesDMobAPI.Services.Sms
{
    public class SmsSettingService
    {
        private HDEntities _dbContext = new HDEntities();
        private readonly IMapper _mapper;

        public SmsSettingService(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Возвращает один активный элемент.
        /// </summary>
        /// <returns></returns>
        public async Task<SMSSettingsListItemDto> GetFirstActive()
        {
            // Получаем все позиции.
            var smsSettings = await _dbContext.SMSSettings.Where(e => e.IsActive == true).OrderBy(x => x.Id).FirstAsync();

            // Передаем выходную модель.
            return _mapper.Map<SMSSettingsListItemDto>(smsSettings);
        }
    }
}