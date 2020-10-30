using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.WorkShifts;
using CourierAPI.Services.Account;
using CourierAPI.Services.Sms;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CourierAPI.Services.WorkShift
{
    [Authorize]
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
        public async Task<IEnumerable<WorkShiftPauseReasonDto>> List()
        {
            var items = new List<WorkShiftPauseReasonDto>()
            {
                new WorkShiftPauseReasonDto()
                {
                    Id = 1,
                    Name= "Поломка транспорта"
                },
                new WorkShiftPauseReasonDto()
                {
                    Id = 2,
                    Name= "Поломка транспорта 2"
                }
            };

            return _mapper.Map<IEnumerable<WorkShiftPauseReasonDto>>(items);
        }
    }
}