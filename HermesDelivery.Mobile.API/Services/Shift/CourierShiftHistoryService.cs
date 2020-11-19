using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Infrastructure.Exceptions;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Models;
using CourierAPI.Models.DTO.Orders;
using CourierAPI.Models.DTO.Shift;
using CourierAPI.Services.Mock;
using Serilog;

namespace CourierAPI.Services.Shift
{
    /// <summary>
    /// Рабочие смены курьера
    /// </summary>
    public class CourierShiftHistoryService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly CourierShiftService _courierWorkShiftsScheduleService; 

        public CourierShiftHistoryService(ILogger logger, IMapper mapper,   CourierShiftService courierWorkShiftsScheduleService)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper; 
            _courierWorkShiftsScheduleService = courierWorkShiftsScheduleService;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>Возвращает историю рабочих смен.</returns>
        public async Task<IEnumerable<CourierShiftHistoryListItemDto>> GetHistoryAsync()
        {
           // var courierWorkShiftItems = _dbContext.CourierShiftHistorys.Join()
            var items = MockService.WorkShiftResponse_getHistory();

            return _mapper.Map<IEnumerable<CourierShiftHistoryListItemDto>>(items);
        }

        /// <summary>
        /// Возвращает текущую или следующую рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о текущей рабочей смене.</returns>
        public async Task<CourierShiftHistoryDto> GetCurrentOrNextAsync(int courierId)
        {
            // Выполняет запрос на получение записи элемента текущей смены.
            var currentOrNextCourierShiftHistory = await FindCurrentOrNextAsync(courierId);

            // Если нет записи текущей смены, то отправит запрос на создание элемента текущей смены
            if (currentOrNextCourierShiftHistory == null)
            {
                await CreateCurrentOrNextAsync(courierId);
                currentOrNextCourierShiftHistory = await FindCurrentOrNextAsync(courierId);
            }

            var item = MapCourierShiftHistoryToDTO(currentOrNextCourierShiftHistory);

            return item;
        } 

        private CourierShiftHistoryDto MapCourierShiftHistoryToDTO(CourierShiftHistory courierWorkShiftsItem)
        {
            // Текущая смена курьера.

            // Получает время начала.
            var workShiftStartTime = courierWorkShiftsItem.CourierShift.Shift.StartTime;

            // Получает время завершения.
            var workShiftEndTime = courierWorkShiftsItem.CourierShift.Shift.EndTime;

            // Создаст дату и время начала.
            var startDateTime = CalcStartDateTime(workShiftStartTime, workShiftEndTime);
            
            // Создаст дату и время завершения.
            var endDateTime = CalcEndDateTime(workShiftStartTime, workShiftEndTime, startDateTime);

            // Создаст и заполнит запись для возвращения.
            var item = new CourierShiftHistoryDto
            {
                IsStarted = courierWorkShiftsItem.IsStarted,
                StartedAt = courierWorkShiftsItem.StartTime?.Format(),
                StartAt = startDateTime.Format(),
                CloseAt = endDateTime.Format(),
                PausedAt = courierWorkShiftsItem.PauseTime?.Format(),
                OrderCount = new OrderCountDto()
                {

                },
                Orders = new List<WorkShiftOrderDto>()
            };
            return item;
        }

        private DateTime CalcStartDateTime(TimeSpan workShiftStartTime, TimeSpan workShiftEndTime)
        {
            // Создаст дату и время начала.
            var startDateTime = DateTime.Now.Date.Add(workShiftStartTime);

            // Если начало смены приходится на следующий день.
            if (DateTime.Now.TimeOfDay > workShiftEndTime)
            {
                startDateTime = startDateTime.AddDays(1);
            }

            return startDateTime;
        }

        private DateTime CalcEndDateTime(TimeSpan workShiftStartTime, TimeSpan workShiftEndTime, DateTime startDateTime)
        {
            DateTime endDateTime;

            // Проверяет переходит ли время завершения смены на новый день.
            if (workShiftStartTime < workShiftEndTime)
            {
                endDateTime = startDateTime.Date.Add(workShiftEndTime);
            }
            else
            {
                endDateTime = startDateTime.Date.AddDays(1).Add(workShiftEndTime);
            }

            return endDateTime;
        }

