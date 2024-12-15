
using userauthjwt.BusinessLogic.Interfaces.User;

namespace userauthjwt.BusinessLogic.Interfaces
{
    public interface IServicesWrapper
    {
        IAuthenticationService AuthenticationService { get; }
        IUserService UserService { get; }
        IUserRegistrationService UserRegistrationService { get; }
        IMailService MailService { get; }
        ISmsService SmsService { get; }
        ILookupService LookupService { get; }
    }
}
