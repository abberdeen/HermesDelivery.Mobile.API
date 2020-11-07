using System.Collections.Generic;
using CourierAPI.Models.DTO.Orders;

namespace CourierAPI.Models.DTO.WorkShifts
{
    /// <summary>
    /// Информация о рабочей смене.
    /// </summary>
    public class CourierWorkShiftsItemDto
    {
        public bool IsStarted { get; set; }
        public string StartAt { get; set; }
        public string CloseAt { get; set; }
        public string PausedAt { get; set; }
        public OrderCountDto OrderCount { get; set; }
        public List<WorkShiftOrderDto> Orders { get; set; }
    }

    /// <summary>
    /// Элемент списка. Информация о рабочей смене.
    /// </summary>
    public class CourierWorkShiftsItemHistoryDto
    {
        public string StartedAt { get; set; }
        public string StoppedAt { get; set; }
        public string Distance { get; set; }
        public int Orders { get; set; }
    }

    /// <summary>
    /// Для запроса на приостановку рабочей смены.
    /// </summary>
    public class CourierWorkShiftsItemPauseRequestDto
    {
        public int ReasonId { get; set; }
        public string Comment { get; set; }
    }

    /// <summary>
    /// Для ответа на запрос о приостановке смены.
    /// </summary>
    public class CourierWorkShiftsItemPauseResponseDto
    {
        public string PausedAt { get; set; }
    }
}