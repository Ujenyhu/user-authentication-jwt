using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Requests.User
{
    public class UploadImageRequest
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string ImageBase64String { get; set; }
    }
}
