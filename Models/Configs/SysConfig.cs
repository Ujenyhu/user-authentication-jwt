using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace userauthjwt.Models.Configs
{
    [Table("SysConfig", Schema = "dbo")]
    public class SysConfig
    {
        [Key]
        public long RecId { get; set; }
        public string Eck { get; set; }
        public int LoginTokenExpiration { get; set; }
        public int RefreshTokenExpiration { get; set; }
        public string OtpTokenExpiration { get; set; }
        public int? LoginAttemptMax { get; set; }
        public int? NotifyForLoginAttempt { get; set; }
        public int? LoginAttemptLockInHours { get; set; }

    }
}
