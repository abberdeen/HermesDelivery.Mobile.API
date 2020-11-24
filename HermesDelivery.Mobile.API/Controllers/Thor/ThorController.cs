using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using CourierAPI.DTO.Orders;
using CourierAPI.Infrastructure.Exceptions;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Services.Order;
using CourierAPI.Services.ThorWebSocket;
using Serilog;

namespace CourierAPI.Controllers
{
    /// <summary>
    ///  
    /// </summary> 
    public class ThorController : ApiControllerExtension
    { 
        [Route("Thor")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage RunThor()
        {
            HttpContext currentContext = HttpContext.Current;
            if (currentContext.IsWebSocketRequest ||
                currentContext.IsWebSocketRequestUpgrading)
            {
                currentContext.AcceptWebSocketRequest(WebSocketHandler.WebSocketRequest);
                return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
            } 

            return Request.CreateResponse(HttpStatusCode.BadGateway);
        }

    }
}