using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Models.User
{

    /* It is always a sounf practice to save and record every action of the user. Especially in a finance related application.
 * This entity will help us track otp records of a user in the system
 */
    [Table("OtpRecord", Schema = "dbo")]
    public class OtpRecord
    {
        [Key]
        [StringLength(50)]
        public string RecId { get; set; }
        [StringLength(50)]
        public string UserId { get; set; }
        [StringLength(10)]
        public string Otp { get; set; }
        [StringLength(100)]
        public string Value { get; set; }
        [StringLength(10)]
        public string Type { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsUsed { get; set; }
    }
}
