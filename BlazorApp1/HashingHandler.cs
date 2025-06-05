using System.Security.Cryptography;
using System.Text;

namespace BlazorApp1
{
    // Simple interface for depency injection
    public interface IHashingHandler
    {
        string BCryptHash(string toBeHashed);
        bool VerifyBCrypt(string value, string hashedValue);
        string BCryptHash2(string toBeHashed);
        bool VerifyBCrypt2(string value, string hashedValue);

        string SHA256Hash(string value);
        bool VerifySHA256Hash(string value, string hashedValue);

        string HMACSHA256Hash(string key, string input);
        bool VerifyHMACSHA256Hash(string key, string input, string expectedHash);

        string PBKDF2Hash(string value, string salt, int iterations = 10000, int hashByteSize = 32);
        bool VerifyPBKDF2Hash(string value, string salt, int iterations, string expectedHash, int hashByteSize = 32);
    }

    public class HashingHandler : IHashingHandler
    {
        public string BCryptHash(string ToBeHashed)
        {
            return BCrypt.Net.BCrypt.HashPassword(ToBeHashed);
        }

        public string BCryptHash2(string ToBeHashed)
        {
            return BCrypt.Net.BCrypt.HashPassword(ToBeHashed, BCrypt.Net.BCrypt.GenerateSalt(10), true, BCrypt.Net.HashType.SHA256);
        }

        public bool VerifyBCrypt(string value, string hashedValue)
        {
            return BCrypt.Net.BCrypt.Verify(value, hashedValue);
        }

        public bool VerifyBCrypt2(string value, string hashedValue)
        {
            return BCrypt.Net.BCrypt.Verify(value, hashedValue, true, BCrypt.Net.HashType.SHA256);
        }

        public string SHA256Hash(string value)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] valueBytes = Encoding.UTF8.GetBytes(value);
                byte[] hashBytes = sha256.ComputeHash(valueBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifySHA256Hash(string value, string hashedValue)
        {
            string computedHash = SHA256Hash(value);
            return computedHash == hashedValue;
        }

        public string HMACSHA256Hash(string key, string input)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = hmac.ComputeHash(inputBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyHMACSHA256Hash(string key, string input, string expectedHash)
        {
            string computedHash = HMACSHA256Hash(key, input);
            return computedHash == expectedHash;
        }

        public string PBKDF2Hash(string value, string salt, int iterations = 10000, int hashByteSize = 32)
        {
            // Convert salt to bytes.
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(value, saltBytes, iterations, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(hashByteSize);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyPBKDF2Hash(string value, string salt, int iterations, string expectedHash, int hashByteSize = 32)
        {
            string computedHash = PBKDF2Hash(value, salt, iterations, hashByteSize);
            return computedHash == expectedHash;
        }
    }
}
