using System.ComponentModel.DataAnnotations;

#nullable disable
namespace userauthjwt.Requests
{
    public partial class SignUpRequest
    {
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Lastname { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        [Required]
        [MaxLength(4)]
        public string Password { get; set; }

        public bool IsNullValue(object? obj)
        {
            var request =  obj as SignUpRequest;
            if (string.IsNullOrEmpty(Firstname)) return true;
            if (string.IsNullOrEmpty(Lastname)) return true;
            if (string.IsNullOrEmpty(EmailAddress)) return true;
            if (string.IsNullOrEmpty(Password)) return true;
            return false;
        }
    }
}
