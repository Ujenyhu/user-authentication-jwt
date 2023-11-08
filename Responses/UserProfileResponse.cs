using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Responses
{
    public class UserProfileResponse
    {
        public string UserId { get; set; }
        
        public string Firstname { get; set; }
        
        public string Lastname { get; set; }
        public string Email { get; set; }
    }
}
