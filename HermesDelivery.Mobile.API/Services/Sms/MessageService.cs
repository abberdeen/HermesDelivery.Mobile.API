using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Services.Sms.External;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Web;
using CourierAPI.DTO.Sms;

namespace CourierAPI.Services.Sms
{
    public class MessageService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly SmsSettingService _smsSettingService;
        private readonly SmsSendLogService _smsSendLogService;

        public MessageService(ILogger logger, IMapper mapper, SmsSettingService smsSettingService, SmsSendLogService smsSendLogService)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
            _smsSettingService = smsSettingService;
            _smsSendLogService = smsSendLogService;
        }

        public async Task<bool> SendMessage(string phoneNumber, string message)
        {
            // Получаем конфигурацию СМС шлюза.
            var settings = await _smsSettingService.GetFirstActive();

            if (settings == null)
            {
                return false;
            }

            // Подготавливаем СМС шлюз.
            SmsService sender = new OsonSmsService(new Uri(settings.BaseUrl), settings.Sender, settings.Login, settings.PassHash);

            // Отправляем сообщение через СМС шлюз.
            var response = await sender.SendMessage(phoneNumber, message);

            // Просто получаем код ответа запроса.
            // TODO: Что если выдала ошибку?
            var statusCode = 0;
            if (response != null)
            {
                statusCode = 200;
                if (response["error"]?["code"] != null)
                {
                    statusCode = int.Parse(response["error"]["code"].ToString());
                    _logger.Error($"SmsService : Service returned exception code {statusCode} for {phoneNumber}");
                }
                else
                {
                    _logger.Information($"SmsService : Message for {phoneNumber} accepted by service");
                }
            }

            // Записываем данные в лог с указанием статуса отправки.
            var log = new SMSSendLogCreateDto
            {
                Message = message,
                PhoneNumber = phoneNumber,
                SMSSettingId = settings.Id,
                Status = statusCode,
                HandlerId = (int)SMSHandlers.HermesDevlieryAPI_Undefined,
                SenderIp = HttpContext.Current.Request.UserHostAddress
            };
            await _smsSendLogService.Create(log);

            return true;
        }
    }
}