using AutoMapper;
using CourierAPI.Infrastructure.Exceptions;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Services.PaymentSystem;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CourierAPI.DTO.PaymentSystem;

namespace CourierAPI.Controllers.PaymentSystem
{
    /// <summary>
    /// Платежные системы (QR).
    /// </summary>
    [Authorize]
    public class PaymentSystemController : ApiControllerExtension
    {
        private ILogger _logger;
        private IMapper _mapper;
        private readonly PaymentSystemService _paymentSystemService;

        public PaymentSystemController(
            ILogger logger,
            IMapper mapper,
            PaymentSystemService paymentSystemService)
        {
            _logger = logger;
            _mapper = mapper;
            _paymentSystemService = paymentSystemService;
        }

        /// <summary>
        /// Получить список платежный систем (QR-ов)
        /// </summary>
        /// <returns></returns>
        // GET: /PaymentSystems/QR/Codes
        // [Route("PaymentSystems/QR/Codes")]
        [Route("QrPayProviders")]
        [ResponseType(typeof(IEnumerable<PaymentSystemDto>))]
        public async Task<IHttpActionResult> Get()
        {
            try
            {
                var paymentSystems = await _paymentSystemService.List();
                return Ok(paymentSystems);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }
    }
}