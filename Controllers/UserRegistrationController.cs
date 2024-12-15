using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using userauthjwt.BusinessLogic.Interfaces;
using userauthjwt.Requests.User;
using userauthjwt.Responses.User;
using userauthjwt.Responses;
using System.ComponentModel.DataAnnotations;
using System.Net;
using userauthjwt.Helpers;

namespace userauthjwt.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("AllowAllHeaders")]
    [Authorize]
    public class UserRegistrationController : ControllerBase
    {
        private readonly IServicesWrapper _services;


        public UserRegistrationController(IServicesWrapper services)
        {
            _services = services;
        }

        /// <summary>
        /// Sign up a new public user
        /// </summary>
        /// <remarks>
        ///  All fields are required  at the point of registration.
        ///  This endpoint is to be used for individual/public users. It does not require Bearer authentication
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>The userId and usertype of the newly created user.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseBase<SignUpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            var result = await _services.UserRegistrationService.SignUp(request);
            return StatusCode(result.StatusCode, result);
        }


        /// <summary>
        /// sends user verification code to Email or Telephone
        /// </summary>
        /// <remarks>
        ///  This endpoint is to be used during the registration process to send one time password to the provided email address or phone number at the point of signup.
        ///  The 'RequestType' differentiates the action as to when you want to send the sms to verify a phone number or email to verify an email address. 
        ///  RequestType can either be 'EMAIL' or 'SMS' and the 'RequestSource' will be the email address or phone number of the user. All fields are required.
        ///  
        /// e.g Verify telephone at the point of reg:
        /// 
        ///     POST api/SendVerificiationOtp
        ///     
        ///     {        
        ///       "RequestType": "SMS",
        ///       "RequestSource": "081xxxx"
        ///       
        ///     }
        ///     
        ///   e.g Verify email at the point of reg:
        /// 
        ///     POST api/SendVerificiationOtp
        ///     
        ///     {        
        ///       "RequestType": "EMAIL",
        ///       "RequestSource": "user@example.com"
        ///       
        ///     } 
        ///     
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseBase<OtpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendVerificationOtp([FromBody] OtpRequest request)
        {
            var result = await _services.UserRegistrationService.SendOtpReg(request);
            return StatusCode(result.StatusCode, result);
        }



        /// <summary>
        /// Verify OTP sent to email/phone during registration process
        /// </summary>
        /// <remarks>
        ///  This endpoint is to be used to verify/compare if the otp sent and the one provided by the user is valid.
        ///  The 'InputType' differentiates the action. InputType can either be 'EMAIL' or 'TELEPHONE' and the 'InputSource' will be the email address or phone number of the user.
        ///  'OTP' will hold the code that was sent to the user's device. All fields are required.
        ///   
        /// e.g Verify telephone at the point of reg:
        /// 
        ///     POST api/ConfirmVerificationOtp
        ///     
        ///     {        
        ///       "InputType": "TELEPHONE",
        ///       "InputSource": "081xxxxxx",
        ///       "Otp": "xxxxxx"
        ///       
        ///     }
        ///     
        ///   e.g Verify email at the point of reg:
        /// 
        ///     POST api/ConfirmVerificationOtp
        ///     
        ///     {        
        ///       "InputType": "EMAIL",
        ///       "InputSource": "user@example.com",
        ///       "Otp": "xxxxxx"
        ///       
        ///     } 
        ///     NOTE: This endpoint should only be used or called at the point of registration. It does not require Bearer authentication
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmVerificationOtp([FromBody] VerifyOtpRequest request)
        {
            var result = await _services.UserRegistrationService.VerifyOtpReg(request);
            return StatusCode(result.StatusCode, result);
        }


        /// <summary>
        /// Retrieves complete user registration profile
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseBase<UserDetailsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserByUserId([Required] string UserId)
        {
            // Check authorization claims
            if (!_services.AuthenticationService.IsValidUser(UserId)) return Unauthorized(new ResponseBase<object>((int)HttpStatusCode.Unauthorized, "Invalid user credentials. Please, login and try again.", VarHelper.ResponseStatus.ERROR.ToString()));


            var result = await _services.UserService.GetUserProfileByUserId(UserId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
