using Azure;
using Microsoft.EntityFrameworkCore;
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.Helpers;
using userauthjwt.Models;
using userauthjwt.Models.User;
using userauthjwt.Requests;
using userauthjwt.Responses;

namespace userauthjwt.DataAccess.Repository
{
    public class UserRepository: GenericRepository<UserProfile>, IUserRepository
    {
        private AppDbContext _context;
        public UserRepository(AppDbContext context) : base(context) => _context = context;


        public async Task AddOtpRecordAsync(OtpRecord request)
        {
            await _context.AddAsync(request);
        }

        public async Task UpdateOtpRecord(OtpRecord model)
        {
            _context.Update(model);
        }

        public async Task<OtpRecord> GetNewestOtpRecord(string UserId)
        {
            var record = await _context.OtpRecord
                .Where(x => x.UserId == UserId)
                .OrderByDescending(o => o.ExpirationTime)
                .FirstOrDefaultAsync();

            return record;
        }
        public async Task AddFailedSecurityAttemptsAsync(FailedSecurityAttempt request)
        {
            await _context.AddAsync(request);

        }


        //public async Task<ResponseBase<SignUpResponse>> SignUp(SignUpRequest request)
        //{
        //    var response = new ResponseBase<SignUpResponse>();

        //    //salt or secure password
        //    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(AppHelper.GetUnique8ByteKey());
        //    string sSalt = Convert.ToBase64String(plainTextBytes);
        //    string passwordHash = AppHelper.HashPassword(request.Password, sSalt);

        //    //create user record
        //    UserProfile _UserProfile = new UserProfile
        //    {
        //        UserId = AppHelper.GetNewUniqueId(),
        //        Email = request.EmailAddress,
        //        Firstname = request.Firstname,
        //        Lastname = request.Lastname,
        //        PasswordSalt = sSalt,
        //        Password = passwordHash,
        //    };

        //    try
        //    {
        //        await _context.AddAsync(_UserProfile);
        //        await _context.SaveChangesAsync();

        //        //Response Data
        //        var responseData = new SignUpResponse()
        //        {
        //            UserId = _UserProfile.UserId,
        //            EmailAddress = _UserProfile.Email,
        //        };

        //        response.Data = responseData;
        //        response.StatusCode = StatusCodes.Status200OK;
        //        response.Message = "Registration was successful.";
        //        response.Status = VarHelper.ResponseStatus.SUCCESS.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        response.StatusCode = StatusCodes.Status500InternalServerError;
        //        response.Message = $"Registration was unsuccessful.{ex.Message}";
        //        response.Status = VarHelper.ResponseStatus.ERROR.ToString();
        //    }
        //    return response;
        //}

        //public async Task<ResponseBase<SignInResponse>> SignIn(UserProfile user)
        //{
        //    var response = new ResponseBase<SignInResponse>();

        //    //create response object
        //    SignInResponse loginUser = new SignInResponse
        //    {
        //        UserId = user.UserId,
        //        EmailAddress = user.Email
        //    };

        //    //generate authentication token if user pass all checks
        //    var getToken = await Task.Run(() => TokenHelper.GenerateToken(loginUser));
        //    var refreshToken = TokenHelper.GenerateRefreshToken();

        //    //Update the database with the new refresh token and expiration date
        //    user.RefreshToken = refreshToken;
        //    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

        //    try
        //    {
        //        //Update and Save changes
        //        _context.Update(user).Property(x => x.UserId).IsModified = false;
        //        await CompleteAsync();

        //        loginUser.Token = getToken;
        //        loginUser.RefreshToken = refreshToken;
        //        loginUser.TokenExpirationDate = DateTime.Now.AddDays(1);
        //        loginUser.Firstname = user.Firstname;
        //        loginUser.Lastname = user.Lastname;

        //        response.Data = loginUser;
        //        response.StatusCode = StatusCodes.Status200OK;
        //        response.Message = "Login successful.";
        //        response.Status = VarHelper.ResponseStatus.SUCCESS.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        response.StatusCode = StatusCodes.Status400BadRequest;
        //        response.Message = "Login not successful.";
        //        response.Status = VarHelper.ResponseStatus.ERROR.ToString();
        //    }
        //    return response;
        //}

        //public async Task<ResponseBase<AuthenticationResponse>> Refresh(TokenRequest request)
        //{
        //    var response = new ResponseBase<AuthenticationResponse>();

        //    string accessToken = request.AccessToken;
        //    string refreshToken = request.RefreshToken;

        //    //verify the access token provided
        //    var principal = TokenHelper.GetPrincipalFromExpiredToken(accessToken);
        //    var userId = principal.Identity.Name; //this is mapped to the Name claim upon token generation

        //    var user = await _context.UserProfile.SingleOrDefaultAsync(u => u.UserId == userId);

