using System.Security.Cryptography;
using System.Text;

namespace userauthjwt.Helpers
{
    public class AppHelper
    {
        //You can use this to generate unique id like user ids
        public static string GetGuid()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GetNewUniqueId()
        {
            return DateTime.Now.ToFileTime().ToString();
        }
        public static string GetUnique8ByteKey()
        {
            try
            {
                int maxSize = 10;
                char[] chars = new char[62];
                string a;
                a = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                chars = a.ToCharArray();
                int size = maxSize;
                byte[] data = new byte[1];
                var crypto = new RNGCryptoServiceProvider();
                crypto.GetNonZeroBytes(data);
                size = maxSize;
                data = new byte[size];
                crypto.GetNonZeroBytes(data);
                StringBuilder result = new StringBuilder(size);
                foreach (byte b in data)
                { result.Append(chars[b % (chars.Length - 1)]); }
                return result.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static string HashPassword(string password, string salt)
        {
            try
            {
                using var bytes = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA256);
                var derivedRandomKey = bytes.GetBytes(32);
                var hash = Convert.ToBase64String(derivedRandomKey);
                return hash;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
