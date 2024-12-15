

namespace userauthjwt.BusinessLogic.Interfaces
{
    public interface IMailService
    {
        //Task<bool> SendMail(string sTo, string sSubject, string sHtmlBody);
        Task<bool> SendOtpMail(string sTo, string Body);
        Task<bool> SendSecurityMail(string sTo, string securityType);

        //Task<bool> SendMailAsync(Email email);

    }
}
