
using System.Net;
using System.Security.Claims;
using userauthjwt.BusinessLogic.Interfaces;
using userauthjwt.BusinessLogic.Interfaces.User;
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.Helpers;
using userauthjwt.Models.User;
using userauthjwt.Requests.User;
using userauthjwt.Responses;
using userauthjwt.Responses.User;

namespace userauthjwt.BusinessLogic.Services.User
{
    public class UserService : IUserService
    {
        private readonly IRepositoryWrapper _repository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMailService _mailService;
        private readonly ISmsService _smsService;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IHttpContextAccessor _httpContext;
        public UserService(IRepositoryWrapper repository,
            IAuthenticationService authenticationService,
            IUnitOfWork unitOfWork,
            IMailService mailService,
            ISmsService smsService,
            IConfiguration config,
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpcontext)
        {
            _repository = repository;
            _authenticationService = authenticationService;
            _unitOfWork = unitOfWork;
            _mailService = mailService;
            _smsService = smsService;
            _config = config;
            _httpContext = httpcontext;
            _WebHostEnvironment = webHostEnvironment;

        }

        public async Task<ResponseBase<SignInResponse>> SignIn(SignInRequest request)
        {
            //Never trust anything from the frontend
            if (request == null || string.IsNullOrWhiteSpace(request.EmailAddress) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new ResponseBase<SignInResponse>((int)HttpStatusCode.BadRequest, "Missing login details", VarHelper.ResponseStatus.ERROR.ToString());
            }

            //This is a reoccurring action and should be done before hitting an action- Use a middleware/Action filter for this
            var _SysConfig = await _repository.SysConfigRepository.FirstOrDefaultAsync();
            int iLoginExpiration = _SysConfig.LoginTokenExpiration;
            if (iLoginExpiration <= 0)
            {
                return new ResponseBase<SignInResponse>((int)HttpStatusCode.BadRequest, "We are currently undergoing maintenance. Check back in few minutes.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            //validate user credentials
            var userProfile = await _repository.UserRepository.FindByConditionAsync(user => user.EmailAddress == request.EmailAddress);
            if (userProfile == null)
            {
                return new ResponseBase<SignInResponse>((int)HttpStatusCode.NotFound, "Invalid login details.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            if (_authenticationService.IsAccountDisabled(userProfile))
            {
                return new ResponseBase<SignInResponse>((int)HttpStatusCode.BadRequest, $"You account is currently Disabled. Contact our customer care", VarHelper.ResponseStatus.ERROR.ToString());
            }

            // Check if the account is locked
            if (_authenticationService.IsAccountLocked(userProfile))
            {
                return new ResponseBase<SignInResponse>(419, $"Your account has been locked! To Unlock your account, click on forgot password.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            var passwordHash =  AppHelper.HashUsingPbkdf2(request.Password, userProfile.PasswordSalt);
            if (_authenticationService.IsValidCredentials(userProfile, passwordHash))
            {
                // Reset failed attempts
                _authenticationService.ResetFailedLoginAttempts(userProfile);

            }
            else
            {
                bool islocked = false;
                // Add failed attempts
                _authenticationService.IncrementFailedLoginAttempts(userProfile);
                if (userProfile != null && userProfile.FailedLoginAttempt >= _SysConfig.LoginAttemptMax)
                {
                    // Lock the account for 1 hour
                    _authenticationService.LockAccount(userProfile, (int)_SysConfig.LoginAttemptLockInHours);
                    islocked = true;
                }

                if (userProfile != null && userProfile.FailedLoginAttempt == _SysConfig.NotifyForLoginAttempt)
                {
                    await _mailService.SendSecurityMail(userProfile.EmailAddress, VarHelper.SecurityAttemptTypes.LOGIN.ToString());
                }

                await _unitOfWork.CommitAsync();

                var trialCount = _SysConfig.LoginAttemptMax - userProfile.FailedLoginAttempt;

                return new ResponseBase<SignInResponse>((int)HttpStatusCode.BadRequest, islocked ? $"Your account has been locked! To Unlock your account, click on forgot password." : $"Invalid Password, you have {trialCount} attempt(s) remaining!", VarHelper.ResponseStatus.ERROR.ToString());

            }


            string deviceType = string.Empty;

            if (_httpContext.HttpContext.Request.Headers.ContainsKey("DeviceType"))
            {
                deviceType = _httpContext.HttpContext.Request.Headers["DeviceType"].FirstOrDefault();
                userProfile.DeviceType = deviceType;
            }

            if (_httpContext.HttpContext.Request.Headers.ContainsKey("DeviceToken"))
            {
                var deviceTokenHeader = _httpContext.HttpContext.Request.Headers["DeviceToken"].FirstOrDefault();
                userProfile.DeviceToken = deviceTokenHeader;
            }

            if (_httpContext.HttpContext.Request.Headers.ContainsKey("DeviceId"))
            {
                var deviceId = _httpContext.HttpContext.Request.Headers["DeviceId"].FirstOrDefault();
                userProfile.DeviceId = deviceId;
            }

            var token = string.Empty;
            var refreshToken = string.Empty;

            SignInResponse response = new SignInResponse();

            //generate token
            token = await Task.Run(() => _authenticationService.GenerateToken(userProfile.UserId, userProfile.UserType, iLoginExpiration));

            refreshToken = _authenticationService.GenerateRefreshToken();

            if (string.IsNullOrWhiteSpace(deviceType))
            {
                userProfile.WebRefreshToken = refreshToken;
            }
            else
            {
                userProfile.MobileRefreshToken = refreshToken;
            }

            userProfile.RefreshTokenExpiryTime = DateTime.Now.AddDays(_SysConfig.RefreshTokenExpiration);

            response.Token = token;
            response.RefreshToken = refreshToken;
            response.TokenExpirationDate = DateTime.Now.AddMinutes(iLoginExpiration);
            response.UserId = userProfile.UserId;
            response.TelephoneVerified = userProfile.TelephoneVerified;
            response.EmailVerified = userProfile.EmailVerified;
            response.UserType = userProfile.UserType;
            await _repository.UserRepository.Update(userProfile);
            await _unitOfWork.CommitAsync();

            return new ResponseBase<SignInResponse>(response, (int)HttpStatusCode.OK, "Login successful.", VarHelper.ResponseStatus.SUCCESS.ToString());

        }


        //This is for fully onboarded users. This endpoint will send neccessary tokens if need be
        public async Task<ResponseBase<object>> SendOtp(OtpRequest request)
        {

            var userDetails = new UserProfile();

            //get userprofile using email or tel
            if (request.RequestType == VarHelper.OtpTypes.EMAIL.ToString()) userDetails = await _repository.UserRepository.FindByConditionAsync(user => user.EmailAddress == request.RequestSource);
            if (request.RequestType == VarHelper.OtpTypes.SMS.ToString()) userDetails = await _repository.UserRepository.FindByConditionAsync(user => user.Telephone == request.RequestSource);

            if (userDetails == null)
            {
                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, "Invalid User.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            //send verification SMS
            string sVeriCode = AppHelper.GenerateRandomNumber();

            var config = await _repository.SysConfigRepository.FirstOrDefaultAsync();

            bool sendSms = false; bool sendEmail = false;
            if (request.RequestType == VarHelper.OtpTypes.SMS.ToString())
            {
                //sendSms = await _smsService.SendTermiiSms(userDetails.Telephone, $"Your confirmation code is {sVeriCode}. Valid for {config.OtpTokenExpiration} minute, One-time use only.");
            }
            else
            {
                //send email
                string Body;
                string mailTo = userDetails.EmailAddress;
                string HtmlTemplateFile = "otp.html";
                string HtmlTemplateFolder = "templates";
                var htmlTemplatePath = Path.Combine(_WebHostEnvironment.WebRootPath, HtmlTemplateFolder);
                using (StreamReader reader = new StreamReader(Path.Combine(htmlTemplatePath, HtmlTemplateFile)))
                {
                    Body = reader.ReadToEnd();
                }

                Body = Body.Replace("{CODE}", sVeriCode);
                Body = Body.Replace("{FULLNAME}", $"{userDetails.LastName} {userDetails.FirstName}");
                Body = Body.Replace("{YEAR}", DateTime.Now.Year.ToString());
                Body = Body.Replace("{EXPIRATION}", config.OtpTokenExpiration.ToString());

                sendEmail = await _mailService.SendOtpMail(mailTo, Body);

            }

            var otpModel = new OtpRecord()
            {
                RecId = AppHelper.GetNewUniqueId(),
                UserId = userDetails.UserId,
                Otp = sVeriCode,
                Value = request.RequestSource,
                Type = request.RequestType,
                ExpirationTime = DateTime.Now.AddMinutes(Convert.ToDouble(config.OtpTokenExpiration)),
                IsUsed = false
            };

            await _repository.UserRepository.AddOtpRecordAsync(otpModel);

            await _unitOfWork.CommitAsync();

            var response = new OtpResponse()
            {
                UserId = otpModel.UserId,
                Otp = otpModel.Otp,
                RequestType = request.RequestType,
                RequestSource = request.RequestSource
            };


            return new ResponseBase<object>(response, (int)HttpStatusCode.OK, "Confirmation code sent successfully.", VarHelper.ResponseStatus.SUCCESS.ToString());
        }


        public async Task<ResponseBase<object>> VerifyOtp(VerifyOtpRequest request)
        {
            var userDetails = new UserProfile();

            //get userprofile using email or tel
            if (request.RequestType == VarHelper.OtpTypes.EMAIL.ToString()) userDetails = await _repository.UserRepository.FindByConditionAsync(user => user.EmailAddress == request.RequestSource);
            if (request.RequestType == VarHelper.OtpTypes.SMS.ToString()) userDetails = await _repository.UserRepository.FindByConditionAsync(user => user.Telephone == request.RequestSource);

            if (userDetails == null)
            {
                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, "Invalid User.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            SignInResponse response = new SignInResponse();

            var otpRecord = await _repository.UserRepository.GetNewestOtpRecord(userDetails.UserId);

            if (otpRecord == null)
            {
                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, "Invalid record", VarHelper.ResponseStatus.ERROR.ToString());
            }

            if (DateTime.Now > otpRecord.ExpirationTime)
            {
                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, "Confirmation code has expired.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            if (otpRecord.IsUsed)
            {
                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, "Confirmation code has already been used", VarHelper.ResponseStatus.ERROR.ToString());
            }

            if (otpRecord.Otp != request.OTP)
            {
                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, "Invalid confirmation code", VarHelper.ResponseStatus.ERROR.ToString());
            }

            otpRecord.IsUsed = true;

            if (request.RequestType == VarHelper.OtpTypes.SMS.ToString())
            {
                string deviceType = string.Empty;

                // Check if the headers exist and then access their values
                if (_httpContext.HttpContext.Request.Headers.ContainsKey("DeviceType"))
                {
                    deviceType = _httpContext.HttpContext.Request.Headers["DeviceType"].FirstOrDefault();
                    userDetails.DeviceType = deviceType;
                }

                userDetails.TelephoneVerified = true;
            }
            else
            {
                userDetails.EmailVerified = true;
            }
            await _repository.UserRepository.UpdateOtpRecord(otpRecord);
            await _repository.UserRepository.Update(userDetails);
            await _unitOfWork.CommitAsync();

            return new ResponseBase<object>((int)HttpStatusCode.OK, "Confirmation code successfully verified", VarHelper.ResponseStatus.SUCCESS.ToString());
        }


        public async Task<ResponseBase<AuthenticationResponse>> Refresh(RefreshTokenRequest request)
        {
            //This is a reoccurring action and should be done before hitting an action- Use a middleware/Action filter for this

            var _SysConfig = await _repository.SysConfigRepository.FirstOrDefaultAsync();
            int iLoginExpiration = _SysConfig.LoginTokenExpiration;
            if (iLoginExpiration <= 0)
            {
                return new ResponseBase<AuthenticationResponse>((int)HttpStatusCode.BadRequest, "We are currently undergoing maintenance. Check back in few minutes.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            if (request is null)
            {
                return new ResponseBase<AuthenticationResponse>((int)HttpStatusCode.BadRequest, "Invalid client request", VarHelper.ResponseStatus.ERROR.ToString());
            }


            string deviceType = string.Empty;
            string existingRefreshToken = string.Empty;

            var principal = _authenticationService.GetPrincipalFromExpiredToken(request.AccessToken);

            var userIdClaim = principal.FindFirst("UserId");
            var userId = userIdClaim?.Value;

            var user = await _repository.UserRepository.FindByConditionAsync(u => u.UserId == userId);
            if (user is null)
            {
                return new ResponseBase<AuthenticationResponse>((int)HttpStatusCode.BadRequest, "Invalid user.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            if (_authenticationService.IsAccountDisabled(user))
            {
                return new ResponseBase<AuthenticationResponse>((int)HttpStatusCode.BadRequest, $"You account is currently Disabled. Contact our customer care", VarHelper.ResponseStatus.ERROR.ToString());
            }

            // Check if the account is locked
            if (_authenticationService.IsAccountLocked(user))
            {
                return new ResponseBase<AuthenticationResponse>(419, $"Your account has been locked! To Unlock your account, click on forgot password.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            // Check if the headers exist and then access their values
            if (_httpContext.HttpContext.Request.Headers.ContainsKey("DeviceType"))
            {
                deviceType = _httpContext.HttpContext.Request.Headers["DeviceType"].FirstOrDefault();
                // Do something with the deviceType value
                existingRefreshToken = user.MobileRefreshToken;

            }
            else
            {
                existingRefreshToken = user.WebRefreshToken;
            }

            if (string.IsNullOrWhiteSpace(existingRefreshToken) || existingRefreshToken != request.RefreshToken || DateTime.Now > user.RefreshTokenExpiryTime)
            {
                return new ResponseBase<AuthenticationResponse>((int)HttpStatusCode.BadRequest, "Invalid client refresh request.", VarHelper.ResponseStatus.ERROR.ToString());
            }



            var newAccessToken = _authenticationService.GenerateAccessToken(principal.Claims, iLoginExpiration);
            var newRefreshToken = _authenticationService.GenerateRefreshToken();

            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(_SysConfig.RefreshTokenExpiration);

            if (string.IsNullOrWhiteSpace(deviceType))
            {
                user.WebRefreshToken = newRefreshToken;
            }
            else
            {
                user.MobileRefreshToken = newRefreshToken;
            }

            await _repository.UserRepository.Update(user);
            await _unitOfWork.CommitAsync();

            var response = new AuthenticationResponse()
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            };

            return new ResponseBase<AuthenticationResponse>(response, (int)HttpStatusCode.OK, "successful", VarHelper.ResponseStatus.SUCCESS.ToString());
        }


        public async Task<ResponseBase<object>> Revoke()
        {
            string userId = string.Empty;
            string authorizationHeader = _httpContext.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrWhiteSpace(authorizationHeader) && (authorizationHeader.StartsWith("Bearer ") || authorizationHeader.StartsWith("bearer ")))
            {
                ClaimsPrincipal userclaims = _httpContext.HttpContext.User;
                Claim claim = userclaims.FindFirst("UserId");
                userId = claim.Value;
            }


            var user = await _repository.UserRepository.FindByConditionAsync(u => u.UserId == userId);
            if (user == null)
            {
                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, "Invalid User request", VarHelper.ResponseStatus.ERROR.ToString());
            }


            string deviceType = string.Empty;

            // Check if the headers exist and then access their values
            if (_httpContext.HttpContext.Request.Headers.ContainsKey("DeviceType"))
            {
                deviceType = _httpContext.HttpContext.Request.Headers["DeviceType"].FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(deviceType))
            {
                user.WebRefreshToken = null;
            }
            else
            {
                user.MobileRefreshToken = null;
            }

            await _unitOfWork.CommitAsync();

            return new ResponseBase<object>((int)HttpStatusCode.OK, "Logout successful", VarHelper.ResponseStatus.SUCCESS.ToString());
        }


        public async Task<ResponseBase<object>> ChangePassword(ChangePasswordRequest request)
        {
            var _UserProfile = await _repository.UserRepository.FindByConditionAsync(x => x.UserId == request.UserId);

            // check if user exist

            if (_UserProfile == null)
            {
                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, "Invalid User.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            if (string.IsNullOrWhiteSpace(_UserProfile.Password))
            {
                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, "Please create a password before you can change", VarHelper.ResponseStatus.ERROR.ToString());
            }

            var _SysConfig = await _repository.SysConfigRepository.FirstOrDefaultAsync();

            // Check if the account is locked
            if (_authenticationService.IsAccountLocked(_UserProfile))
            {
                return new ResponseBase<object>(StatusCodes.Status419AuthenticationTimeout, $"Your account has been locked! To Unlock your account, click on forgot password.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            var passwordHash =  AppHelper.HashUsingPbkdf2(request.OldPassword, _UserProfile.PasswordSalt);
            if (_authenticationService.IsValidCredentials(_UserProfile, passwordHash))
            {
                // Reset failed attempts
                _authenticationService.ResetFailedLoginAttempts(_UserProfile);

            }
            else
            {
                // Increment failed attempts
                _authenticationService.IncrementFailedLoginAttempts(_UserProfile);
                if (_UserProfile != null && _UserProfile.FailedLoginAttempt >= _SysConfig.LoginAttemptMax)
                {
                    // Lock the account for 1 hour
                    _authenticationService.LockAccount(_UserProfile, (int)_SysConfig.LoginAttemptLockInHours);
                }

                if (_UserProfile.FailedLoginAttempt == _SysConfig.NotifyForLoginAttempt)
                {
                    await _mailService.SendSecurityMail(_UserProfile.EmailAddress, VarHelper.SecurityAttemptTypes.PASSWORDCHANGE.ToString());
                }

                await _unitOfWork.CommitAsync();

                var ans = _SysConfig.LoginAttemptMax - _UserProfile.FailedLoginAttempt;

                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, ans <= 0 ? $"Wrong old password. Your account has been locked! To Unlock your account, click on forgot password." : $"Invalid Password, you have {ans} attempt(s) remaining!", VarHelper.ResponseStatus.ERROR.ToString());

            }

            var newpasswordHash =  AppHelper.HashUsingPbkdf2(request.NewPassword, _UserProfile.PasswordSalt);
            if (_UserProfile.Password == newpasswordHash)
            {
                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, "New Password can't be same as Current Password.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(AppHelper.GetUnique8ByteKey());
            string sSalt = Convert.ToBase64String(plainTextBytes);
            string sHashedPassword =  AppHelper.HashUsingPbkdf2(request.NewPassword, sSalt);

            _UserProfile.Password = sHashedPassword;
            _UserProfile.PasswordSalt = sSalt;

            await _repository.UserRepository.Update(_UserProfile);

            await _unitOfWork.CommitAsync();

            return new ResponseBase<object>((int)HttpStatusCode.OK, "Your Password was successfully changed.", VarHelper.ResponseStatus.SUCCESS.ToString());
        }


        public async Task<ResponseBase<object>> ForgotPassword(ForgotPasswordRequest request)
        {
            if (request.NewPassword.Length > 8 || request.NewPassword.Length < 8)
            {
                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, "Your new password cannot be greater or less than 8 alphanumeric", VarHelper.ResponseStatus.ERROR.ToString());
            }
            var userProfile = await _repository.UserRepository.FindByConditionAsync(x => x.UserId == request.UserId);

            // check if user exist

            if (userProfile == null)
            {
                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, "Invalid User.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            if (string.IsNullOrWhiteSpace(userProfile.Password))
            {
                return new ResponseBase<object>((int)HttpStatusCode.BadRequest, "Please create a password before you can change", VarHelper.ResponseStatus.ERROR.ToString());
            }


            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(AppHelper.GetUnique8ByteKey());
            string sSalt = Convert.ToBase64String(plainTextBytes);
            string sHashedPassword = AppHelper.HashUsingPbkdf2(request.NewPassword, sSalt);

            userProfile.Password = sHashedPassword;
            userProfile.PasswordSalt = sSalt;

            await _repository.UserRepository.Update(userProfile);

            await _unitOfWork.CommitAsync();

            return new ResponseBase<object>((int)HttpStatusCode.OK, "Your Password was successfully changed.", VarHelper.ResponseStatus.SUCCESS.ToString());
        }


        public async Task<ResponseBase<UserDetailsResponse>> GetUserProfileByUserId(string UserId)
        {
            UserDetailsResponse _User = new UserDetailsResponse();

            var userProfile = await _repository.UserRepository.FindByConditionAsync(x => x.UserId == UserId);
            var userProfileReg = await _repository.UserRegRepository.FindByConditionAsync(x => x.UserId == UserId);

            if (userProfileReg != null)
            {
                Mapper<UserRegistration, UserDetailsResponse>.map(userProfileReg, _User);

                return new ResponseBase<UserDetailsResponse>(_User, (int)HttpStatusCode.OK, "Registration still in process. See Details", VarHelper.ResponseStatus.SUCCESS.ToString());
            }

            if (userProfile != null)
            {
                Mapper<UserProfile, UserDetailsResponse>.map(userProfile, _User);
                return new ResponseBase<UserDetailsResponse>(_User, (int)HttpStatusCode.OK, "Retrieval successful.", VarHelper.ResponseStatus.SUCCESS.ToString());
            }

            return new ResponseBase<UserDetailsResponse>((int)HttpStatusCode.NotFound, "Invalid UserId.", VarHelper.ResponseStatus.ERROR.ToString());

        }

        private async Task<bool> UserProfileExistsAsync(string id)
        {
            return await _repository.UserRepository.GetAnyAsync(e => e.UserId == id);
        }

    }
}