        //    if (user == null)
        //    {
        //        response.StatusCode = StatusCodes.Status400BadRequest;
        //        response.Message = "Invalid client request.";
        //        response.Status = VarHelper.ResponseStatus.ERROR.ToString();
        //    }
        //    if (user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
        //    {
        //        response.StatusCode = StatusCodes.Status400BadRequest;
        //        response.Message = "Invalid client request.";
        //        response.Status = VarHelper.ResponseStatus.ERROR.ToString();
        //    }

        //    var newAccessToken = TokenHelper.GenerateAccessToken(principal.Claims);
        //    var newRefreshToken = TokenHelper.GenerateRefreshToken();

        //    //Update the database with the new refresh token and expiration date
        //    user.RefreshToken = newRefreshToken;
        //    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

        //    try
        //    {
        //        //Update and Save changes
        //        _context.Update(user).Property(x => x.UserId).IsModified = false;
        //        await CompleteAsync();


        //        //assign data to the response object
        //        var responseData = new AuthenticationResponse()
        //        {
        //            Token = newAccessToken,
        //            RefreshToken = newRefreshToken,
        //        };

        //        response.Data = responseData;
        //        response.StatusCode = StatusCodes.Status200OK;
        //        response.Message = "Token refreshed.";
        //        response.Status = VarHelper.ResponseStatus.SUCCESS.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        response.StatusCode = StatusCodes.Status400BadRequest;
        //        response.Message = $"Token refresh was unsuccessful.{ex.Message}";
        //        response.Status = VarHelper.ResponseStatus.ERROR.ToString();
        //    }
        //    return response;
        //}

        //public async Task<ResponseBase<T>> UpdateUser(UpdateUserRequest request)
        //{
        //    var response = new ResponseBase<T>();


        //    //get user details from the database
        //    var _UserProfile = await _context.UserProfile.SingleOrDefaultAsync(x => x.UserId == request.UserId);
        //    if (_UserProfile == null)
        //    {
        //        response.StatusCode = StatusCodes.Status400BadRequest;
        //        response.Message = "Invalid UserId. user record not found";
        //        response.Status = VarHelper.ResponseStatus.ERROR.ToString();
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(request.Firstname))
        //        {
        //            _UserProfile.Firstname = request.Firstname;
        //        }
        //        if (!string.IsNullOrEmpty(request.Lastname))
        //        {
        //            _UserProfile.Lastname = request.Lastname;
        //        }

        //        try
        //        {
        //            //Update and Save changes
        //            _context.Update(_UserProfile).Property(x => x.UserId).IsModified = false;
        //            await CompleteAsync();

        //            response.StatusCode = StatusCodes.Status200OK;
        //            response.Message = "UserProfile Successfully Updated.";
        //            response.Status = VarHelper.ResponseStatus.SUCCESS.ToString();
        //        }
        //        catch (Exception ex)
        //        {
        //            response.StatusCode = StatusCodes.Status400BadRequest;
        //            response.Message = $"UserProfile not updated.{ex.Message}";
        //            response.Status = VarHelper.ResponseStatus.ERROR.ToString();
        //        }
        //    }
        //    return response;

        //}

        //public async Task<ResponseBase<T>> ChangePassword(ChangePasswordRequest request)
        //{
        //    var response = new ResponseBase<T>();

        //    //get user details
        //    var userProfile = await _context.UserProfile.SingleOrDefaultAsync(x => x.UserId == request.UserId);
        //    if (userProfile == null)
        //    {
        //        response.StatusCode = StatusCodes.Status404NotFound;
        //        response.Message = "Invalid UserId. user record not found";
        //        response.Status = VarHelper.ResponseStatus.ERROR.ToString();
        //    }

        //    //confirm if the old password provided is correct
        //    var passwordHash = AppHelper.HashPassword(request.OldPassword, userProfile.PasswordSalt);
        //    if (userProfile.Password != passwordHash)
        //    {
        //        response.StatusCode = StatusCodes.Status400BadRequest;
        //        response.Message = "Old Password provided does not match Current Password.";
        //        response.Status = VarHelper.ResponseStatus.ERROR.ToString();
        //    }

        //    //Secure new password

        //    var bytes = System.Text.Encoding.UTF8.GetBytes(AppHelper.GetUnique8ByteKey());
        //    string sSalt = Convert.ToBase64String(bytes);
        //    string hash = AppHelper.HashPassword(request.NewPassword, sSalt);

        //    //Update and save new password
        //    userProfile.Password = hash;
        //    userProfile.PasswordSalt = sSalt;
        //    try
        //    {
        //        _context.Update(userProfile);
        //        await CompleteAsync();

        //        response.StatusCode = StatusCodes.Status200OK;
        //        response.Message = "Password changed successfully.";
        //        response.Status = VarHelper.ResponseStatus.SUCCESS.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        response.StatusCode = StatusCodes.Status400BadRequest;
        //        response.Message = $"Could not change password.{ex.Message}";
        //        response.Status = VarHelper.ResponseStatus.ERROR.ToString();
        //    }
        //    return response;
        //}
    }
}
