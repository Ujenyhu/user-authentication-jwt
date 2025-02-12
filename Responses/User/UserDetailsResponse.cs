namespace userauthjwt.Responses.User
{
    public class UserDetailsResponse
    {
        public string UserId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Telephone { get; set; }
        public string EmailAddress { get; set; }
        public bool TelephoneVerified { get; set; }
        public bool EmailVerified { get; set; }
        public string UserStatus { get; set; }
        public string UserType { get; set; }
    }
}
