using System;

namespace CourierAPI.Infrastructure.Exceptions
{
    [Serializable]
    public class AppException : Exception
    {
        public AppMessage AppMessage { get; }

        public AppException(AppMessage appMessage)
            : base(appMessage.ToString())
        {
            AppMessage = appMessage;
        }
    }
}