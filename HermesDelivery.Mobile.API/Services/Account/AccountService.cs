using System;
using AutoMapper;
using HermesDMobAPI.Infrastructure;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using HermesDMobAPI.Models.DTO;
using HermesDMobAPI.Models.DTO.OAuth;
using HermesDMobAPI.Services.Sms;
using Microsoft.AspNet.Identity;

namespace HermesDMobAPI.Services.Account
{
    [Authorize]
    public class AccountService
    {
        private readonly HDEntities _dbContext = new HDEntities();
        private readonly IMapper _mapper;
        private readonly UserService _userService; 
        private readonly MessageService _messageService;
        private readonly PasswordHasher _passwordHasher = new PasswordHasher();

        public AccountService(IMapper mapper, UserService userService, MessageService messageService)
        {
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