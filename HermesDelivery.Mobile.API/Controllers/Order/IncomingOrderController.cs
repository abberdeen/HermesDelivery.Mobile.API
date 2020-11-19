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
    /// <summary>
    /// Заказы курьеров.
    /// </summary>
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

        /// <summary>
        /// Получить список заказов текущей смены.
        /// </summary>
        /// <returns></returns> 
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

        /// <summary>
        /// Проверить наличие входящего заказа.
        /// </summary>
        /// <returns></returns> 
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

        /// <summary>
        /// Получить детали заказа.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
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

        /// <summary>
        /// Принять входящий заказ в работу.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
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

        /// <summary>
        /// Сменить статус заказа.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="statusId"></param>
        /// <returns></returns> 
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

        /// <summary>
        /// Отклонить входящий заказ.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
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