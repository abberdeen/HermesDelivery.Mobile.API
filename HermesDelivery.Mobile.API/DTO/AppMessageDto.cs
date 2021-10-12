namespace CourierAPI.DTO
{
    public class AppMessageDto
    {
        /// <summary>
        ///
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Text { get; set; }

        public AppMessageDto(AppMessage message)
        {
            Code = message.Code;
            Text = message.Description;
        }

        public AppMessageDto(AppMessage message, string details)
        {
            Code = message.Code;
            Text = message.Description + System.Environment.NewLine + details;
        }
    }
}