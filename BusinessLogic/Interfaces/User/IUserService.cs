using userauthjwt.Requests.User;
using userauthjwt.Responses;
using userauthjwt.Responses.User;

namespace userauthjwt.BusinessLogic.Interfaces.User
{
    public interface IUserService
    {
        Task<ResponseBase<SignInResponse>> SignIn(SignInRequest request);
        Task<ResponseBase<object>> VerifyOtp(VerifyOtpRequest request);
        Task<ResponseBase<object>> SendOtp(OtpRequest request);
        Task<ResponseBase<AuthenticationResponse>> Refresh(RefreshTokenRequest request);
        Task<ResponseBase<object>> Revoke();
        Task<ResponseBase<object>> ChangePassword(ChangePasswordRequest _Request);
        Task<ResponseBase<object>> ForgotPassword(ForgotPasswordRequest request);
        Task<ResponseBase<object>> UploadProfileImage(UploadImageRequest request);

        Task<ResponseBase<UserDetailsResponse>> GetUserProfileByUserId(string UserId);

    }
}
