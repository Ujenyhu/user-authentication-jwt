
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using userauthjwt.BusinessLogic.Interfaces;
using userauthjwt.DataAccess.Interfaces;

namespace userauthjwt.BusinessLogic.Services
{
    public class SmsService : ISmsService
    {
        private IRepositoryWrapper _repository;
        private JsonSerializer _serializer = new JsonSerializer();
        private HttpClient client;
        public SmsService(IRepositoryWrapper repository)
        {
            _repository = repository;
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }



        public async Task<bool> SendTermiiSms(string sToTelephone, string sMessage)
        {
            try
            {
                var _SmsConfig = await _repository.SysConfigRepository.GetSmsConfigFirstOrDefaultAsync();

                if (_SmsConfig == null)
                {
                    // FriendlyErrorMessage = "Missing mail server configuration";
                    return false;
                }

                //Creating Json object
                TermiiSmsBody objectBody = new TermiiSmsBody()
                {
                    to = sToTelephone,
                    from = _SmsConfig.SenderId,
                    sms = sMessage,
                    api_key = _SmsConfig.ApiKey,
                    type = _SmsConfig.Type,
                    channel = _SmsConfig.Channel
                };

                var content = new StringContent(JsonConvert.SerializeObject(objectBody), Encoding.UTF8, "application/json");
                client.BaseAddress = new Uri("https://v3.api.termii.com/api/sms/send");

                var response = await client.PostAsync(client.BaseAddress, content);
                var str = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) return true;

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SendTwilioSms( string sToTelephone, string sMessage)

        {
            try
            {
                var _SmsConfig = await _repository.SysConfigRepository.GetSmsConfigFirstOrDefaultAsync();

                if (_SmsConfig == null)
                {
                    // FriendlyErrorMessage = "Missing mail server configuration";
                    return false;
                }
                string sAccountSid = _SmsConfig.Type;
                string sTokenAuth = _SmsConfig.ApiKey;
                string sFromNumber = _SmsConfig.SenderId;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                TwilioClient.Init(sAccountSid, sTokenAuth);
                var message = MessageResource.Create(
                    body: sMessage,
                    from: new Twilio.Types.PhoneNumber(sFromNumber),
                    //to: new Twilio.Types.PhoneNumber(sToTelephone)
                    to: new Twilio.Types.PhoneNumber($"+{sToTelephone}")
                );
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private class TermiiSmsBody
        {
            public string to { get; set; }
            public string from { get; set; }
            public string sms { get; set; }
            public string type { get; set; }
            public string channel { get; set; }
            public string api_key { get; set; }
        }
    }
}
