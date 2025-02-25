using userauthjwt.Helpers;
using userauthjwt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Azure;
using Microsoft.AspNetCore.Authentication.OAuth;
using Azure.Core;
using userauthjwt.DataAccess.Interfaces;
using userauthjwt.Requests.User;
using userauthjwt.Responses.User;
using userauthjwt.Models.User;
using System.ComponentModel.DataAnnotations;
using userauthjwt.BusinessLogic.Interfaces;
using userauthjwt.Responses;
using System.Net;

namespace userauthjwt.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("AllowAllHeaders")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IServicesWrapper _services;


        public UserController(IServicesWrapper services)
        {
            _services = services;
        }



        /// <summary>
        /// Signs in user into the app.
        /// </summary>
        /// <remarks>
        /// It returns a token. Use the token for Bearer authentication to access authorized endpoints
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseBase<SignInResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            var result = await _services.UserService.SignIn(request);
            return StatusCode(result.StatusCode, result);
        }



        ///<summary>
        /// This endpoint is used to refresh a users token.
        ///</summary>
        /// <param name="tokenApiModel"></param>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResponseBase<AuthenticationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest tokenApiModel)
        {
            var result = await _services.UserService.Refresh(tokenApiModel);
            return StatusCode(result.StatusCode, result);
        }


        ///<summary>
        /// This endpoint is used to revoke a token and logout currently logged in user
        ///</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Revoke()
        {
            var result = await _services.UserService.Revoke();
            return StatusCode(result.StatusCode, result);
        }



        ///<summary>
        /// Change user password
        ///</summary>
        /// <param name="request"></param>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            // Check authorization claims
            if (!_services.AuthenticationService.IsValidUser(request.UserId)) return Unauthorized(new ResponseBase<object>((int)HttpStatusCode.Unauthorized, "Invalid user credentials. Please, login and try again.", VarHelper.ResponseStatus.ERROR.ToString()));

            var result = await _services.UserService.ChangePassword(request);

            return StatusCode(result.StatusCode, result);
        }


        ///<summary>
        /// Request to change password if forgotten
        ///</summary>
        /// <param name="request"></param>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // Check authorization claims
            if (!_services.AuthenticationService.IsValidUser(request.UserId)) return Unauthorized(new ResponseBase<object>((int)HttpStatusCode.Unauthorized, "Invalid user credentials. Please, login and try again.", VarHelper.ResponseStatus.ERROR.ToString()));

            var result = await _services.UserService.ForgotPassword(request);
            return StatusCode(result.StatusCode, result);
        }



        /// <summary>
        /// sends otp to Email or Telephone
        /// </summary>
        /// <remarks>
        ///  It an be used to send codes for authentication purposes to the provided email address or phone number of users for password/transaction pin change, 2FA, etc.
        ///  The 'RequestType' differentiates the action as to when you want to send an sms to a phone number or email to an email address. 
        ///  RequestType can either be 'EMAIL' or 'SMS' and the 'RequestSource' will be the email address or phone number of the user. 
        ///  All fields are required.
        ///  
        /// e.g For sms:
        /// 
        ///     POST api/SendOtp
        ///     
        ///     {        
        ///       "RequestType": "SMS",
        ///       "RequestSource": "081xxxxxx"
        ///       
        ///     }
        ///     
        ///   e.g For email:
        /// 
        ///     POST api/SendOtp
        ///     
        ///     {        
        ///       "RequestType": "EMAIL",
        ///       "InpuRequestSourcetSource": "user@example.com"
        ///       
        ///     } 
        ///     
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseBase<OtpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendOtp([FromBody] OtpRequest request)
        {
            var result = await _services.UserService.SendOtp(request);
            return StatusCode(result.StatusCode, result);
        }



        /// <summary>
        /// Verify OTP sent to email/phone
        /// </summary>
        /// <remarks>
        ///  This endpoint is to be used to verify the validity of an otp.
        ///  The 'RequestType' differentiates the action as to when you want to send an sms to a phone number or email to an email address. 
        ///  RequestType can either be 'EMAIL' or 'SMS' and the 'RequestSource' will be the email address or phone number of the user. 
        ///  All fields are required.
        ///  
        /// e.g For sms:
        /// 
        ///     POST api/VerifyOtp
        ///     
        ///     {        
        ///       "RequestType": "SMS",
        ///       "RequestSource": "081xxxxxx"
        ///       
        ///     }
        ///     
        ///   e.g For email:
        /// 
        ///     POST api/VerifyOtp
        ///     
        ///     {        
        ///       "RequestType": "EMAIL",
        ///       "RequestSource": "user@example.com"
        ///       
        ///     } 
        ///     
        ///     NOTE: This endpoint should only be used or called for users who have completed the registration process.
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseBase<OtpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var result = await _services.UserService.VerifyOtp(request);
            return StatusCode(result.StatusCode, result);
        }


        /// <summary>
        /// Upload profile image
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseBase<UserDetailsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadProfileImage([Required] UploadImageRequest request)
        {
            // Check authorization claims
            if (!_services.AuthenticationService.IsValidUser(request.UserId)) 
                return Unauthorized(new ResponseBase<object>((int)HttpStatusCode.Unauthorized, "Invalid user credentials. Please, login and try again.", VarHelper.ResponseStatus.ERROR.ToString()));


            var result = await _services.UserService.UploadUserImage(request);
            return StatusCode(result.StatusCode, result);
        }


        /// <summary>
        /// Retrieves complete user profile by UserId
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
