using System.ComponentModel.DataAnnotations;
using userauthjwt.DataAccess.Interfaces;

namespace userauthjwt.Models.User
{
    public class UserRegistration : ISoftDelete
    {
        [Key]
        public string UserId { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }
        public string Username { get; set; }

        [Required]
        [StringLength(20)]
        public string Telephone { get; set; }
        [StringLength(100)]
        public string EmailAddress { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PasswordSalt { get; set; }
        public bool TelephoneVerified { get; set; }
        public bool EmailVerified { get; set; }
        [StringLength(10)]
        public string? DeviceType { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceToken { get; set; }
        public string UserType { get; set; }
        public string UserStatus { get; set; }
        public DateTime DateRegistered { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
