using AutoMapper;
using CourierAPI.DTO;
using CourierAPI.DTO.Orders;
using CourierAPI.DTO.Shift;
using CourierAPI.Infrastructure.Database;
using CourierAPI.Infrastructure.Exceptions;
using CourierAPI.Infrastructure.Extensions;
using CourierAPI.Services.Mock;
using CourierAPI.Services.Order;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using OrderStatusCode = HermesDAdmin.Dto.Orders.OrderStatusCode;

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
        private readonly SupplierInfoService _supplierInfoService;
        private readonly OrderInfoService _orderInfoSrv;
        private readonly CourierShiftService _courierWorkShiftsScheduleService;

        public CourierShiftHistoryService(
            ILogger logger,
            IMapper mapper,
            SupplierInfoService supplierInfoService,
            OrderInfoService orderInfoSrv,
            CourierShiftService courierWorkShiftsScheduleService)
        {
            _dbContext = new AppDbContext();
            _logger = logger;
            _mapper = mapper;
            _supplierInfoService = supplierInfoService;
            _orderInfoSrv = orderInfoSrv;
            _courierWorkShiftsScheduleService = courierWorkShiftsScheduleService;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns>Возвращает историю рабочих смен.</returns>
        public async Task<IEnumerable<CourierShiftHistoryListItemDto>> GetHistoryAsync(int courierId)
        {
            var courierWorkShiftItems = await _dbContext.CourierShiftHistories 
                .Where(x =>
                    x.CourierShift.CourierId== courierId &&
                    x.IsEnded)
                .Select(x=> new
                {
                    x.StartTime,
                    x.EndTime,
                    OrderCount = x.Orders.Count(z => z.OrderStatusCode.IsFinal)
                }) 
                .ToListAsync();

            var list = courierWorkShiftItems.Select(x=> new CourierShiftHistoryListItemDto
            {
                StartedAt = x.StartTime?.Format(),
                StoppedAt = x.EndTime?.Format(),
                Distance = null, 
                Orders = x.OrderCount
            }); 

            return list;
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

            var item = await MapCourierShiftHistoryToDTO(courierId, currentOrNextCourierShiftHistory);

            return item;
        }

        private async Task<CourierShiftHistoryDto> MapCourierShiftHistoryToDTO(int courierId, CourierShiftHistory courierShiftHistory)
        {
            var orderCountDto = new OrderCountDto
            {
                Active = _dbContext.Orders
                    .Count(x =>
                        x.OrderStatusCode.IsFinal == false &&
                        x.CourierShiftHistoryId == courierShiftHistory.Id),

                Delivered = _dbContext.Orders
                    .Count(x =>
                        x.OrderStatusCodeId == (int)OrderStatusCode.Completed &&
                        x.CourierShiftHistoryId == courierShiftHistory.Id),

                Rejected = _dbContext.IncomingOrderHistories
                    .Count(x => 
                        x.StatusId == (int) IncomingOrderStatuses.Rejected &&
                        x.CourierShiftHistoryId == courierShiftHistory.Id),
                Total = 0
            };

            orderCountDto.Total = orderCountDto.Rejected + orderCountDto.Active + orderCountDto.Delivered;

            // Текущая смена курьера.

            // Получает время начала.
            var shiftStartTime = courierShiftHistory.CourierShift.Shift.StartTime;

            // Получает время завершения.
            var shiftEndTime = courierShiftHistory.CourierShift.Shift.EndTime;

            // Создаст дату и время начала.
            var startDateTime = CalcStartDateTime(shiftStartTime, shiftEndTime);

            // Создаст дату и время завершения.
            var endDateTime = CalcEndDateTime(shiftStartTime, shiftEndTime, startDateTime);

            // Создаст и заполнит запись для возвращения.
            var item = new CourierShiftHistoryDto
            {
                IsStarted = courierShiftHistory.IsStarted,
                StartedAt = courierShiftHistory.StartTime?.Format(),
                StartAt = startDateTime.Format(),
                CloseAt = endDateTime.Format(),
                PausedAt = courierShiftHistory.PauseTime?.Format(),
                OrderCount = orderCountDto,
                Orders = await GetShiftActiveOrderList(courierId, courierShiftHistory.Id)
            };
            return item;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="courierId"></param>
        /// <returns></returns>
        public async Task<bool> AnyPendingOrActiveOrder(int courierId)
        {
            // 
            var anyPendingOrder = await _dbContext.Orders
                .AnyAsync(x =>
                    x.CourierId == courierId &&
                    x.OrderStatusCode.IsFinal == false &&
                    x.CourierShiftHistoryId.HasValue == false);

            // Выполняет запрос на получение записи элемента текущей смены.
            var currentOrNextCourierShiftHistory = await FindCurrentOrNextAsync(courierId);

            var anyActiveOrder =await _dbContext.Orders.AnyAsync(x =>
                x.CourierId == courierId &&
                x.CourierShiftHistoryId == currentOrNextCourierShiftHistory.Id &&
                x.OrderStatusCode.IsFinal == false);

            return anyPendingOrder || anyActiveOrder;
        }
         
        public async Task<List<ShiftOrderDto>> GetShiftActiveOrderList(int courierId, int courierShiftHistoryId)
        {
            var orders = _dbContext.Orders.Where(x =>
                x.CourierId == courierId &&
                x.CourierShiftHistoryId == courierShiftHistoryId &&
                x.OrderStatusCode.IsFinal == false);

            var shiftOrderList = new List<ShiftOrderDto>();
            foreach (var order in orders)
            {
                var supplierInfo = await _supplierInfoService.GetSupplierInfo(order.Id);

                var orderInfo = await _orderInfoSrv.GetOrderInfoAsync(order.Id);
                var item = new ShiftOrderDto
                {
                    Id = order.Id,
                    VendorLogo = supplierInfo.Logo,
                    ClientName = order.Customer.Name,
                    Status = order.OrderStatusCode.Text,
                    TotalCost = orderInfo.Total
                };
                shiftOrderList.Add(item);
            }

            return shiftOrderList;
        } 

        private DateTime CalcStartDateTime(TimeSpan shiftStartTime, TimeSpan shiftEndTime)
        {
            // Создаст дату и время начала.
            var startDateTime = DateTime.Now.Date.Add(shiftStartTime);

            // Если начало смены приходится на следующий день.
            if (DateTime.Now.TimeOfDay > shiftEndTime)
            {
                startDateTime = startDateTime.AddDays(1);
            }

            return startDateTime;
        }

        private DateTime CalcEndDateTime(TimeSpan shiftStartTime, TimeSpan shiftEndTime, DateTime startDateTime)
        {
            DateTime endDateTime;

            // Проверяет переходит ли время завершения смены на новый день.
            if (shiftStartTime < shiftEndTime)
            {
                endDateTime = startDateTime.Date.Add(shiftEndTime);
            }
            else
            {
                endDateTime = startDateTime.Date.AddDays(1).Add(shiftEndTime);
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

            if (await AnyPendingOrActiveOrder(courierId))
            {
                throw new AppException(AppMessage.ClosingUncompletedShift);
            }

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

            if (await AnyPendingOrActiveOrder(courierId))
            {
                throw new AppException(AppMessage.ClosingUncompletedShift);
            }

            item.IsStarted = false;

            //
            item.IsPaused = true;
            item.PauseTime = DateTime.Now;
            item.PauseReasonId = model.ReasonId;
            item.PauseDescription = model.Comment;

            await _dbContext.SaveChangesAsync();


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
        public async Task<CourierShiftHistory> FindCurrentOrNextAsync(int courierId)
        {
            var courierShift = await _courierWorkShiftsScheduleService.GetCurrentOrNextActiveScheduleAsync(courierId);

            var currentCourierShiftHistory = await _dbContext.CourierShiftHistories
                .Include(x => x.CourierShift)
                .OrderByDescending(x =>
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
        /// <param name="shiftStartTime"></param>
        /// <param name="shiftEndTime"></param>
        /// <returns></returns>
        private bool IsTimeOfWorkShift(TimeSpan shiftStartTime, TimeSpan shiftEndTime)
        {
            // Создаст дату и время начала.
            var startDateTime = CalcStartDateTime(shiftStartTime, shiftEndTime);

            // Создаст дату и время завершения.
            var endDateTime = CalcEndDateTime(shiftStartTime, shiftEndTime, startDateTime);

            var dateTimeNow = DateTime.Now;
            return dateTimeNow >= startDateTime && dateTimeNow < endDateTime;
        }
    }
}