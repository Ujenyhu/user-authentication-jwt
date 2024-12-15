using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using userauthjwt.Helpers;

namespace userauthjwt.Requests.User
{
    public class VerifyOtpRequest
    {
        [Required]
        [EnumDataType(typeof(VarHelper.OtpTypes), ErrorMessage = "Check the lookup section for accepted otp request types.")]
        public string RequestType { get; set; }
        [Required]
        public string RequestSource { get; set; }
        [Required]
        [DefaultValue("string")]
        [StringLength(6)]
        [RegularExpression(@"^(?!-)\d+$", ErrorMessage = "Invalid positive integer.")]
        public string OTP { get; set; }
    }
}
