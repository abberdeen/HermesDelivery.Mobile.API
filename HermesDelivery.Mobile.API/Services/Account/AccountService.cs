using AutoMapper;
using CourierAPI.Infrastructure;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Services.Sms;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using CourierAPI.DTO;
using CourierAPI.DTO.Account;

namespace CourierAPI.Services.Account
{
    [Authorize]
    public class AccountService
    {
        private AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly MessageService _messageService;
        private readonly PasswordHasher _passwordHasher = new PasswordHasher();

        public AccountService(ILogger logger, IMapper mapper, MessageService messageService)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
            _messageService = messageService;
        }

        public async Task<AppMessage> ChangePasswordAsync(int courierId, ChangePasswordDto model)
        {
            // Получает модель пользователя по ИД.
            var courier = await _dbContext.Couriers.FirstOrDefaultAsync(x => x.Id == courierId);

            // Если пользователь не найден, то завершает работу.
            if (courier == null)
            {
                return AppMessage.InvalidUsername;
            }

            // Сверяет старый пароль с новым.
            var verifyResult = _passwordHasher.VerifyHashedPassword(courier.PasswordHash, model.CurrentPassword);
            if (verifyResult != PasswordVerificationResult.Success)
            {
                return AppMessage.InvalidPassword;
            }

            // Проверяет корректность нового пароля.
            if (model.CurrentPassword == model.NewPassword || string.IsNullOrEmpty(model.NewPassword))
            {
                return AppMessage.InvalidPassword;
            }

            // Обновляет пароль на новый.
            courier.PasswordHash = _passwordHasher.HashPassword(model.NewPassword);

            // Отправляет запрос на сохранение изменений.
            await _dbContext.SaveChangesAsync();
            return AppMessage.Ok;
        }

        public async Task<AppMessage> ResetPasswordAsync(int courierId)
        {
            var currentUser = await _dbContext.Couriers.Where(e => e.Id == courierId).FirstOrDefaultAsync();
            if (currentUser == null)
            {
                return AppMessage.InvalidUsername;
            }

            var random = new Random();
            var newPassword = random.Next(11111, 99999);

            currentUser.PasswordHash = new PasswordHasher().HashPassword(newPassword.ToString());

            await _dbContext.SaveChangesAsync();

            var isSuccess = await _messageService.SendMessage(currentUser.Phone,
                "Вы сбросили свой пароль. Ваш новый пароль: " + newPassword);

            if (isSuccess)
            {
                _logger.Information("AccountSrv: Password for courier " + courierId + " reset-ed");
                return AppMessage.Ok;
            }

            _logger.Fatal("AccountSrv: " + AppMessage.SmsServiceError);

            return AppMessage.SmsServiceError;
        }
    }
}