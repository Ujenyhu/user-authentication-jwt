#nullable disable
using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Requests
{
    public class SignInRequest
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        [Required]
        [MaxLength(4)]
        public string Password { get; set; }

        public bool IsNullValue(object? obj)
        {
            var request = obj as SignInRequest;
            if (string.IsNullOrEmpty(EmailAddress)) return true;
            if (string.IsNullOrEmpty(Password)) return true;
            return false;
        }
    }
}
