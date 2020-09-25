using AutoMapper;
using HermesDMobAPI.Infrastructure;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using HermesDMobAPI.Models.DTO.OAuth;
using Microsoft.AspNet.Identity;

namespace HermesDMobAPI.Services.Account
{
    [Authorize]
    public class AccountService
    {
        private HDEntities _dbContext = new HDEntities();
        private readonly IMapper _mapper;
        private readonly UserService _userService;
        private readonly PasswordHasher _passwordHasher;
        public AccountService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public AccountService()
        { 
            _passwordHasher = new PasswordHasher();
            _userService = new UserService();
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto model)
        {
            // Получает модель пользователя по ИД.
            var user = await _dbContext.AspNetUsers.Where(x => x.Id == userId).FirstOrDefaultAsync();

            // Если пользователь не найден, то завершает работу.
            if (user == null)
            {
                return false;
            }
          
            // Сверяет старый пароль с новым.
            var verifyResult = _passwordHasher.VerifyHashedPassword(user.PasswordHash, model.CurrentPassword);
            if (verifyResult != PasswordVerificationResult.Success)
            {
                return false;
            }

            // Проверяет корректность нового пароля.
            if (model.CurrentPassword == model.NewPassword || string.IsNullOrEmpty(model.NewPassword))
            {
                return false;
            }

            // Обновляет пароль на новый.
            user.PasswordHash = _passwordHasher.HashPassword(model.NewPassword);

            // Отправляет запрос на сохранение изменений.
            return await _dbContext.SaveChangesAsync() > 0;
        } 
    }
}