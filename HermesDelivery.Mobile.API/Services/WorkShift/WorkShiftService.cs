using AutoMapper;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Models.DTO.Orders;
using CourierAPI.Models.DTO.WorkShifts;
using CourierAPI.Services.Account;
using CourierAPI.Services.Sms;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Mvc;
using CourierAPI.Services.Mock;

namespace CourierAPI.Services.WorkShift
{ 
    public class WorkShiftService
    {
        private AppDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public WorkShiftService(ILogger logger, IMapper mapper, UserService userService, MessageService messageService)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>Возвращает историю рабочих смен.</returns>
        public async Task<IEnumerable<WorkShiftHistoryDto>> GetHistory()
        {
            var items = MockService.WorkShiftResponse_getHistory();

            return _mapper.Map<IEnumerable<WorkShiftHistoryDto>>(items);
        }

        /// <summary>
        /// Возвращает текущую рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о текущей рабочей смене.</returns>
        public async Task<WorkShiftDto> GetCurrent()
        {
            var item = MockService.WorkShiftResponse_getCurrent();

            return _mapper.Map<WorkShiftDto>(item);
        }

        /// <summary>
        /// Запускает рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о запущенной рабочей смене.</returns>
        public async Task<WorkShiftDto> Start()
        {
            var item = MockService.WorkShiftResponse_start();

            return _mapper.Map<WorkShiftDto>(item);
        }

        /// <summary>
        /// Завершает рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о следующей рабочей смене.</returns>
        public async Task<WorkShiftDto> End()
        {
            var item = MockService.WorkShiftResponse_end();

            return _mapper.Map<WorkShiftDto>(item);
        }

        /// <summary>
        /// Завершает рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о следующей рабочей смене.</returns>
        public async Task<WorkShiftPauseResponseDto> Pause(WorkShiftPauseRequestDto model)
        {
            // mocked
            return new WorkShiftPauseResponseDto()
            {
                PausedAt = "2020-10-09T10:22:00Z"
            };
        }
    }
}