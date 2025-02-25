using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using userauthjwt.BusinessLogic.Interfaces;
using userauthjwt.Responses.Lookup;
using userauthjwt.Responses;

namespace userauthjwt.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("AllowAllHeaders")]

    public class LookupController : ControllerBase
    {
        private readonly IServicesWrapper _services;

        /// <summary>
        /// This is the lookup section. It helps you the correct pattern or requirement for default metadata 
        /// </summary>
        /// <param name="services"></param>
        public LookupController(IServicesWrapper services)
        {
            _services = services;
        }


        /// <summary>
        ///  Endpoint to get the different user status
        /// </summary>
        /// <remarks> 
        ///  ONBOARDING - Is when the user is still on the registration process or just registered to the platform.
        ///  
        ///  
        ///  ENABLED -  Is when the user is now an authorised/approved user and has completed all the registration requirement
        ///  
        ///  DISABLED -  Explains when a vendor's account is under investigation and cannot perform certain actions.
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseBase<List<LookupResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UserStatus()
        {
            var result = await _services.LookupService.GetUserStatus();
            return StatusCode(result.StatusCode, result);
        }


        /// <summary>
        ///  Endpoint to get the different user types
        /// </summary>
        /// <remarks> 
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseBase<List<LookupResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UserTypes()
        {
            var result = await _services.LookupService.GetUserTypes();
            return StatusCode(result.StatusCode, result);
        }


        /// <summary>
        /// Endpoint to get supported countries
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseBase<List<CountryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCountries()
        {
            var result = await _services.LookupService.GetCountries();
            return StatusCode(result.StatusCode, result);
        }


        /// <summary>
        ///  Endpoint to get accepted otp request types
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseBase<List<LookupResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseBase<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> OtpRequestTypes()
        {
            var result = await _services.LookupService.OtpRequestTypes();
            return StatusCode(result.StatusCode, result);
        }

    }
}