        /// <summary>
        /// Запускает рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о запущенной рабочей смене.</returns>
        public async Task<CourierShiftHistoryDto> StartAsync(int courierId)
        {
            var item = await FindCurrentOrNextAsync(courierId);

            if (item.IsEnded)
            {
                throw new AppException(AppMessage.ShiftEnded);
            }

            if (!item.IsStarted)
            {
                item.IsStarted = true;

                // Если смена запускается в первый раз, то есть, если не снимается с паузы.
                if (item.StartTime.HasValue == false)
                {
                    item.StartTime = DateTime.Now;
                }

                // Если был на паузе, то снимает с паузы. 
                item.IsPaused = false;
                item.PauseTime = null;
                item.PauseReasonId = null;
                item.PauseDescription = null;

                await _dbContext.SaveChangesAsync();
            }

            return await GetCurrentOrNextAsync(courierId);
        }

        /// <summary>
        /// Завершает рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о следующей рабочей смене.</returns>
        public async Task<CourierShiftHistoryDto> EndAsync(int courierId)
        {
            var item = await FindCurrentOrNextAsync(courierId);

            if (item.IsStarted || item.IsPaused)
            {
                item.IsEnded = true;
                item.EndTime = DateTime.Now;

                // Обнуляет ненужные параметры.
                //
                item.IsStarted = false; 
                //
                item.IsPaused = false;
                item.PauseTime = null;
                item.PauseReasonId = null;
                item.PauseDescription = null;

                await _dbContext.SaveChangesAsync();
            }
            else
            {
               throw new AppException(AppMessage.ShiftNotStartedOrPaused);
            }

            return await GetCurrentOrNextAsync(courierId);
        }

        /// <summary>
        /// Завершает рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о следующей рабочей смене.</returns>
        public async Task<CourierShiftHistoryPauseResponseDto> PauseAsync(CourierShiftHistoryPauseRequestDto model, int courierId)
        {
            var item = await FindCurrentOrNextAsync(courierId);

            if (item.IsEnded)
            {
                throw new AppException(AppMessage.ShiftEnded);
            }

            if (item.IsStarted)
            {
                item.IsStarted = false; 

                // Если был на паузе, то снимает с паузы. 
                item.IsPaused = true;
                item.PauseTime = DateTime.Now;
                item.PauseReasonId = model.ReasonId;
                item.PauseDescription = model.Comment;

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new AppException(AppMessage.ShiftNotStarted);
            }

            return new CourierShiftHistoryPauseResponseDto()
            {
                PausedAt = item.PauseTime?.Format()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courierId"></param>
        /// <returns></returns>
        private async Task<CourierShiftHistory> FindCurrentOrNextAsync(int courierId)
        {
            var courierShift = await _courierWorkShiftsScheduleService.GetCurrentOrNextActiveScheduleAsync(courierId);
             
            var currentCourierShiftHistory = await _dbContext.CourierShiftHistories
                .Include(x=>x.CourierShift)
                .OrderByDescending(x=>
                    x.CreatedAt)
                .FirstOrDefaultAsync(x =>
                    x.CourierShiftId == courierShift.Id &&
                    x.IsEnded == false);

            return currentCourierShiftHistory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courierId"></param>
        /// <returns></returns>
        private async Task CreateCurrentOrNextAsync(int courierId)
        {
            var schedule = await _courierWorkShiftsScheduleService.GetCurrentOrNextActiveScheduleAsync(courierId);

            var item = new CourierShiftHistory
            {
                CourierShiftId = schedule.Id,
                IsStarted = false,
                StartTime = null,
                IsEnded = false,
                EndTime = null,
                IsPaused = false,
                PauseReasonId = null,
                PauseTime = null,
                PauseDescription = null,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _dbContext.CourierShiftHistories.Add(item);

            await _dbContext.SaveChangesAsync(); 
        }


        /// <summary>
        /// Проверит текущее время с временем расписания.
        /// </summary>
        /// <param name="workShiftStartTime"></param>
        /// <param name="workShiftEndTime"></param>
        /// <returns></returns>
        private bool IsTimeOfWorkShift(TimeSpan workShiftStartTime, TimeSpan workShiftEndTime)
        {
            // Создаст дату и время начала.
            var startDateTime = CalcStartDateTime(workShiftStartTime, workShiftEndTime);

            // Создаст дату и время завершения.
            var endDateTime = CalcEndDateTime(workShiftStartTime, workShiftEndTime, startDateTime);

            var dateTimeNow = DateTime.Now;
            return dateTimeNow >= startDateTime && dateTimeNow < endDateTime;
        }
    }
}