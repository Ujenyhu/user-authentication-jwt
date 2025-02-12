using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable disable
namespace userauthjwt.Requests.User
{
    public partial class SignUpRequest
    {

        public string CountryId { get; set; }

        [Required]
        [RegularExpression(@"^\d{7,14}$", ErrorMessage = "Invalid phone number. Only digits are allowed.")]
        [DefaultValue("string")]
        public string Telephone { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(20)]
        public string LastName { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(20)]
        public string FirstName { get; set; }


        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression("^(?=.*\\d)(?=.*[^a-zA-Z0-9]).{6}$", ErrorMessage = "Passwords must be 6 characters and contain at least 1 of the following: Number (0-9) and special character (e.g. !@#$%^&*)")]
        public string Password { get; set; }
    }
}
