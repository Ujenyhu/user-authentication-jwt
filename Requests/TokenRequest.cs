namespace userauthjwt.Requests
{
     #nullable disable
    public class TokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public bool IsNullValue(object? obj)
        {
            var request = obj as TokenRequest;
            if (string.IsNullOrEmpty(AccessToken)) return true;
            if (string.IsNullOrEmpty(RefreshToken)) return true;
            return false;
        }
    }
}
