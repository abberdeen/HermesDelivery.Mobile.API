using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace CourierAPI.Services.Sms.External
{
    internal interface ISmsService
    {
        Task<JObject> SendMessage(string phoneNumber, string message);

        decimal? Balance { get; }
    }
}