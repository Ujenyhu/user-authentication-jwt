using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Models
{
    [Table("AuditLogs", Schema = "dbo")]
    public class AuditLogs
    {
        [Key]
        [StringLength(50)]
        public string RecId { get; set; }
        [StringLength(200)]
        public string? UserId { get; set; }
        public string? AppUser { get; set; }
        public string Type { get; set; }
        public string? IP { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        [StringLength(50)]
        public string TableName { get; set; }
        public DateTime DateTime { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string AffectedColumns { get; set; }
        [StringLength(50)]
        public string? PrimaryKey { get; set; }
    }
}
