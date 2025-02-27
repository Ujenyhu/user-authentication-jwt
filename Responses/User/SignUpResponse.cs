using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Responses.User
{
    public class SignUpResponse
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Telephone { get; set; }
        [Required]
        public string UserType { get; set; }
    }
}
