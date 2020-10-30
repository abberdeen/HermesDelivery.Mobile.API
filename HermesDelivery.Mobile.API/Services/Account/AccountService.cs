using AutoMapper;
using CourierAPI.Infrastructure;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.Account;
using CourierAPI.Services.Sms;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CourierAPI.Services.Account
{
    [Authorize]
    public class AccountService
    {
        private AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly UserService _userService;
        private readonly MessageService _messageService;
        private readonly PasswordHasher _passwordHasher = new PasswordHasher();

        public AccountService(ILogger logger, IMapper mapper, UserService userService, MessageService messageService)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
            _messageService = messageService;
            _userService = userService;
        }

        public async Task<AppMessage> ChangePasswordAsync(string userId, ChangePasswordDto model)
        {
            // Получает модель пользователя по ИД.
            var user = await _dbContext.AspNetUsers.Where(x => x.Id == userId).FirstOrDefaultAsync();

            // Если пользователь не найден, то завершает работу.
            if (user == null)
            {
                return AppMessage.InvalidUsername;
            }

            // Сверяет старый пароль с новым.
            var verifyResult = _passwordHasher.VerifyHashedPassword(user.PasswordHash, model.CurrentPassword);
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
            user.PasswordHash = _passwordHasher.HashPassword(model.NewPassword);

            // Отправляет запрос на сохранение изменений.
            await _dbContext.SaveChangesAsync();
            return AppMessage.Ok;
        }

        public async Task<AppMessage> ResetPasswordAsync(string userId)
        {
            var currentUser = await _dbContext.AspNetUsers.Where(e => e.Id == userId).FirstOrDefaultAsync();
            if (currentUser == null)
            {
                return AppMessage.InvalidUsername;
            }

            var random = new Random();
            var newPassword = random.Next(11111, 99999);

            currentUser.PasswordHash = new PasswordHasher().HashPassword(newPassword.ToString());

            await _dbContext.SaveChangesAsync();

            var isSuccess = await _messageService.SendMessage(currentUser.PhoneNumber,
                "Вы сбросили свой пароль. Ваш новый пароль: " + newPassword);

            if (isSuccess)
            {
                return AppMessage.Ok;
            }

            return AppMessage.SmsServiceError;
        }
    }
}