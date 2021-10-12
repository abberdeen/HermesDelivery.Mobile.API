using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using CourierAPI.DTO;
using CourierAPI.Infrastructure.Exceptions;
using Microsoft.AspNet.Identity;

namespace CourierAPI.Infrastructure.Extensions
{
    public class ApiControllerExtension : ApiController
    {
        protected IHttpActionResult Response(AppMessage message)
        {
            var dto = new AppMessageDto(message);
            var responseMsg = CreateErrorResponse(message); 
  
            return ResponseMessage(responseMsg);
        }

        protected IHttpActionResult Response(AppException ex)
        {
            var responseMsg = CreateErrorResponse(ex.AppMessage);
            return ResponseMessage(responseMsg);
        }

        private HttpResponseMessage CreateErrorResponse(AppMessage appMessage)
        {
            var request = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
            var dto = new AppMessageDto(appMessage);
            return new HttpResponseMessage(appMessage.HttpStatusCode)
            {
                ReasonPhrase = appMessage.Code + ": " + appMessage.Description,
                RequestMessage = request,
                Content = new ObjectContent(dto.GetType(), dto, new JsonMediaTypeFormatter())
            };
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