using AutoMapper;
using CourierAPI.Infrastructure.Exceptions;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Models.DTO.WorkShifts;
using CourierAPI.Services.WorkShift;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace CourierAPI.Controllers.WorkShift
{
    [Authorize]
    public class WorkShiftPauseReasonController : ApiControllerExtension
    {
        private ILogger _logger;
        private IMapper _mapper;
        private readonly WorkShiftPauseReasonService _workShiftPauseReasonService;

        public WorkShiftPauseReasonController(
            ILogger logger,
            IMapper mapper,
            WorkShiftPauseReasonService workShiftPauseReasonService)
        {
            _logger = logger;
            _mapper = mapper;
            _workShiftPauseReasonService = workShiftPauseReasonService;
        }

        // GET: /WorkShifts/PauseReasons
        [Route("WorkShifts/PauseReasons")]
        [Route("Turn/PauseReasons")]
        [ResponseType(typeof(IEnumerable<WorkShiftPauseReasonDto>))]
        public async Task<IHttpActionResult> Get()
        {
            try
            {
                var workShiftPauseReasons = await _workShiftPauseReasonService.List();
                return Ok(workShiftPauseReasons);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }
    }
}