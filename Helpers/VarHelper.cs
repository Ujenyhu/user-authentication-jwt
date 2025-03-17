namespace userauthjwt.Helpers
{
    public class VarHelper
    {

        #region Default Variables

        public enum ResponseStatus
        {
            SUCCESS,
            PENDING,
            ERROR
        }


        public enum MetaDataTypes
        {
            USER_TYPE,
            USER_STATUS,
            OTP_TYPE,
        }
        #endregion




        #region User Variables

        public enum OtpTypes
        {
            SMS,
            EMAIL,
        }

        public enum UserTypes
        {
            PUBLIC,
        }

        public enum UserStatus
        {
            ONBOARDING,
            ENABLED,
            DISABLED,
        }

        public enum SecurityAttemptTypes
        {
            LOGIN,
            PASSWORDCHANGE,
        }

        public enum AuditType
        {
            NONE = 0,
            CREATE = 1,
            UPDATE = 2,
            DELETE = 3
        }


        public const string loginSecurityMsg = "Multiple failed login attempt(s) has been carried out on your account, If this action was not performed by you, please contact customer care.";
        public const string PasswordChangeSecurityMsg = "Multiple failed passwod change attempt(s) has been carried out on your account, If this action was not performed by you, please contact customer care.";
    


        #endregion
    }
}
