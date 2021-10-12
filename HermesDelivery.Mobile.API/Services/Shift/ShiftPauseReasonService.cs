using AutoMapper;
using CourierAPI.DTO.Shift;
using CourierAPI.Infrastructure.Database;
using Serilog;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace CourierAPI.Services.Shift
{
    /// <summary>
    /// Сервис причин приостановки смен.
    /// </summary>
    public class ShiftPauseReasonService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ShiftPauseReasonService(ILogger logger, IMapper mapper)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>Возвращает список причин приостановки рабочей смены.</returns>
        public async Task<IEnumerable<ShiftPauseReasonDto>> GetReasons()
        {
            var items = await _dbContext.ShiftPauseReasons.Where(x => x.IsActive).ToListAsync();

            return _mapper.Map<IEnumerable<ShiftPauseReasonDto>>(items);
        }
    }
}