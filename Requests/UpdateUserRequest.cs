using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace userauthjwt.Requests
{
    public class UpdateUserRequest
    {
        public string UserId { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
    }
}
