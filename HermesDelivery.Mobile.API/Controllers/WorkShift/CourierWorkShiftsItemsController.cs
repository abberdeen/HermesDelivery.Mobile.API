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
    public class CourierWorkShiftsItemsController : ApiControllerExtension
    {
        private ILogger _logger;
        private IMapper _mapper;
        private readonly CourierWorkShiftsItemService _courierWorkShiftService;

        public CourierWorkShiftsItemsController(
            ILogger logger,
            IMapper mapper,
            CourierWorkShiftsItemService courierWorkShiftService)
        {
            _logger = logger;
            _mapper = mapper;
            _courierWorkShiftService = courierWorkShiftService;
        }

        /// <summary>
        /// Получить историю смен.
        /// </summary>
        /// <returns></returns>
        // GET: /WorkShifts/History
        // [Route("WorkShifts/History")]
        [Route("Turn/History")]
        [ResponseType(typeof(IEnumerable<CourierWorkShiftsItemHistoryDto>))]
        public async Task<IHttpActionResult> GetHistory()
        {
            try
            {
                var workShiftHistory = await _courierWorkShiftService.GetHistoryAsync();
                return Ok(workShiftHistory);
            }
            catch (AppException e)
            {
                return Response(e);
            }
        }

        /// <summary>
        /// Получить текущую смену.
        /// </summary>
        /// <returns></returns>
        // GET: /WorkShifts/Current
        // [Route("WorkShifts/Current")]
        [Route("Turn")]
        [ResponseType(typeof(CourierWorkShiftsItemDto))]
        public async Task<IHttpActionResult> GetCurrent()
        {
            try
            {
                var currentWorkShift = await _courierWorkShiftService.GetCurrentAsync(GetCourierId());
                return Ok(currentWorkShift);
            }
            catch (AppException e)
            {
                return Response(e);
            }
        }

        /// <summary>
        /// Запустить смену.
        /// </summary>
        /// <returns></returns>
        // POST: /WorkShifts/Current/Start
        // [Route("WorkShifts/Current/Start")]
        [Route("Turn")]
        [ResponseType(typeof(CourierWorkShiftsItemDto))]
        [HttpPost]
        public async Task<IHttpActionResult> Start()
        {
            try
            {
                var startedWorkShift = await _courierWorkShiftService.StartAsync(GetCourierId());
                return Ok(startedWorkShift);
            }
            catch (AppException e)
            {
                return Response(e);
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
        [ResponseType(typeof(CourierWorkShiftsItemPauseResponseDto))]
        [HttpPut]
        public async Task<IHttpActionResult> Pause(CourierWorkShiftsItemPauseRequestDto model)
        {
            if (ModelState.IsValid == false)
            {
                return Response(AppMessage.InvalidModel);
            }

            try
            {
                var pauseResponse = await _courierWorkShiftService.PauseAsync(model, GetCourierId());
                return Ok(pauseResponse);
            }
            catch (AppException e)
            {
                return Response(e);
            }
        }

        /// <summary>
        /// Завершить смену.
        /// </summary>
        /// <returns></returns>
        // DELETE: /WorkShifts/Current/End
        // [Route("WorkShifts/Current/End")]
        [Route("Turn")]
        [ResponseType(typeof(CourierWorkShiftsItemDto))]
        [HttpDelete]
        public async Task<IHttpActionResult> End()
        {
            try
            {
                var nextWorkShift = await _courierWorkShiftService.EndAsync(GetCourierId());
                return Ok(nextWorkShift);
            }
            catch (AppException e)
            {
                return Response(e);
            }
        }
    }
}