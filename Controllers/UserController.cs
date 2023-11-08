using userauthjwt.Helpers;
using userauthjwt.Models;
using userauthjwt.Requests;
using userauthjwt.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Azure;
using Microsoft.AspNetCore.Authentication.OAuth;
using Azure.Core;
using userauthjwt.Interfaces;

namespace userauthjwt.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("AllowAllHeaders")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly  IUserRepository<UserProfile> _userRepository;
        private readonly IHttpContextAccessor _httpContext;
        private AuthenticationHelper _AuthHelper; 

        public UserController( AppDbContext context, IHttpContextAccessor httpContext, IUserRepository<UserProfile> userRepository)
        {
            _userRepository = userRepository;
            _context = context;
            this._httpContext = httpContext;
            _AuthHelper = new AuthenticationHelper(_httpContext);
        }


        /// <summary>
        /// Register a new user
        /// </summary>
        /// <remarks>
        /// All fields in the request are required.
        /// 
        /// You cannot register thesame email twice. 
        /// The Password is strictly numeric and must be 4 digits
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseBase<SignUpResponse>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseBase<object>),StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignUp(SignUpRequest request)
        {

            //check for null values
            if (request.IsNullValue(request))
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "Value(s) cannot be null", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //Check password validity
            var pinRegex = @"^\d{4}$";
            if(!Regex.IsMatch(request.Password, pinRegex))
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "Password is strictly numeric and must be 4 digits", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //check if email exists
            var userEmail = await _userRepository.FindByConditionAsync(user => user.Email == request.EmailAddress);
            if (userEmail != null)
            {
                return Conflict(new ResponseBase<object>(StatusCodes.Status409Conflict, "This email has been previously registered.", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //create user record if all requirements are met
            var signUpUser = await _userRepository.SignUp(request);
            if(signUpUser.Data == null)
            {
                return BadRequest(signUpUser);
            }

            return Ok(signUpUser);
        }


        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseBase<SignInResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignIn(SignInRequest request)
        {
            if (request.IsNullValue(request))
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "Value(s) cannot be null", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //Check password validity
            var pinRegex = @"^\d{4}$";
            if (!Regex.IsMatch(request.Password, pinRegex))
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "Password is strictly numeric and must be 4 digits", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //validate user credentials
            var user = await _userRepository.FindByConditionAsync(user => user.Email == request.EmailAddress);
            if (user == null)
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "Invalid Email", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //Verify Password
            var passwordHash = AppHelper.HashPassword(request.Password, user.PasswordSalt);
            if (user.Password != passwordHash)
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "Invalid Password", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //Go ahead to log in user if credentials are valid
            var signInUser = await _userRepository.SignIn(user);
            if(signInUser.Data == null)
            {
                return BadRequest(signInUser);
            }

            return Ok(signInUser);
        }


        /// <summary>
        /// Refresh your page
        /// </summary>
        /// <remarks>
        /// All fields in the request are required.
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseBase<AuthenticationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Refresh(TokenRequest request)
        {

            if (request.IsNullValue(request))
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "Request value cannot be null", VarHelper.ResponseStatus.ERROR.ToString()));
            }
            var refresh = await _userRepository.Refresh(request);
            if (refresh.Data == null)
            {
                return BadRequest(refresh);
            }
            return Ok(refresh);
        }



        [HttpPost]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Revoke()
        {
            //get the userId from the claims
            var userId = _httpContext.HttpContext.User.Identity.Name;

            //Confirm userId validity
            var user = await _userRepository.FindByConditionAsync(u => u.UserId == userId);
            if (user == null)
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "Invalid user request", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            user.RefreshToken = null;

            await _userRepository.CompleteAsync();
            return Ok(new ResponseBase<object>(StatusCodes.Status200OK, "Token revoked.", VarHelper.ResponseStatus.SUCCESS.ToString()));

        }


        ///<summary>
        /// Update or make changes to User Profile
        ///</summary>
        ///<remarks>
        /// Not all parameters are required. Chnage only the necessary field
        ///</remarks>
        /// <param name="request"></param>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> UpdateUser(UpdateUserRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "UserId cannot be null.", VarHelper.ResponseStatus.ERROR.ToString()));
            }
            //check authorization claims
            if (!_AuthHelper.IsAuthenticated(request.UserId))
            {
                return Unauthorized(new ResponseBase<object>(StatusCodes.Status401Unauthorized, "Invalid user credentials.  Please, login and try again.", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //update user if authentication is valid
            var updateUser = await _userRepository.UpdateUser(request);
            if (updateUser.StatusCode == StatusCodes.Status400BadRequest) { return BadRequest(updateUser); }
            return Ok(updateUser);
        }


        ///<summary>
        /// Change Password
        ///</summary>
        /// <param name="request"></param>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            if (request.IsNullValue(request))
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "Request value cannot be null", VarHelper.ResponseStatus.ERROR.ToString()));
            }
            //Check password validity
            var pinRegex = @"^\d{4}$";
            if (!Regex.IsMatch(request.OldPassword, pinRegex) || !Regex.IsMatch(request.NewPassword, pinRegex))
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "Password is strictly numeric and must be 4 digits", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //check authorization claims
            if (!_AuthHelper.IsAuthenticated(request.UserId))
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "Invalid user credentials.  Please, login and try again.", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //change password
            var changePass = await _userRepository.ChangePassword(request);

            if(changePass.StatusCode == StatusCodes.Status404NotFound) { return NotFound(changePass); }
            if(changePass.StatusCode == StatusCodes.Status400BadRequest) { return BadRequest(changePass); }

            return Ok(changePass);
        }


        /// <summary>
        /// Retrieves complete user profile information of the current logged in user by userId
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCurrentUserById(string UserId)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "UserId cannot be null.", VarHelper.ResponseStatus.ERROR.ToString()));
            }
            //check authorization claims
            if (!_AuthHelper.IsAuthenticated(UserId))
            {
                return Unauthorized(new ResponseBase<object>(StatusCodes.Status401Unauthorized, "Invalid user credentials.  Please, login and try again.", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //get user details from the database
            var _UserProfile = await _userRepository.GetByIdAsync(UserId);
            if (_UserProfile == null)
            {
                return NotFound(new ResponseBase<object>(StatusCodes.Status404NotFound, "Invalid UserId. Record not found", VarHelper.ResponseStatus.ERROR.ToString()));
            }


            //set user info to response object
            var response = new UserProfileResponse()
            {
                UserId = _UserProfile.Data.UserId,
                Lastname = _UserProfile.Data.Lastname,
                Firstname = _UserProfile.Data.Firstname,
                Email = _UserProfile.Data.Email,
            };
            return Ok(new ResponseBase<object>(response, StatusCodes.Status200OK, "User detail retrieved successfully.", VarHelper.ResponseStatus.SUCCESS.ToString()));
        }


        /// <summary>
        /// Retrieves complete user profile information of the current logged in user by email address
        /// </summary>
        /// <param name="EmailAddress"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCurrentUserByEmail(string EmailAddress)
        {
            if (string.IsNullOrEmpty(EmailAddress))
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "Email address cannot be null.", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //get the userId from the claims
            var userId = _httpContext.HttpContext.User.Identity.Name;

            //check authorization claims
            if (!_AuthHelper.IsAuthenticated(userId))
            {
                return Unauthorized(new ResponseBase<object>(StatusCodes.Status401Unauthorized, "Invalid user credentials.  Please, login and try again.", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //get user details from the database
            var _UserProfile = await _userRepository.FindByConditionAsync(x => x.Email == EmailAddress);
            if (_UserProfile == null)
            {
                return NotFound(new ResponseBase<object>(StatusCodes.Status404NotFound, "Invalid Email Address. Record not found.", VarHelper.ResponseStatus.ERROR.ToString()));
            }


            //set user info to response object
            var response = new UserProfileResponse()
            {
                UserId = _UserProfile.UserId,
                Lastname = _UserProfile.Lastname,
                Firstname = _UserProfile.Firstname,
                Email = _UserProfile.Email,
            };
            return Ok(new ResponseBase<object>(response, StatusCodes.Status200OK, "User detail retrieved successfully.", VarHelper.ResponseStatus.SUCCESS.ToString()));
        }


        /// <summary>
        /// Retrieves complete user profile information of any user by userId
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserByUserId(string UserId)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "UserId cannot be null.", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //get user details from the database
            var _UserProfile = await _userRepository.GetByIdAsync(UserId);
            if (_UserProfile == null)
            {
                return NotFound(new ResponseBase<object>(StatusCodes.Status404NotFound, "Invalid UserId. Record not found", VarHelper.ResponseStatus.ERROR.ToString()));
            }


            //set user info to response object
            var response = new UserProfileResponse()
            {
                UserId = _UserProfile.Data.UserId,
                Lastname = _UserProfile.Data.Lastname,
                Firstname = _UserProfile.Data.Firstname,
                Email = _UserProfile.Data.Email,
            };
            return Ok(new ResponseBase<object>(response, StatusCodes.Status200OK, "User detail retrieved successfully.", VarHelper.ResponseStatus.SUCCESS.ToString()));
        }


        /// <summary>
        /// Retrieves complete user profile information of any user by email address
        /// </summary>
        /// <param name="EmailAddress"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUserByEmail(string EmailAddress)
        {
            if (string.IsNullOrEmpty(EmailAddress))
            {
                return BadRequest(new ResponseBase<object>(StatusCodes.Status400BadRequest, "Email address cannot be null.", VarHelper.ResponseStatus.ERROR.ToString()));
            }

            //get user details from the database
            var _UserProfile = await _userRepository.FindByConditionAsync(x => x.Email == EmailAddress);
            if (_UserProfile == null)
            {
                return NotFound(new ResponseBase<object>(StatusCodes.Status404NotFound, "Invalid Email Address. Record not found.", VarHelper.ResponseStatus.ERROR.ToString()));
            }


            //set user info to response object
            var response = new UserProfileResponse()
            {
                UserId = _UserProfile.UserId,
                Lastname = _UserProfile.Lastname,
                Firstname = _UserProfile.Firstname,
                Email = _UserProfile.Email,
            };
            return Ok(new ResponseBase<object>(response, StatusCodes.Status200OK, "User detail retrieved successfully.", VarHelper.ResponseStatus.SUCCESS.ToString()));
        }
    }
}
