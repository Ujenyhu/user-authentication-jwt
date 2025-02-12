using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Models.Configs
{
    [Table("MailConfig", Schema = "dbo")]
    public partial class MailConfig
    {
        [Key]
        public long RecId { get; set; }
        [StringLength(250)]
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public int SslSupport { get; set; }
        [Required]
        [StringLength(250)]
        public string SmtpUsername { get; set; }
        [Required]
        [StringLength(100)]
        public string SmtpPassword { get; set; }
        [StringLength(250)]
        public string DefaultEmail { get; set; }
    }
}
