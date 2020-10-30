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
        // GET: /WorkShifts/History
        [Route("WorkShifts/History")]
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
        // GET: /WorkShifts/Current
        [Route("WorkShifts/Current")]
        [Route("Turn")]
        [ResponseType(typeof(WorkShiftDto))]
        public async Task<IHttpActionResult> GetCurrent()
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
        // POST: /WorkShifts/Current/Start
        [Route("WorkShifts/Current/Start")]
        [Route("Turn")]
        [ResponseType(typeof(WorkShiftDto))]
        [HttpPost]
        public async Task<IHttpActionResult> Start()
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
        // PUT: /WorkShifts/Current/Pause
        [Route("WorkShifts/Current/Pause")]
        [Route("Turn")]
        [ResponseType(typeof(WorkShiftPauseResponseDto))]
        [HttpPut]
        public async Task<IHttpActionResult> Pause(WorkShiftPauseRequestDto model)
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
        // DELETE: /WorkShifts/Current/End
        [Route("WorkShifts/Current/End")]
        [Route("Turn")]
        [ResponseType(typeof(WorkShiftDto))]
        [HttpDelete]
        public async Task<IHttpActionResult> End()
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