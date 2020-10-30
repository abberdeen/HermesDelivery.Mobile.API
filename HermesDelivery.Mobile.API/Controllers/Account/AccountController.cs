using CourierAPI.Infrastructure;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Models.DTO.Account;
using CourierAPI.Services.Account;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Web.Http;

namespace CourierAPI.Controllers.Account
{
    [Authorize]
    public class AccountController : ApiControllerExtension
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [Route("Account/ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword([FromBody]ChangePasswordDto model)
        {
            var userId = User.Identity.GetUserId();
            var msg = await _accountService.ChangePasswordAsync(userId, model);

            if (msg == AppMessage.Ok)
            {
                return Ok("Password changed");
            }

            return Response(msg);
        }

        [Route("Account/ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword()
        {
            var userId = User.Identity.GetUserId();
            var msg = await _accountService.ResetPasswordAsync(userId);

            if (msg == AppMessage.Ok)
            {
                return Ok("Password reset success");
            }

            return Response(msg);
        }
    }
}