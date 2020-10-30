using System.Net.Http;
using System.Web.Http;

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
    }
}