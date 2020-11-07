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
    /// <summary>
    /// Смены курьеров.
    /// </summary>
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

        /// <summary>
        /// Получить историю смен.
        /// </summary>
        /// <returns></returns>
        // GET: /WorkShifts/History
        // [Route("WorkShifts/History")]
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

        /// <summary>
        /// Получить текущую смену.
        /// </summary>
        /// <returns></returns>
        // GET: /WorkShifts/Current
        // [Route("WorkShifts/Current")]
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

        /// <summary>
        /// Запустить смену.
        /// </summary>
        /// <returns></returns>
        // POST: /WorkShifts/Current/Start
        // [Route("WorkShifts/Current/Start")]
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

        /// <summary>
        /// Поставить смену на паузу.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        // PUT: /WorkShifts/Current/Pause
        // [Route("WorkShifts/Current/Pause")]
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

        /// <summary>
        /// Завершить смену.
        /// </summary>
        /// <returns></returns>
        // DELETE: /WorkShifts/Current/End
        // [Route("WorkShifts/Current/End")]
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