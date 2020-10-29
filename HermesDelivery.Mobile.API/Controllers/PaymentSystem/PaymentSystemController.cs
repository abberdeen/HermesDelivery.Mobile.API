using AutoMapper;
using HermesDMobAPI.Infrastructure.Exceptions;
using HermesDMobAPI.Infrastructure.Extensions;
using HermesDMobAPI.Models.DTO.PaymentSystems;
using HermesDMobAPI.Services.PaymentSystem;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace HermesDMobAPI.Controllers.PaymentSystem
{
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

        // GET: /QrPayProviders
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