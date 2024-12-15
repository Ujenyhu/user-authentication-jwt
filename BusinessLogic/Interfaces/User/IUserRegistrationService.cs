using userauthjwt.Requests.User;
using userauthjwt.Responses;
using userauthjwt.Responses.User;

namespace userauthjwt.BusinessLogic.Interfaces.User
{
    public interface IUserRegistrationService
    {
        Task<ResponseBase<SignUpResponse>> SignUp(SignUpRequest _Request);
        Task<ResponseBase<object>> VerifyOtpReg(VerifyOtpRequest request);
        Task<ResponseBase<object>> SendOtpReg(OtpRequest request);
    }
}
