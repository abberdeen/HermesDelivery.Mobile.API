using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CourierAPI.Services.Sms.External
{
    public class OsonSmsService : SmsService
    {
        public OsonSmsService(Uri baseUrl, string sender, string login, string password) : base(baseUrl, sender, login, password)
        {
        }

        public override decimal? Balance
        {
            get
            {
                decimal? balance = null;
                var config = GetConfig();
                var txnId = Guid.NewGuid().ToString();
                JObject joResponse3 = CheckBalance(GetConfig(), txnId); // Проверка баланса
                if (joResponse3["error"] == null)
                {
                    balance = decimal.Parse(joResponse3["balance"].ToString());
                }
                return balance;
            }
        }

        public override async Task<JObject> SendMessage(string phoneNumber, string message)
        {
            var config = GetConfig();

            var txnId = Guid.NewGuid().ToString();

            JObject joResponse = await SendSMS(config, phoneNumber, message, txnId); // Отправка СМС сообщения

            return joResponse;
        }

        #region "Oson SMS"

        private IDictionary<string, string> GetConfig()
        {
            IDictionary<string, string> config = new Dictionary<string, string>();
            config["dlm"] = Delimeter; // не надо менять!!!
            config["t"] = "23"; // не надо менять!!!
            config["login"] = Login; // Ваш логин
            config["pass_hash"] = Password; // Ваш хэш код
            config["sender"] = Sender; // Ваш алфанумерик
            return config;
        }

        [JsonObject(MemberSerialization.OptIn)]
        private struct oSendMessage
        {
            [JsonProperty("status")]
            public string status { get; set; }

            [JsonProperty("timestamp")]
            public DateTime timestamp { get; set; }

            [JsonProperty("txn_id")]
            public string txn_id { get; set; }

            [JsonProperty("msg_id")]
            public string msg_id { get; set; }

            [JsonProperty("smsc_msg_id")]
            public string smsc_msg_id { get; set; }

            [JsonProperty("smsc_msg_status")]
            public string smsc_msg_status { get; set; }

            [JsonProperty("smsc_msg_parts")]
            public string smsc_msg_parts { get; set; }
        }

        private async Task<JObject> SendSMS(IDictionary<string, string> config, string phone_number, string msg, string txn_id)
        {
            //var txn_id = "test_12783"; // Должен быть уникальным для каждого запроса
            var str_hash = Sha256Hash(txn_id + config["dlm"] + config["login"] + config["dlm"] + config["sender"] + config["dlm"] + phone_number + config["dlm"] + config["pass_hash"]);

            var client = new RestClient(new Uri(BaseUrl, "sendsms_v1.php"));
            var request = new RestRequest(Method.GET);
            request.AddParameter("from", config["sender"]);
            request.AddParameter("login", config["login"]);
            request.AddParameter("t", config["t"]);
            request.AddParameter("phone_number", phone_number);
            request.AddParameter("msg", msg);
            request.AddParameter("str_hash", str_hash);
            request.AddParameter("txn_id", txn_id);

            IRestResponse response = await client.ExecuteAsync(request);
            var content = response.Content; // raw content as string
            /*
            * Console.WriteLine(content);
            * Ответ сервера при успешной отправки сообщения
            * { "status":"ok","timestamp":"2017-12-13 15:12:15","txn_id":"test_1233","msg_id":266083,"smsc_msg_id":"5783F3B6","smsc_msg_status":"success","smsc_msg_parts":1}
            * При ошибке
            * {"error":{"code":108,"msg":"Duplicate txn_id. It should be unique.","timestamp":"2017-12-13 15:14:34"}}
            */

            JObject joResponse = null;
            if (content != null && !string.IsNullOrEmpty(content))
            {
                joResponse = JObject.Parse(content);
            }

            return joResponse;
        }

        public JObject CheckSMSStatus(IDictionary<string, string> config, string msg_id)
        {
            // Параметры, которые могут меняться каждый раз
            var txn_id = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds; // Должен быть уникальным для каждого запроса
            var str_hash = Sha256Hash(config["login"] + config["dlm"] + txn_id + config["dlm"] + config["pass_hash"]);

            var client = new RestClient(new Uri(BaseUrl, "query_sms.php"));
            var request = new RestRequest(Method.GET);
            request.AddParameter("t", config["t"]);
            request.AddParameter("login", config["login"]);
            request.AddParameter("msg_id", msg_id);
            request.AddParameter("str_hash", str_hash);
            request.AddParameter("txn_id", txn_id);

            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string
            /*
            * Console.WriteLine(content);
            * Ответ сервера при успешной отправки сообщения
            * {"message_id": "57861909", "final_date": {"date": "2017-12-13 16:44:30.000000", "timezone_type": 1, "timezone": "+05:00"}, "message_state_code": 2, "error_code": 0, "message_state": "Delivered"}
            * При ошибке
            * {"error":{"code":107,"msg":"Message ID is invalid","timestamp":"2017-12-13 16:58:37"}}
            */
            JObject joResponse = null;
            if (content != null && !string.IsNullOrEmpty(content))
            {
                joResponse = JObject.Parse(content);
            }

            return joResponse;
        }

        private JObject CheckBalance(IDictionary<string, string> config, string txn_id)
        {
            // Параметры, которые могут меняться каждый раз
            //var txn_id = "test_12342"; // Должен быть уникальным для каждого запроса
            var str_hash = Sha256Hash(txn_id + config["dlm"] + config["login"] + config["dlm"] + config["pass_hash"]);

            var client = new RestClient(new Uri(BaseUrl, "check_balance.php"));
            var request = new RestRequest(Method.GET);
            request.AddParameter("t", config["t"]);
            request.AddParameter("login", config["login"]);
            request.AddParameter("str_hash", str_hash);
            request.AddParameter("txn_id", txn_id);

            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string
            /*
             * Console.WriteLine(content);
             * Ответ сервера {"balance":6.07,"timestamp":"2017-12-13 15:51:00"}
             */

            JObject joResponse = null;
            if (content != null && !string.IsNullOrEmpty(content))
            {
                joResponse = JObject.Parse(content);
            }

            return joResponse;
        }

        private String Sha256Hash(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        #endregion "Oson SMS"
    }
}