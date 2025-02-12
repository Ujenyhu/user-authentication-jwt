using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Models.Configs
{
    [Table("SmsConfig", Schema = "dbo")]
    public partial class SmsConfig
    {
        [Key]
        public long RecId { get; set; }
        public string SenderId { get; set; }
        public string Type { get; set; }
        public string Channel { get; set; }
        public string ApiKey { get; set; }

    }
}
