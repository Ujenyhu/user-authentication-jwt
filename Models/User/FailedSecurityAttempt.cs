using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace userauthjwt.Models.User
{
    [Table("FailedSecurityAttempt", Schema = "dbo")]
    public class FailedSecurityAttempt
    {
        [Key]
        [StringLength(50)]
        public string RecId { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public string? IP { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
