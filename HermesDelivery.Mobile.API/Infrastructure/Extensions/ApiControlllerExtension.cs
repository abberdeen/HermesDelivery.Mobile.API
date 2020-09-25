using System.Net;
using System.Net.Http;
using System.Web.Http;
using HermesDMobAPI.Models.DTO;

namespace HermesDMobAPI.Infrastructure.Extensions
{
    public class ApiControllerExtension : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected IHttpActionResult BadRequest(AppMessage message)
        {
            return Response(HttpStatusCode.BadRequest, message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected IHttpActionResult NotFound(AppMessage message)
        {
            return Response(HttpStatusCode.NotFound, message);
        }

        private IHttpActionResult Response(HttpStatusCode statusCode, AppMessage message)
        {
            var dto = new AppMessageDto(message); 
            var responseMsg = Request.CreateResponse(statusCode, dto); 

            return ResponseMessage(responseMsg); ;
        }
    }
}