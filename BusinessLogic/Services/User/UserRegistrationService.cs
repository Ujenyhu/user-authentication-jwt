using Microsoft.EntityFrameworkCore;
using System.Net;
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
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly IRepositoryWrapper _repository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMailService _mailService;
        private readonly ISmsService _smsService;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ICacheService _cacheService;
        public UserRegistrationService(IRepositoryWrapper repository,
            IAuthenticationService authenticationService,
            IUnitOfWork unitOfWork,
            IMailService mailService,
            ISmsService smsService,
            IConfiguration config,
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpcontext,
            ICacheService cacheService)
        {
            _repository = repository;
            _authenticationService = authenticationService;
            _unitOfWork = unitOfWork;
            _mailService = mailService;
            _smsService = smsService;
            _config = config;
            _httpContext = httpcontext;
            _WebHostEnvironment = webHostEnvironment;
            _cacheService = cacheService;
        }

        public async Task<ResponseBase<SignUpResponse>> SignUp(SignUpRequest _Request)
        {
            //This is a reoccurring action and should be done before hitting an action- Use a middleware/Action filter for this

            var config = await _repository.SysConfigRepository.FirstOrDefaultAsync();
            int iLoginExpiration = config.LoginTokenExpiration;
            if (iLoginExpiration <= 0)
            {
                return new ResponseBase<SignUpResponse>((int)HttpStatusCode.BadRequest, "We are currently undergoing maintenance. Check back in few minutes.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            //check if telephone exists
            var _UserTelephone = await _repository.UserRepository.FindByConditionAsync(user => user.Telephone == _Request.Telephone);
            var registeredTelephone = await _repository.UserRegRepository.FindByConditionAsync(user => user.Telephone == _Request.CountryId + _Request.Telephone);
            if (_UserTelephone != null || registeredTelephone != null)
            {
                return new ResponseBase<SignUpResponse>((int)HttpStatusCode.BadRequest, "This telephone number has been registered already, login to continue", VarHelper.ResponseStatus.ERROR.ToString());
            }

            //check if email exists
            var _UserEmail = await _repository.UserRepository.FindByConditionAsync(user => user.EmailAddress == _Request.Email);
            var registeredEmail = await _repository.UserRegRepository.FindByConditionAsync(user => user.EmailAddress == _Request.Email);

            if (_UserEmail != null || registeredEmail != null)
            {
                return new ResponseBase<SignUpResponse>((int)HttpStatusCode.BadRequest, "This email has been registered already.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            //check if username exists
            var usernameExist = await _repository.UserRepository.GetAnyAsync(user => user.Username == _Request.Username);
            var registeredUsername = await _repository.UserRegRepository.GetAnyAsync(user => user.Username == _Request.Username);

            if (usernameExist || registeredUsername)
            {
                return new ResponseBase<SignUpResponse>((int)HttpStatusCode.BadRequest, "This username has been registered already.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            // var _userId = AppHelper.GetNewUniqueId();
            var _userId = AppHelper.GetGuid();

            UserRegistration newUser = new UserRegistration();


            //create user record
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(AppHelper.GetUnique8ByteKey());
            string sSalt = Convert.ToBase64String(plainTextBytes);
            string sHashedPassword = AppHelper.HashUsingPbkdf2(_Request.Password, sSalt);

            newUser.UserId = _userId;
            newUser.Username = _Request.Username;
            newUser.Telephone = _Request.Telephone;
            newUser.EmailAddress = _Request.Email;
            newUser.PasswordSalt = sSalt;
            newUser.Password = sHashedPassword;
            newUser.FirstName = _Request.FirstName;
            newUser.LastName = _Request.LastName;
            newUser.DateRegistered = DateTime.UtcNow;


            // Check if the headers exist and then access their values
            if (_httpContext.HttpContext.Request.Headers.ContainsKey("DeviceType"))
            {
                var deviceType = _httpContext.HttpContext.Request.Headers["DeviceType"].FirstOrDefault();
                newUser.DeviceType = deviceType;
            }

            if (_httpContext.HttpContext.Request.Headers.ContainsKey("DeviceToken"))
            {
                var deviceTokenHeader = _httpContext.HttpContext.Request.Headers["DeviceToken"].FirstOrDefault();
                newUser.DeviceToken = deviceTokenHeader;
            }

            if (_httpContext.HttpContext.Request.Headers.ContainsKey("DeviceId"))
            {
                var deviceId = _httpContext.HttpContext.Request.Headers["DeviceId"].FirstOrDefault();
                newUser.DeviceId = deviceId;
            }

            try
            {

                await _repository.UserRegRepository.AddAsync(newUser);

                await _unitOfWork.CommitAsync();

                SignUpResponse response = new SignUpResponse();

                Mapper<UserRegistration, SignUpResponse>.map(newUser, response);


                return new ResponseBase<SignUpResponse>(response, (int)HttpStatusCode.OK, "Your registration was successful.", VarHelper.ResponseStatus.SUCCESS.ToString());
            }
            catch (DbUpdateException)
            {
                if (await UserRegProfileExistsAsync(newUser.UserId))
                {
                    return new ResponseBase<SignUpResponse>((int)HttpStatusCode.BadRequest, "User profile generation conflict.", VarHelper.ResponseStatus.ERROR.ToString());
                }
                else
                {
                    throw;
                }
            }
        }




        public async Task<ResponseBase<DoesUsernameExistResponse>> DoesUsernameExist(string Username)
        {
            //This is a reoccurring action and should be done before hitting an action- Use a middleware/Action filter for this

            var config = await _repository.SysConfigRepository.FirstOrDefaultAsync();
            int iLoginExpiration = config.LoginTokenExpiration;
            if (iLoginExpiration <= 0)
            {
                return new ResponseBase<DoesUsernameExistResponse>((int)HttpStatusCode.BadRequest, "We are currently undergoing maintenance. Check back in few minutes.", VarHelper.ResponseStatus.ERROR.ToString());
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                return new ResponseBase<DoesUsernameExistResponse>((int)HttpStatusCode.BadRequest, "Username cannot be empty", VarHelper.ResponseStatus.ERROR.ToString());
            }

            var response = new DoesUsernameExistResponse();
            //check if username exists
            //Check cache first
            string key = $"username_{Username}";
            string? cachedVal = await _cacheService.GetAsync<string>(key);

            if(cachedVal != null)
            {
                response.UsernameExist = true;
                return new ResponseBase<DoesUsernameExistResponse>(response, (int)HttpStatusCode.Conflict, "Username already exist in the system", VarHelper.ResponseStatus.ERROR.ToString());
            }

            //Retrieve from DB
            bool existsInRegDb = await _repository.UserRegRepository.GetAnyAsync(x => x.Username == Username);
            bool existsInDb = await _repository.UserRepository.GetAnyAsync(x => x.Username == Username);

            if(await _repository.UserRegRepository.GetAnyAsync(x => x.Username == Username) || await _repository.UserRepository.GetAnyAsync(x => x.Username == Username))
            {
                _cacheService.SetAsync(key, true, TimeSpan.FromMinutes(10));
                response.UsernameExist = true;
                return new ResponseBase<DoesUsernameExistResponse>(response, (int)HttpStatusCode.Conflict, "Username already exist in the system", VarHelper.ResponseStatus.ERROR.ToString());

            }
            response.UsernameExist = false;
            return new ResponseBase<DoesUsernameExistResponse>(response, (int)HttpStatusCode.OK, "Proceed with Username", VarHelper.ResponseStatus.SUCCESS.ToString());
        }


        private async Task<bool> UserRegProfileExistsAsync(string id)
        {
            return await _repository.UserRegRepository.GetAnyAsync(e => e.UserId == id);
        }


        //This is for the registration process. This endpoint will send code to verify newly registered users
        public async Task<ResponseBase<object>> SendOtpReg(OtpRequest request)
        {

            var userDetails = new UserRegistration();

            //get userprofile using email or tel
            if (request.RequestType == VarHelper.OtpTypes.EMAIL.ToString()) userDetails = await _repository.UserRegRepository.FindByConditionAsync(user => user.EmailAddress == request.RequestSource);
            if (request.RequestType == VarHelper.OtpTypes.SMS.ToString()) userDetails = await _repository.UserRegRepository.FindByConditionAsync(user => user.Telephone == request.RequestSource);

            if (userDetails == null)
            {
                return new ResponseBase<object>((int)HttpStatusCode.NotFound, "Invalid user. Only use this endpoint for registration!", VarHelper.ResponseStatus.ERROR.ToString());
            }

            //send verification SMS
            string sVeriCode = AppHelper.GenerateRandomNumber();

            var config = await _repository.SysConfigRepository.FirstOrDefaultAsync();

            bool sendSms = false; bool sendEmail = false;
            if (request.RequestType == VarHelper.OtpTypes.SMS.ToString())
            {
                sendSms = await _smsService.SendTwilioSms(userDetails.Telephone, $"Your confirmation code is {sVeriCode}. Valid for {config.OtpTokenExpiration} minute, One-time use only.");
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
                //Body = Body.Replace("{FULLNAME}", $"{userDetails.LastName} {userDetails.FirstName}");
                Body = Body.Replace("{YEAR}", DateTime.Now.Year.ToString());
                Body = Body.Replace("{EXPIRATION}", config.OtpTokenExpiration.ToString());

                sendEmail = await _mailService.SendOtpMail(mailTo, Body);

            }

            if (sendEmail || sendSms)
            {
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
                    UserId = userDetails.UserId,
                    Otp = sVeriCode,
                    RequestType = request.RequestType,
                    RequestSource = request.RequestSource
                };

                return new ResponseBase<object>(response, (int)HttpStatusCode.OK, "Confirmation code sent successfully.", VarHelper.ResponseStatus.SUCCESS.ToString());
            }
            return new ResponseBase<object>((int)HttpStatusCode.InternalServerError, "Confirmation code not sent. Please try again later", VarHelper.ResponseStatus.ERROR.ToString());

        }


        public async Task<ResponseBase<object>> VerifyOtpReg(VerifyOtpRequest request)
        {
            var userDetails = new UserRegistration();

            //get userprofile using email or tel
            if (request.RequestType == VarHelper.OtpTypes.EMAIL.ToString()) userDetails = await _repository.UserRegRepository.FindByConditionAsync(user => user.EmailAddress == request.RequestSource);
            if (request.RequestType == VarHelper.OtpTypes.SMS.ToString()) userDetails = await _repository.UserRegRepository.FindByConditionAsync(user => user.Telephone == request.RequestSource);

            if (userDetails == null)
            {
                return new ResponseBase<object>((int)HttpStatusCode.NotFound, "Invalid User. Only use this endpoint during registration!", VarHelper.ResponseStatus.ERROR.ToString());
            }

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
            await _repository.UserRegRepository.Update(userDetails);
            await _unitOfWork.CommitAsync();

            return new ResponseBase<object>((int)HttpStatusCode.OK, "Confirmation code successfully verified", VarHelper.ResponseStatus.SUCCESS.ToString());
        }
    }
}
