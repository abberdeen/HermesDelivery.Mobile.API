using AutoMapper;
using CourierAPI.Infrastructure;
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
        // GET: /Turn/Orders/
        [Route("Turn/Orders")]
        [ResponseType(typeof(IEnumerable<IncomingOrderDto>))]
        public async Task<IHttpActionResult> GetOrders()
        {
            try
            {
                var incomingOrderList = await _incomingOrderService.GetList();
                return Ok(incomingOrderList);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }

        // Проверить наличие входящего заказа.
        // GET: /Turn/IncomingOrder
        [Route("Turn/IncomingOrder")]
        [ResponseType(typeof(IncomingOrderInfoDto))]
        public async Task<IHttpActionResult> Get()
        {
            try
            {
                var incomingOrderInfo = await _incomingOrderService.GetIncomingOrder();
                return Ok(incomingOrderInfo);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }

        // Получить детали заказа.
        // GET: /Turn/Orders/{id}/
        [Route("Turn/Orders/{id:int}")]
        [ResponseType(typeof(IncomingOrderDetailsDto))]
        public async Task<IHttpActionResult> Details(int id)
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
        // POST: /Turn/IncomingOrder/{id}/
        [Route("Turn/IncomingOrder/{id:int}")]
        [ResponseType(typeof(IncomingOrderInfoDto))]
        [HttpPost]
        public async Task<IHttpActionResult> Post(int id)
        {
            if (id <= 0)
            {
                return Response(AppMessage.BadRequest);
            }

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
        // PUT: /Turn/Orders/{id}/UpdateStatus/{statusId}/
        [Route("Turn/Orders/{orderId:int}/UpdateStatus/{statusId:int}")]
        [ResponseType(typeof(IncomingOrderStatusChangeResponseDto))]
        [HttpPut]
        public async Task<IHttpActionResult> Put(int orderId, int statusId)
        {
            return Ok(new IncomingOrderStatusChangeResponseDto()
            {
                Id = 2,
                Name = "Отдал клиенту"
            });
        }

        // Отклонить входящий заказ.
        // DELETE: /Turn/IncomingOrder/{id}/
        [Route("Turn/IncomingOrder/{id:int}")]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return Response(AppMessage.BadRequest);
            }

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