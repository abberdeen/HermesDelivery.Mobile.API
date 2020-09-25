using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using HermesDMobAPI.Infrastructure;
using HermesDMobAPI.Models.DTO.OAuth;
using HermesDMobAPI.Services.Account;
using Microsoft.AspNet.Identity;

namespace HermesDMobAPI.Controllers.Account
{
    public class AccountController : ApiController
    {
        private AccountService _accountService;
        public AccountController()
        {
            _accountService = new AccountService();
        }
        public async Task<IHttpActionResult> ChangePassword([FromBody]ChangePasswordDto model)
        {
            var userId = User.Identity.GetUserId();
            var isSuccess = await _accountService.ChangePasswordAsync(userId, model);
            
            if (isSuccess)
            {
                return Ok("Password changed");
            }
            return BadRequest("Uncaught error");
        }

    }
}
