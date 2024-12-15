using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace userauthjwt.Requests.User
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        [RegularExpression("^(?=.*\\d)(?=.*[^a-zA-Z0-9]).{6}$", ErrorMessage = "Passwords must be 6 characters and contain at least 1 of the following: Number (0-9) and special character (e.g. !@#$%^&*)")]
        public string NewPassword { get; set; }
    }
}
