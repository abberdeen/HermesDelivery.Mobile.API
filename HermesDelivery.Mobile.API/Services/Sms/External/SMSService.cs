using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace CourierAPI.Services.Sms.External
{
    public abstract class SmsService : ISmsService
    {
        public Uri BaseUrl { get; set; }
        public string Sender { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Delimeter { get; set; }

        protected SmsService(Uri baseUrl, string sender, string login, string password)
        {
            BaseUrl = baseUrl;
            Sender = sender;
            Login = login;
            Password = password;
            Delimeter = ";";
        }

        public abstract decimal? Balance { get; }

        public abstract Task<JObject> SendMessage(string phoneNumber, string message);
    }
}