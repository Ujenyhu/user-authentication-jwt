using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Models
{
    public class Country
    {
        [Key]
        public long RecId { get; set; }
        [Required]
        public string DialingCode { get; set; }
        [Required]
        public string CountryName { get; set; }
        public string? TelephoneLimit { get; set; }
        [StringLength(500)]
        public string? Flag { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
