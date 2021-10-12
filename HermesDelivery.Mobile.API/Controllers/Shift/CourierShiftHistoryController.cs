using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoMapper;
using CourierAPI.DTO;
using CourierAPI.DTO.Shift;
using CourierAPI.Infrastructure.Exceptions;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Services.Shift;
using Serilog;

namespace CourierAPI.Controllers.Shift
{
    /// <summary>
    /// Смены курьеров.
    /// </summary>
    [Authorize]
    public class CourierShiftHistoryController : ApiControllerExtension
    {
        private ILogger _logger;
        private IMapper _mapper;
        private readonly CourierShiftHistoryService _courierShiftHistoryService;

        public CourierShiftHistoryController(
            ILogger logger,
            IMapper mapper,
            CourierShiftHistoryService courierShiftHistoryService)
        {
            _logger = logger;
            _mapper = mapper;
            _courierShiftHistoryService = courierShiftHistoryService;
        }

        /// <summary>
        /// Получить историю смен.
        /// </summary>
        /// <returns></returns>
        [Route("Turn/History")]
        [ResponseType(typeof(IEnumerable<CourierShiftHistoryListItemDto>))]
        public async Task<IHttpActionResult> GetHistory()
        {
            try
            {
                var workShiftHistory = await _courierShiftHistoryService.GetHistoryAsync(GetCourierId());
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
        [Route("Turn")]
        [ResponseType(typeof(CourierShiftHistoryDto))]
        public async Task<IHttpActionResult> GetCurrent()
        {
            try
            {
                var currentWorkShift = await _courierShiftHistoryService.GetCurrentOrNextAsync(GetCourierId());
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
        [Route("Turn")]
        [ResponseType(typeof(CourierShiftHistoryDto))]
        [HttpPost]
        public async Task<IHttpActionResult> Start()
        {
            try
            {
                var startedWorkShift = await _courierShiftHistoryService.StartAsync(GetCourierId());
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
        [Route("Turn")]
        [ResponseType(typeof(CourierShiftHistoryPauseResponseDto))]
        [HttpPut]
        public async Task<IHttpActionResult> Pause(CourierShiftHistoryPauseRequestDto model)
        {
            if (ModelState.IsValid == false)
            {
                return Response(AppMessage.InvalidModel);
            }

            try
            {
                var pauseResponse = await _courierShiftHistoryService.PauseAsync(model, GetCourierId());
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
        [Route("Turn")]
        [ResponseType(typeof(CourierShiftHistoryDto))]
        [HttpDelete]
        public async Task<IHttpActionResult> End()
        {
            try
            {
                var nextWorkShift = await _courierShiftHistoryService.EndAsync(GetCourierId());
                return Ok(nextWorkShift);
            }
            catch (AppException e)
            {
                return Response(e);
            }
        }
    }
}