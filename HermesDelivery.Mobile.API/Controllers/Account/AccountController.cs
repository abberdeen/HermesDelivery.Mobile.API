using CourierAPI.Infrastructure;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Services.Account;
using System.Threading.Tasks;
using System.Web.Http;
using CourierAPI.DTO;
using CourierAPI.DTO.Account;

namespace CourierAPI.Controllers.Account
{
    /// <summary>
    /// Учетные записи.
    /// </summary>
    [Authorize]
    public class AccountController : ApiControllerExtension
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Смена пароля.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("Account/ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword([FromBody]ChangePasswordDto model)
        { 
            var msg = await _accountService.ChangePasswordAsync(GetCourierId(), model);

            if (msg == AppMessage.Ok)
            {
                return Ok("Password changed");
            }

            return Response(msg);
        }

        /// <summary>
        /// Сброс пароля.
        /// </summary>
        /// <returns></returns>
        [Route("Account/ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword()
        { 
            var msg = await _accountService.ResetPasswordAsync(GetCourierId());

            if (msg == AppMessage.Ok)
            {
                return Ok("Password reset success");
            }

            return Response(msg);
        }
    }
}