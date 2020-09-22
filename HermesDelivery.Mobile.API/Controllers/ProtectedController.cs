using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace HermesDelivery.Mobile.API.Controllers
{
    [System.Web.Mvc.Authorize]
    [System.Web.Mvc.RoutePrefix("api/protected")]
    public class ProtectedController : ApiController
    {
        [System.Web.Mvc.Route("")]
        public IEnumerable<object> Get()
        {
            var identity = User.Identity as ClaimsIdentity;

            return identity.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            });
        }
    }
}