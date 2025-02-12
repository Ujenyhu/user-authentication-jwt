namespace userauthjwt.Responses.User
{
    public class AuthenticationResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
