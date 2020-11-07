using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.WorkShifts;
using CourierAPI.Services.Mock;
using CourierAPI.Services.Sms;
using Serilog;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace CourierAPI.Services.WorkShift
{
    /// <summary>
    /// Сервис причин приостановки смен.
    /// </summary>
    public class WorkShiftPauseReasonService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public WorkShiftPauseReasonService(ILogger logger, IMapper mapper)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>Возвращает список причин приостановки рабочей смены.</returns>
        public async Task<IEnumerable<WorkShiftPauseReasonDto>> GetReasons()
        {
            var items = await _dbContext.WorkShiftPauseReasons.Where(x => x.IsActive).ToListAsync();

            return _mapper.Map<IEnumerable<WorkShiftPauseReasonDto>>(items);
        }
    }
}