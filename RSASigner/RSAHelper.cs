using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RSASigner
{
    public static class RSAHelper
    {
        public static string SignPayload(string payload, string privateKeyPem)
        {
            try
            {
                using (var rsa = RSA.Create())
                {
                    // Import the PEM private key
                    ImportRSAPrivateKey(rsa, privateKeyPem);

                    // Sign the payload with SHA256
                    byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
                    byte[] signature = rsa.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                    return Convert.ToBase64String(signature);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to sign payload: {ex.Message}", ex);
            }
        }

        public static string SignPayloadWithEncoding(string payload, string privateKeyPem, string encoding)
        {
            try
            {
                using (var rsa = RSA.Create())
                {
                    // Import the PEM private key
                    ImportRSAPrivateKey(rsa, privateKeyPem);

                    // Sign the payload with SHA256
                    byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
                    byte[] signature = rsa.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                    // Return in requested encoding
                    switch (encoding.ToLower())
                    {
                        case "base64":
                            return Convert.ToBase64String(signature);
                        case "hex":
                            return BitConverter.ToString(signature).Replace("-", "").ToLower();
                        default:
                            throw new ArgumentException($"Unsupported encoding: {encoding}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to sign payload: {ex.Message}", ex);
            }
        }

        private static void ImportRSAPrivateKey(RSA rsa, string privateKeyPem)
        {
            try
            {
                // Use the modern ImportFromPem method in .NET 8
                rsa.ImportFromPem(privateKeyPem);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to import RSA private key: {ex.Message}", ex);
            }
        }

        private static string ExtractKeyFromPem(string pem, string keyType)
        {
            string header = $"-----BEGIN {keyType}-----";
            string footer = $"-----END {keyType}-----";

            int start = pem.IndexOf(header);
            if (start == -1) return null;

            start += header.Length;
            int end = pem.IndexOf(footer, start);
            if (end == -1) return null;

            return pem.Substring(start, end - start)
                      .Replace("\n", "")
                      .Replace("\r", "")
                      .Replace(" ", "")
                      .Replace("\t", "");
        }
    }
}