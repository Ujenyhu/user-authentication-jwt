using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace userauthjwt.Models.User
{
    [Table("UserProfile", Schema = "dbo")]
    public partial class UserProfile
    {
        [Key]
        [StringLength(50)]
        public string UserId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Username { get; set; }

        public string Telephone { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string? Image { get; set; }
        public bool TelephoneVerified { get; set; }
        public bool EmailVerified { get; set; }
        public string UserType { get; set; }
        public string UserStatus { get; set; }
        public string? MobileRefreshToken { get; set; }
        public string? WebRefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        [StringLength(10)]
        public string? DeviceType { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceToken{ get; set; }
        public int? FailedLoginAttempt { get; set; }
        public DateTime? LastFailedLoginAttempt { get; set; }
        public bool? IsAccountLocked { get; set; }
        public bool? IsPinDisabled { get; set; }
        public int? FailedPinAttempt { get; set; }
        public DateTime? LastFailedPinAttempt { get; set; }
        public DateTime DateAdded { get; set; }
    }

}
