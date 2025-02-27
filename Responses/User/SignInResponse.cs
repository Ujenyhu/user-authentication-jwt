using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Responses.User
{
    public class SignInResponse
    {
        [Required]
        public string UserId { get; set; }
        public string EmailAddress { get; set; }

        [Required]
        public string Username { get; set; }
        [Required]
        public string Token { get; set; }
        public string? RefreshToken { get; set; }
        [Required]
        public DateTime TokenExpirationDate { get; set; }
        [Required]
        public bool TelephoneVerified { get; set; }
        [Required]
        public bool EmailVerified { get; set; }
        [Required]
        public string UserType { get; set; }
    }
}
