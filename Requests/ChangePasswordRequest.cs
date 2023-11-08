using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Requests
{
    public class ChangePasswordRequest
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        [MaxLength(4)]
        public string OldPassword { get; set; }
        [Required]
        [MaxLength(4)]
        public string NewPassword { get; set; }

        public bool IsNullValue(object? obj)
        {
            var request = obj as ChangePasswordRequest;
            if (string.IsNullOrEmpty(request.UserId)) return true;
            if (string.IsNullOrEmpty(request.OldPassword)) return true;
            if (string.IsNullOrEmpty(request.NewPassword)) return true;
            return false;
        }
    }
}
