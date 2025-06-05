using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Cryptography;
using System.Text;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EncryptionController : ControllerBase
    {
        [HttpPost("encrypt")]
        public IActionResult Encrypt([FromBody] EncryptRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.PlainText) || string.IsNullOrEmpty(request.PublicKey))
            {
                return BadRequest("Invalid parameters.");
            }

            try
            {
                byte[] publicKeyBytes = Convert.FromBase64String(request.PublicKey);

                using (RSA rsa = RSA.Create())
                {
                    rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

                    // Convert the text to bytes.
                    byte[] plainBytes = Encoding.UTF8.GetBytes(request.PlainText);

                    byte[] encryptedBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);

                    string encryptedBase64 = Convert.ToBase64String(encryptedBytes);
                    return Ok(encryptedBase64);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Encryption failed: {ex.Message}");
            }
        }

        [HttpGet("generatekeys")]
        public IActionResult GenerateKeys()
        {
            try
            {
                using (RSA rsa = RSA.Create())
                {
                    byte[] privateKeyBytes = rsa.ExportRSAPrivateKey();
                    byte[] publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();

                    string privateKeyBase64 = Convert.ToBase64String(privateKeyBytes);
                    string publicKeyBase64 = Convert.ToBase64String(publicKeyBytes);

                    // Return both as json
                    return Ok(new
                    {
                        PrivateKey = privateKeyBase64,
                        PublicKey = publicKeyBase64
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Private Key generation failed: {ex.Message}");
            }
        }
    }

    public class EncryptRequest
    {
        public string PlainText { get; set; }
        public string PublicKey { get; set; }
    }
}
