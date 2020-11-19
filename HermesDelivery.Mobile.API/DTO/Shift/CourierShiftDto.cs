namespace CourierAPI.DTO.Shift
{
    /// <summary>
    /// Информация о текущем активном расписании рабочей смены.
    /// </summary>
    public class CurrentActiveCourierShiftDto
    {
        public int Id { get; set; }
        public int CourierId { get; set; }
        public int WorkShiftId { get; set; }
        public string Description { get; set; }
    }
}