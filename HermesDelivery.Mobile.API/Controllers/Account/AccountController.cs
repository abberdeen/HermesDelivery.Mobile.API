using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http; 
using HermesDMobAPI.Infrastructure;
using HermesDMobAPI.Infrastructure.Extensions;
using HermesDMobAPI.Models.DTO;
using HermesDMobAPI.Models.DTO.OAuth;
using HermesDMobAPI.Services.Account;
using Microsoft.AspNet.Identity;

namespace HermesDMobAPI.Controllers.Account
{
    [Authorize]
    public class AccountController : ApiControllerExtension
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword([FromBody]ChangePasswordDto model)
        {
            var userId = User.Identity.GetUserId();
            var msg = await _accountService.ChangePasswordAsync(userId, model);
            
            if (msg == AppMessage.Ok)
            {
                return Ok("Password changed");
            }

            return BadRequest(msg);
        }

        [Route("ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword()
        {
            var userId = User.Identity.GetUserId();
            var msg = await _accountService.ResetPasswordAsync(userId);

            if (msg == AppMessage.Ok)
            {
                return Ok("Password reset success");
            }

            return BadRequest(msg);
        }

    }
}
