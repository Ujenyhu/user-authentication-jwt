using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Requests.User
{
    public class RefreshTokenRequest
    {
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
