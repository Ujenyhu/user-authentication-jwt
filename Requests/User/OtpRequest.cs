using System.ComponentModel.DataAnnotations;
using userauthjwt.Helpers;

namespace userauthjwt.Requests.User
{
    public class OtpRequest
    {
        [Required]
        [EnumDataType(typeof(VarHelper.OtpTypes), ErrorMessage = "Check the lookup section for accepted otp request types.")]
        public string RequestType { get; set; }
        [Required]
        public string RequestSource { get; set; }
    }
}
