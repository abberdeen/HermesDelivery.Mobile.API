using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.WorkShifts;
using CourierAPI.Services.Account;
using CourierAPI.Services.Mock;
using CourierAPI.Services.Sms;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourierAPI.Services.WorkShift
{
    public class WorkShiftPauseReasonService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public WorkShiftPauseReasonService(ILogger logger, IMapper mapper, UserService userService, MessageService messageService)
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
            var items = MockService.WorkShiftPauseReasonResponse_getReasons();

            return _mapper.Map<IEnumerable<WorkShiftPauseReasonDto>>(items);
        }
    }
}