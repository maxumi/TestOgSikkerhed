using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace BlazorApp1.Encryption
{
    // Simple interface for depency injection
    public interface IAsymmetricEncryptionService
    {
        byte[] GetPublicKey();
        string Decrypt(byte[] cipherText);
        Task<string> EncryptTodoViaApiAsync(string plainText);

    }

    public class AsymmetricEncryptionService : IAsymmetricEncryptionService
    {
        private readonly RSA _rsa;
        private readonly HttpClient _httpClient;

        public byte[] PublicKey { get; }
        private byte[] _PrivateKey { get; }

        public AsymmetricEncryptionService(string privateKeyBase64, HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Convert the base-64 string to bytes.
            byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
            _rsa = RSA.Create();
            _rsa.ImportRSAPrivateKey(privateKeyBytes, out _);

            _PrivateKey = _rsa.ExportRSAPrivateKey();
            PublicKey = _rsa.ExportSubjectPublicKeyInfo();
        }

        public byte[] GetPublicKey()
        {
            return _rsa.ExportSubjectPublicKeyInfo();
        }

        public byte[] GetPrivateKey()
        {
            return _rsa.ExportRSAPrivateKey();
        }

        public string Decrypt(byte[] cipherText)
        {
            byte[] decryptedBytes = _rsa.Decrypt(cipherText, RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        // Test endpoint in api so it test encryption on WebApi
        public async Task<string> EncryptTodoViaApiAsync(string plainText)
        {
            string publicKeyBase64 = Convert.ToBase64String(GetPublicKey());

            var request = new EncryptRequest
            {
                PlainText = plainText,
                PublicKey = publicKeyBase64
            };

            // Call the external encryption WebApi.
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7145/api/encryption/encrypt", request);
            response.EnsureSuccessStatusCode();

            string encryptedBase64 = await response.Content.ReadAsStringAsync();
            return encryptedBase64;
        }
    }
    // Model for the encryption request Sent to the Web api project
    public class EncryptRequest
    {
        public string PlainText { get; set; }
        public string PublicKey { get; set; }
    }
}
