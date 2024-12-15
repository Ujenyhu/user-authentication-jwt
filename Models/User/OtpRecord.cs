using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Models.User
{
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
