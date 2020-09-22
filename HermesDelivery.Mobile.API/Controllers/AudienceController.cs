using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using HermesDelivery.Mobile.API.App_Extension;
using HermesDelivery.Mobile.API.App_Extension.OAuth;
using HermesDelivery.Mobile.API.Models;
using HermesDelivery.Mobile.API.Models.Auth;
using Microsoft.AspNet.Identity;

namespace HermesDelivery.Mobile.API.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/audience")]
    public class AudienceController : ApiController
    {
        [Route("")]
        public IHttpActionResult Post(AudienceModel audienceModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Audience newAudience = AudiencesStore.AddAudience(audienceModel.Name);

            return Ok<Audience>(newAudience);

        }
    }
}