namespace userauthjwt.Responses
{
    public class SignInResponse
    {
        public string Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime TokenExpirationDate { get; set; }
        public string UserId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string EmailAddress { get; set; }
    }
}

