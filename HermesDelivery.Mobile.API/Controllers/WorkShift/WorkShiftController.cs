using AutoMapper;
using CourierAPI.Infrastructure;
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
    public class WorkShiftController : ApiControllerExtension
    {
        private ILogger _logger;
        private IMapper _mapper;
        private readonly WorkShiftService _workShiftService;

        public WorkShiftController(
            ILogger logger,
            IMapper mapper,
            WorkShiftService workShiftService)
        {
            _logger = logger;
            _mapper = mapper;
            _workShiftService = workShiftService;
        }

        // Получить историю смен
        // GET: /Turn/History
        [Route("Turn/History")]
        [ResponseType(typeof(IEnumerable<WorkShiftHistoryDto>))]
        public async Task<IHttpActionResult> GetHistory()
        {
            try
            {
                var workShiftHistory = await _workShiftService.GetHistory();
                return Ok(workShiftHistory);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }

        // Получить текущую смену
        // GET: /Turn
        [Route("Turn")]
        [ResponseType(typeof(WorkShiftDto))]
        public async Task<IHttpActionResult> Get()
        {
            try
            {
                var currentWorkShift = await _workShiftService.GetCurrent();
                return Ok(currentWorkShift);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }

        // Запустить смену
        // POST: /Turn
        [Route("Turn")]
        [ResponseType(typeof(WorkShiftDto))]
        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            try
            {
                var startedWorkShift = await _workShiftService.Start();
                return Ok(startedWorkShift);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }

        // Поставить смену на паузу
        // PUT: /Turn
        [Route("Turn")]
        [ResponseType(typeof(WorkShiftPauseResponseDto))]
        [HttpPut]
        public async Task<IHttpActionResult> Put(WorkShiftPauseRequestDto model)
        {
            if (ModelState.IsValid == false)
            {
                return Response(AppMessage.InvalidModel);
            }

            try
            {
                var pauseResponse = await _workShiftService.Pause(model);
                return Ok(pauseResponse);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }

        // Завершить смену
        // DELETE: /Turn
        [Route("Turn")]
        [ResponseType(typeof(WorkShiftDto))]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete()
        {
            try
            {
                var nextWorkShift = await _workShiftService.End();
                return Ok(nextWorkShift);
            }
            catch (AppException e)
            {
                return Response(e.AppMessage);
            }
        }
    }
}