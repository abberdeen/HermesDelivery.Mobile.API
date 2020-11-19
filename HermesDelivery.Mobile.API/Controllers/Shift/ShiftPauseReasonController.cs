using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using CourierAPI.DTO.Shift;
using CourierAPI.Infrastructure.Exceptions;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Services.Shift;
using Serilog;

namespace CourierAPI.Controllers.Shift
{
    /// <summary>
    /// Причины приостановки смен.
    /// </summary>
    [Authorize]
    public class ShiftPauseReasonController : ApiControllerExtension
    {
        private ILogger _logger;
        private IMapper _mapper;
        private readonly ShiftPauseReasonService _workShiftPauseReasonService;

        public ShiftPauseReasonController(
            ILogger logger,
            IMapper mapper,
            ShiftPauseReasonService workShiftPauseReasonService)
        {
            _logger = logger;
            _mapper = mapper;
            _workShiftPauseReasonService = workShiftPauseReasonService;
        }

        /// <summary>
        /// Получить список причин остановки смен.
        /// </summary>
        /// <returns></returns> 
        [Route("Turn/PauseReasons")]
        [ResponseType(typeof(IEnumerable<ShiftPauseReasonDto>))]
        public async Task<IHttpActionResult> GetReasons()
        {
            try
            {
                var workShiftPauseReasons = await _workShiftPauseReasonService.GetReasons();
                return Ok(workShiftPauseReasons);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }
    }
}