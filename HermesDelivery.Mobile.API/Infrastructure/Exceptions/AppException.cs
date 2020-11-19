using System;
using CourierAPI.DTO;

namespace CourierAPI.Infrastructure.Exceptions
{
    [Serializable]
    public class AppException : Exception
    {
        public AppMessage AppMessage { get; }

        public string Description { get; } 

        public AppException(AppMessage appMessage, string description = null)
            : base(appMessage.ToString()  )
        {
            AppMessage = appMessage;
            Description = description;
        }
    }
}