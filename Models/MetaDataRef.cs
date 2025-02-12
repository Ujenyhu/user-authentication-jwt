using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Models
{
    [Table("MetaDataRef", Schema = "dbo")]
    public partial class MetaDataRef
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        [StringLength(50)]
        public string ReferenceId { get; set; }
        [StringLength(500)]
        public string ReferenceName { get; set; }
        [Required]
        [StringLength(50)]
        public string MetaDataType { get; set; }
    }
}
