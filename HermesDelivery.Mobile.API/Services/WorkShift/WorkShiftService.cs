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

namespace CourierAPI.Services.WorkShift
{
    [Authorize]
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
            var items = new List<WorkShiftHistoryDto>()
            {
                new WorkShiftHistoryDto()
                {
                    StartedAt = "2020-10-09T09:00:00Z",
                    StoppedAt = "2020-10-09T18:00:00Z",
                    Distance = "45км.",
                    Orders = 84
                }
            };

            return _mapper.Map<IEnumerable<WorkShiftHistoryDto>>(items);
        }

        /// <summary>
        /// Возвращает текущую рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о текущей рабочей смене.</returns>
        public async Task<WorkShiftDto> GetCurrent()
        {
            var item = new WorkShiftDto()
            {
                IsStarted = false, // true, если смена начата
                StartAt = "2020-10-09T09:00:00Z",
                CloseAt = "2020-10-09T18:00:00Z",
                PausedAt = "2020-10-09T18:00:00Z", // null, если не на наузе
                OrderCount = new OrderCountDto()
                {
                    Active = 5,
                    Delivered = 12,
                    Rejected = 1,
                    Total = 18
                },
                Orders = new List<WorkShiftOrderDto> {
                               new WorkShiftOrderDto() {
                                   Id =  1,
                                   VendorLogo =  "https://admin.kenguru.tj/files/nKv1ahUMjE2XcxgCVr6yHA.jpg",
                                   ClientName =  "Азамат",
                                   Status =  "Готовится",
                                   TotalCost =  (decimal) 234.00
                    }
                }
            };

            return _mapper.Map<WorkShiftDto>(item);
        }

        /// <summary>
        /// Запускает рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о запущенной рабочей смене.</returns>
        public async Task<WorkShiftDto> Start()
        {
            return await this.GetCurrent();
        }

        /// <summary>
        /// Завершает рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о следующей рабочей смене.</returns>
        public async Task<WorkShiftDto> End()
        {
            return new WorkShiftDto();
        }

        /// <summary>
        /// Завершает рабочую смену.
        /// </summary>
        /// <returns>Возвращает информацию о следующей рабочей смене.</returns>
        public async Task<WorkShiftPauseResponseDto> Pause(WorkShiftPauseRequestDto model)
        {
            return new WorkShiftPauseResponseDto()
            {
                PausedAt = DateTime.Now.ToString(CultureInfo.InvariantCulture)
            };
        }
    }
}