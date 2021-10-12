namespace CourierAPI.DTO.Shift
{
    /// <summary>
    /// Элемент списка. Причина приостановки рабочей смены.
    /// </summary>
    public class ShiftPauseReasonDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}