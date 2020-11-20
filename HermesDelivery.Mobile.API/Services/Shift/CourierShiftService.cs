using AutoMapper;
using CourierAPI.DTO;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Infrastructure.Exceptions;
using Serilog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace CourierAPI.Services.Shift
{
    /// <summary>
    /// Расписание рабочих смен.
    /// </summary>
    public class CourierShiftService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public CourierShiftService(ILogger logger, IMapper mapper)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public async Task<CourierShift> GetCurrentOrNextActiveScheduleAsync(int courierId)
        {
            var activeSchedule = await GetCurrentActiveScheduleAsync(courierId);

            // Если сейчас нет активной текущей смены.
            if (activeSchedule == null)
            {
                // Получит близлежащую по времени следующею смену курьера.
                activeSchedule = await GetNextActiveScheduleAsync(courierId);
            }

            return activeSchedule;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public async Task<CourierShift> GetCurrentActiveScheduleAsync(int courierId)
        {
            var timeNow = DateTime.Now.TimeOfDay;

            if (!await AnyActiveScheduleAsync(courierId))
            {
                throw new AppException(AppMessage.ShiftNotAssigned);
            }

            // Ищет текущую активную смену для курьера.
            var currentActiveSchedule = await _dbContext.CourierShifts
                 .FirstOrDefaultAsync(x =>
                     x.CourierId == courierId &&
                     x.IsActive &&
                     ((x.Shift.StartTime <= timeNow && x.Shift.EndTime >= timeNow && x.Shift.StartTime < x.Shift.EndTime) || // Normal case, e.g. 8am-2pm
                      ((x.Shift.StartTime <= timeNow || x.Shift.EndTime <= timeNow) && x.Shift.StartTime > x.Shift.EndTime)));  // Reverse case, e.g. 10pm-2am

            return currentActiveSchedule;
        }

        /// <summary>
        /// Возвращает близлежащую по времени следующею смену курьера.
        /// </summary>
        /// <param name="courierId"></param>
        /// <returns></returns>
        private async Task<CourierShift> GetNextActiveScheduleAsync(int courierId)
        {
            if (!await AnyActiveScheduleAsync(courierId))
            {
                throw new AppException(AppMessage.ShiftNotAssigned);
            }

            // Получает список расписаний курьера, сортирует по начальному времени смены.
            var courierShiftsSchedules = await _dbContext.CourierShifts
                .Where(x =>
                    x.CourierId == courierId &&
                    x.IsActive)
                .OrderBy(x =>
                    x.Shift.StartTime)
                .ToListAsync();

            // Ищет близлежащую по времени следующею смену.
            var nearestActiveSchedule = courierShiftsSchedules
                .FirstOrDefault(i =>
                    i.Shift.StartTime >= DateTime.Now.TimeOfDay);

            // Если на сегодня нету смены, получает первую на следующий день смену.
            if (nearestActiveSchedule is null)
            {
                nearestActiveSchedule = courierShiftsSchedules.FirstOrDefault();
            }

            return nearestActiveSchedule;
        }

        /// <summary>
        /// Проверит есть ли у курьера активная назначенная смена.
        /// </summary>
        /// <param name="courierId"></param>
        /// <returns></returns>
        public async Task<bool> AnyActiveScheduleAsync(int courierId)
        {
            var anyActiveSchedule = await _dbContext.CourierShifts
                .AnyAsync(x =>
                    x.CourierId == courierId &&
                    x.IsActive);
            return anyActiveSchedule;
        }
    }
}