using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BlazorApp1.Encryption
{
    // Simple interface for depency injection
    public interface ISymmetricEncryptionService
    {
        byte[] Encrypt(string plainText);
        string Decrypt(byte[] cipherText);
    }

    public class SymmetricEncryptionService : ISymmetricEncryptionService
    {
        public byte[] Key { get; }
        public byte[] IV { get; }

        // generates a new key and IV
        public SymmetricEncryptionService()
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();
                Key = aes.Key;
                IV = aes.IV;
            }
        }

        // Gey key and iv in constructor
        public SymmetricEncryptionService(byte[] key, byte[] iv)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            IV = iv ?? throw new ArgumentNullException(nameof(iv));
        }

        public byte[] Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                using var encryptor = aes.CreateEncryptor();
                using var ms = new MemoryStream();
                using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                using var sw = new StreamWriter(cs);
                sw.Write(plainText);
                sw.Close();
                return ms.ToArray();
            }
        }

        public string Decrypt(byte[] cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream(cipherText);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
        }
    }
}
