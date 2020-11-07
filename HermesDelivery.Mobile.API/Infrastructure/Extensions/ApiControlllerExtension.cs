using System;
using System.Net.Http;
using System.Web.Http;
using CourierAPI.Infrastructure.Exceptions;
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

        protected IHttpActionResult Response(AppException ex)
        {
            var dto = new AppMessageDto(ex.AppMessage, ex.Description);
            var responseMsg = Request.CreateResponse(ex.AppMessage.HttpStatusCode, dto);

            return ResponseMessage(responseMsg);
        }

        /// <summary>
        /// 
        /// </summary> 
        protected int GetCourierId()
        {
            Int32.TryParse(User.Identity.GetUserId(), out int courierId);
            return courierId;
        }
    }
}