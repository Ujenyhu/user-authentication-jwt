using userauthjwt.DataAccess.Interfaces;
using userauthjwt.Helpers;
using MimeKit;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using userauthjwt.BusinessLogic.Interfaces.User;
using userauthjwt.BusinessLogic.Interfaces;

namespace userauthjwt.BusinessLogic.Services
{
    public class MailService : IMailService
    {
        private IRepositoryWrapper _repository;
        private readonly IWebHostEnvironment _environment;
        private readonly IAuthenticationService _authenticationService;
        public MailService(IRepositoryWrapper repository,
            IAuthenticationService authenticationService,
             IWebHostEnvironment environment)
        {
            _repository = repository;
            _authenticationService = authenticationService;
            _environment = environment;
        }

        /*This method id for the default .net SMTP Implementation */
        //public async Task<bool> SendMail(string sTo, string sSubject, string sHtmlBody)
        //{
        //    try
        //    {
        //        var _MailConfig = await _repository.SysConfigRepository.GetMailConfigFirstOrDefaultAsync();

        //        if (_MailConfig == null)
        //        {
        //            // FriendlyErrorMessage = "Missing mail server configuration";
        //            return false;
        //        }

        //        using (var mm =  new MailMessage())
        //        {
        //            mm.From = new MailAddress(_MailConfig.DefaultEmail);
        //            mm.To.Add(sTo);
        //            mm.Subject = sSubject;
        //            mm.Body = sHtmlBody;
        //            mm.IsBodyHtml = true;

        //            using (var smtp = new SmtpClient(_MailConfig.SmtpServer))
        //            {
        //                smtp.EnableSsl = _MailConfig.SslSupport != 0;
        //                smtp.UseDefaultCredentials = false;
        //                smtp.Credentials = new NetworkCredential(_MailConfig.SmtpUsername, _MailConfig.SmtpPassword);
        //                if (_MailConfig.SmtpPort != 0)
        //                    smtp.Port = _MailConfig.SmtpPort;

        //                await smtp.SendMailAsync(mm);
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}


        //Customize your otp mail

        public async Task<bool> SendOtpMail(string sTo, string Body)
        {
            try
            {

                var email = new Email
                {
                    MailTo = sTo,
                    Subject = "Confirmation code",
                    Body = Body
                };

                //Send it to this mimekit method to be handled
                return await SendMailAsync(email);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //Customize your otp mail
        public async Task<bool> SendSecurityMail(string sTo, string securityType)
        {
            try
            {

                //Set and ovveride params and customize body of email if using an html file
                string Body;
                string mailTo = sTo;
                string mailSubject = "Security Alert";
                string HtmlTemplateFile = "securitymail.html";
                string HtmlTemplateFolder = "templates";
                var htmlTemplatePath = Path.Combine(_environment.WebRootPath, HtmlTemplateFolder);
                using (StreamReader reader = new StreamReader(Path.Combine(htmlTemplatePath, HtmlTemplateFile)))
                {
                    Body = reader.ReadToEnd();
                }

                var ipInfo = await _authenticationService.GetIPInfo();

                // Check for null reference for ipInfo before accessing its properties
                var locationInfo = ipInfo != null
                    ? $"{ipInfo.city ?? "Unknown City"}, {ipInfo.regionName ?? "Unknown State"}, {ipInfo.country ?? "Unknown Country"}"
                    : "Unknown Location";

                var ipAddress = ipInfo?.query ?? "Unknown IP";

                var userAgent = _authenticationService.GetBrowserInfo()?.UserAgent ?? "Unknown Device";

                string securityMessage = string.Empty;

                switch (securityType)
                {
                    case "LOGIN":
                        securityMessage = VarHelper.loginSecurityMsg;
                        break;
                    case "PASSWORDCHANGE":
                        securityMessage = VarHelper.PasswordChangeSecurityMsg;
                        break;
                }


                Body = Body.Replace("{MESSAGE}", securityMessage);
                Body = Body.Replace("{Location}", locationInfo);
                Body = Body.Replace("{IP}", ipAddress);
                Body = Body.Replace("{Device}", userAgent);
                Body = Body.Replace("{Date}", DateTime.Now.ToString("MMMM dd, yyyy hh:mm:ss tt"));
                Body = Body.Replace("{YEAR}", DateTime.Now.Year.ToString());

                var email = new Email
                {
                    MailTo = mailTo,
                    Subject = mailSubject,
                    Body = Body
                };
                //Send it to the mimekit method to be handled

                return await SendMailAsync(email);
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        //Mime message implementation
        private async Task<bool> SendMailAsync(Email email)
        {
            var _MailConfig = _repository.SysConfigRepository.GetMailConfigFirstOrDefaultAsync().GetAwaiter().GetResult();

            if (_MailConfig == null) return false;

            email.Sender = _MailConfig.DefaultEmail;
            using var client = new SmtpClient();
            await client.ConnectAsync(host: _MailConfig.SmtpServer,
                port: _MailConfig.SmtpPort,
                options: MailKit.Security.SecureSocketOptions.StartTls);
            try
            {
                await client.AuthenticateAsync(_MailConfig.SmtpUsername, _MailConfig.SmtpPassword);
                await client.SendAsync(CreateMimeMessage(email));
                await client.DisconnectAsync(true);
                client.Dispose();
                return true;
            }
            catch(Exception ex){
                return false;
            }
        }


        private MimeMessage CreateMimeMessage(Email email)
        {
            var message = new MimeMessage()
            {
                Subject = email.Subject,
                Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = email.Body
                }
            };

            message.From.Add(new MailboxAddress("Sender", email.Sender));
            message.To.Add(new MailboxAddress("Reciever", email.MailTo));
            return message;

        }

        private class Email
        {
            public string? Sender { get; set; }
            public string MailTo { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        }

    }
}
