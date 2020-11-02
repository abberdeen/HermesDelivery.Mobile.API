using AutoMapper;
using CourierAPI.Infrastructure.Exceptions;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Models.DTO.Orders;
using CourierAPI.Services.Order;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace CourierAPI.Controllers.Order
{
    [Authorize]
    public class IncomingOrderController : ApiControllerExtension
    {
        private ILogger _logger;
        private IMapper _mapper;
        private readonly IncomingOrderService _incomingOrderService;

        public IncomingOrderController(
            ILogger logger,
            IMapper mapper,
            IncomingOrderService incomingOrderService)
        {
            _logger = logger;
            _mapper = mapper;
            _incomingOrderService = incomingOrderService;
        }

        // Получить список заказов.
        // GET: /IncomingOrders/
        [Route("IncomingOrders")]
        [Route("Turn/Orders")]
        [ResponseType(typeof(IEnumerable<IncomingOrderDto>))]
        public async Task<IHttpActionResult> GetOrders()
        {
            try
            {
                var incomingOrderList = await _incomingOrderService.GetOrders();
                return Ok(incomingOrderList);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }

        // Проверить наличие входящего заказа.
        // GET: /IncomingOrders/New
        [Route("IncomingOrders/Pending")]
        [Route("Turn/IncomingOrder")]
        [ResponseType(typeof(IncomingOrderInfoDto))]
        public async Task<IHttpActionResult> GetPending()
        {
            try
            {
                var incomingOrderInfo = await _incomingOrderService.GetPending();
                return Ok(incomingOrderInfo);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }

        // Получить детали заказа.
        // GET: /IncomingOrders/1/Details
        [Route("IncomingOrders/{id:int}/Details")]
        [Route("Turn/Orders/{id:int}")]
        [ResponseType(typeof(IncomingOrderDetailsDto))]
        public async Task<IHttpActionResult> GetDetails(int id)
        {
            try
            {
                var incomingOrderDetails = await _incomingOrderService.Details(id);
                return Ok(incomingOrderDetails);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }

        // Принять входящий заказ в работу.
        // POST: /IncomingOrder/1/Accept
        [Route("IncomingOrder/{id:int:min(1)}/Accept")]
        [Route("Turn/IncomingOrder/{id:int:min(1)}")]
        [ResponseType(typeof(IncomingOrderInfoDto))]
        [HttpPost]
        public async Task<IHttpActionResult> Accept(int id)
        {
            try
            {
                var incomingOrderInfo = await _incomingOrderService.Accept(id);
                return Ok(incomingOrderInfo);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }

        // Сменить статус заказа.
        // PUT: /IncomingOrder/1/SetStatus/1
        [Route("IncomingOrder/{orderId:int:min(1)}/SetStatus/{statusId:int:min(1)}")]
        [Route("Turn/Orders/{orderId:int}/UpdateStatus/{statusId:int}")]
        [ResponseType(typeof(IncomingOrderStatusChangeResponseDto))]
        [HttpPut]
        public async Task<IHttpActionResult> SetStatus(int orderId, int statusId)
        {
            return Ok(new IncomingOrderStatusChangeResponseDto()
            {
                Id = 2,
                Name = "Отдал клиенту"
            });
        }

        // Отклонить входящий заказ.
        // DELETE: /IncomingOrder/1/Reject
        [Route("IncomingOrder/{id:int:min(1)}/Reject")]
        [Route("Turn/IncomingOrder/{id:int}")]
        [HttpDelete]
        public async Task<IHttpActionResult> Reject(int id)
        {
            try
            {
                await _incomingOrderService.Reject(id);
                return Ok();
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }
    }
}