using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Requests.User
{
    public class ChangePasswordRequest
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string OldPassword { get; set; }
        [Required]
        [DefaultValue("string")]
        [RegularExpression("^(?=.*\\d)(?=.*[^a-zA-Z0-9]).{6}$", ErrorMessage = "Passwords must be 6 characters and contain at least 1 of the following: Number (0-9) and special character (e.g. !@#$%^&*)")]
        public string NewPassword { get; set; }

        //public bool IsNullValue(object? obj)
        //{
        //    var request = obj as ChangePasswordRequest;
        //    if (string.IsNullOrEmpty(request.UserId)) return true;
        //    if (string.IsNullOrEmpty(request.OldPassword)) return true;
        //    if (string.IsNullOrEmpty(request.NewPassword)) return true;
        //    return false;
        //}
    }
}
