using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using AutoMapper;
using CourierAPI.Infrastructure;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Infrastructure.Exceptions;
using CourierAPI.Models.DTO.WorkShifts;
using Serilog;

namespace CourierAPI.Services.WorkShift
{
    /// <summary>
    /// Расписание рабочих смен.
    /// </summary>
    public class CourierWorkShiftsScheduleService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper; 

        public CourierWorkShiftsScheduleService(ILogger logger, IMapper mapper)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper; 
        }

       /// <summary>
       /// 
       /// </summary>
       /// <returns></returns>
        public async Task<CourierWorkShiftsCurrentActiveScheduleDto> GetCurrentActiveScheduleAsync(int courierId)
        {
            var timeNow = DateTime.Now.TimeOfDay;
  
           var courierWorkShiftsSchedule = await _dbContext.CourierWorkShiftsSchedules
                .FirstOrDefaultAsync(x => 
                    x.CourierId == courierId &&
                    x.IsActive && 
                    ((x.WorkShift.StartTime <= timeNow && x.WorkShift.EndTime >= timeNow && x.WorkShift.StartTime < x.WorkShift.EndTime) || // Normal case, e.g. 8am-2pm
                     ((x.WorkShift.StartTime <= timeNow || x.WorkShift.EndTime <= timeNow) && x.WorkShift.StartTime > x.WorkShift.EndTime)));  // Reverse case, e.g. 10pm-2am

            if (courierWorkShiftsSchedule == null)
            {
                throw new AppException(AppMessage.NowNotShiftTime);
            }

            return _mapper.Map<CourierWorkShiftsCurrentActiveScheduleDto>(courierWorkShiftsSchedule);
        }
    }
}