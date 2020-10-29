using AutoMapper;
using HermesDMobAPI.Infrastructure.Database;
using HermesDMobAPI.Models.DTO.Sms;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.Logging;

namespace HermesDMobAPI.Services.Sms
{
    public class SmsSettingService
    {
        private readonly DatabaseContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public SmsSettingService(ILogger logger, IMapper mapper)
        {
            _dbContext = new DatabaseContext();
            _logger = logger;
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