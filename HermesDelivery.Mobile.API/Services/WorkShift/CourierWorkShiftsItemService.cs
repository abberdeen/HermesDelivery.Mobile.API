using System;
using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.WorkShifts;
using CourierAPI.Services.Mock;
using Serilog;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using CourierAPI.Infrastructure;
using CourierAPI.Infrastructure.Exceptions;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Models.DTO.Orders;

namespace CourierAPI.Services.WorkShift
{
    /// <summary>
    /// Рабочие смены курьера
    /// </summary>
    public class CourierWorkShiftsItemService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly CourierWorkShiftsScheduleService _courierWorkShiftsScheduleService; 

        public CourierWorkShiftsItemService(ILogger logger, IMapper mapper,   CourierWorkShiftsScheduleService courierWorkShiftsScheduleService)
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
        public async Task<IEnumerable<CourierWorkShiftsItemHistoryDto>> GetHistoryAsync()
        {
            var items = MockService.WorkShiftResponse_getHistory();

            return _mapper.Map<IEnumerable<CourierWorkShiftsItemHistoryDto>>(items);
        }

        /// <summary>
        /// Возвращает текущую рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о текущей рабочей смене.</returns>
        public async Task<CourierWorkShiftsItemDto> GetCurrentAsync(int courierId)
        {
            // Выполняет запрос на получение записи элемента текущей смены.
            var currentCourierWorkShiftsItem = await FindCurrentAsync(courierId);

            // Если нет записи текущей смены, то отправит запрос на создание элемента текущей смены
            if (currentCourierWorkShiftsItem == null)
            {
                currentCourierWorkShiftsItem = await CreateCurrentAsync(courierId);
            }

            // Текущая смена курьера.

            // Получает время начала.
            var startTime = currentCourierWorkShiftsItem.CourierWorkShiftsSchedule.WorkShift.StartTime;
            // Создаст дату и время начала.
            var startDateTime = DateTime.Now.Date.Add(startTime);

            // Получает время завершения.
            var endTime = currentCourierWorkShiftsItem.CourierWorkShiftsSchedule.WorkShift.EndTime;
            
            // Создаст дату и время завершения.
            
            DateTime endDateTime;

            // Проверяет переходит ли время завершения смены на новый день?.
            if (startTime < endTime)
            {
                endDateTime = startDateTime.Date.Add(endTime);
            }
            else
            {
                endDateTime = startDateTime.Date.AddDays(1).Add(endTime);
            }

            // Создаст и заполнит запись для возвращения.
            var item = new CourierWorkShiftsItemDto
            {
                IsStarted = currentCourierWorkShiftsItem.IsStarted,
                StartAt = startDateTime.Format(),
                CloseAt = endDateTime.Format(),
                PausedAt = currentCourierWorkShiftsItem.PauseTime?.Format(),
                OrderCount = new OrderCountDto()
                {

                },
                Orders = new List<WorkShiftOrderDto>()
            };

            return item;
        }

        /// <summary>
        /// Запускает рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о запущенной рабочей смене.</returns>
        public async Task<CourierWorkShiftsItemDto> StartAsync(int courierId)
        {
            var item = await FindCurrentAsync(courierId);
            
            if (!item.IsStarted)
            { 
                item.IsStarted = true;

                if (item.StartTime.HasValue == false)
                {
                    item.StartTime = DateTime.Now;
                }

                // 
                item.IsEnded = false;
                item.EndTime = null;

                // Если был на паузе, то снимает с паузы. 
                item.IsPaused = false;
                item.PauseTime = null;
                item.PauseReasonId = null;
                item.PauseDescription = null;

                await _dbContext.SaveChangesAsync();
            }

            return await GetCurrentAsync(courierId);
        }

        /// <summary>
        /// Завершает рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о следующей рабочей смене.</returns>
        public async Task<CourierWorkShiftsItemDto> EndAsync(int courierId)
        {
            var item = await FindCurrentAsync(courierId);

            if (item.IsStarted || item.IsPaused)
            {
                item.IsEnded = true;
                item.EndTime = DateTime.Now;

                // Обнуляет ненужные параметры.
                item.IsStarted = false; 
                item.IsPaused = false;
                item.PauseTime = null;
                item.PauseReasonId = null;
                item.PauseDescription = null;

                await _dbContext.SaveChangesAsync();
            }
            else
            {
               throw new AppException(AppMessage.WorkShiftNotStarted);
            }

            return await GetCurrentAsync(courierId);
        }

        /// <summary>
        /// Завершает рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о следующей рабочей смене.</returns>
        public async Task<CourierWorkShiftsItemPauseResponseDto> PauseAsync(CourierWorkShiftsItemPauseRequestDto model, int courierId)
        {
            var item = await FindCurrentAsync(courierId);

            if (item.IsEnded)
            {
                throw new AppException(AppMessage.WorkShiftEnded);
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
                throw new AppException(AppMessage.WorkShiftNotStarted);
            }

            return new CourierWorkShiftsItemPauseResponseDto()
            {
                PausedAt = item.PauseTime?.Format()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courierId"></param>
        /// <returns></returns>
        private async Task<CourierWorkShiftsItem> FindCurrentAsync(int courierId)
        {
            var schedule = await _courierWorkShiftsScheduleService.GetCurrentActiveScheduleAsync(courierId);

            var today = DateTime.Now;

            var currentCourierWorkShiftsItem = await _dbContext.CourierWorkShiftsItems
                .FirstOrDefaultAsync(x =>
                    x.CourierWorkShiftShceduleId == schedule.Id &&
                    x.CreatedAt.Year == today.Year &&
                    x.CreatedAt.Month == today.Month &&
                    x.CreatedAt.Day == today.Day);

            return currentCourierWorkShiftsItem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="courierId"></param>
        /// <returns></returns>
        private async Task<CourierWorkShiftsItem> CreateCurrentAsync(int courierId)
        {
            var schedule = await _courierWorkShiftsScheduleService.GetCurrentActiveScheduleAsync(courierId);

            var item = new CourierWorkShiftsItem
            {
                CourierWorkShiftShceduleId = schedule.Id,
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

            _dbContext.CourierWorkShiftsItems.Add(item);

            await _dbContext.SaveChangesAsync();

            return item;
        }


        /// <summary>
        /// Проверит текущее время с временем расписания.
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private bool IsTimeOfWorkShift(TimeSpan startTime, TimeSpan endTime)
        {
            var timeNow = DateTime.Now.TimeOfDay;
            return (startTime <= timeNow && endTime >= timeNow && startTime < endTime) || // Normal case, e.g. 8am-2pm
                   ((startTime <= timeNow || endTime <= timeNow) && startTime > endTime);  // Reverse case, e.g. 10pm-2am
        }
    }
}