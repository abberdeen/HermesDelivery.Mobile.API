using System;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.Identity;

namespace CourierAPI.Infrastructure.Extensions
{
    public class ApiControllerExtension : ApiController
    {
        protected IHttpActionResult Response(AppMessage message)
        {
            var dto = new AppMessageDto(message);
            var responseMsg = Request.CreateResponse(message.HttpStatusCode, dto);

            return ResponseMessage(responseMsg);
        }

        protected int CourierId()
        {
            return Int32.Parse(User.Identity.GetUserId());
        }
    }
}