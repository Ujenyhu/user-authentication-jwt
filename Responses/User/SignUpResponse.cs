using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Responses.User
{
    public class SignUpResponse
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Telephone { get; set; }
        [Required]
        public string UserType { get; set; }
    }
}
