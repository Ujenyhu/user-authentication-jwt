using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Responses.User
{
    public class OtpResponse
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Otp { get; set; }
        [Required]
        public string RequestType { get; set; }
        [Required]
        public string RequestSource { get; set; }
    }
}
