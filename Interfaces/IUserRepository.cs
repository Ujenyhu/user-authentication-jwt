using userauthjwt.Models;
using userauthjwt.Requests;
using userauthjwt.Responses;

namespace userauthjwt.Interfaces
{
    public interface IUserRepository<T> : IGenericRepository<T> where T : class
    {
        Task<ResponseBase<SignUpResponse>> SignUp(SignUpRequest request);
        Task<ResponseBase<SignInResponse>> SignIn(UserProfile user);
        Task<ResponseBase<AuthenticationResponse>> Refresh(TokenRequest request);
        Task<ResponseBase<T>> UpdateUser(UpdateUserRequest request);
        Task<ResponseBase<T>> ChangePassword(ChangePasswordRequest request);
    }
}
