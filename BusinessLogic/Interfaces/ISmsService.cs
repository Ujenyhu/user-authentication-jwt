namespace userauthjwt.BusinessLogic.Interfaces
{
    public interface ISmsService
    {
        Task<bool> SendTermiiSms(string sToTelephone, string sMessage);
        Task<bool> SendTwilioSms(string sToTelephone, string sMessage);
    }
}
