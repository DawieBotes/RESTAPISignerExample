using System;
using System.Security.Cryptography;
using System.Text;

namespace RSAVerifier
{
    public static class RSAHelper
    {
        public static bool VerifySignature(string payload, string signature, string publicKeyPem, string encoding = "base64")
        {
            try
            {
                using (var rsa = RSA.Create())
                {
                    // Import the PEM public key
                    ImportRSAPublicKey(rsa, publicKeyPem);

                    // Convert payload to bytes
                    byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

                    // Convert signature to bytes based on encoding
                    byte[] signatureBytes = DecodeSignature(signature, encoding);

                    // Verify the signature
                    return rsa.VerifyData(payloadBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to verify signature: {ex.Message}", ex);
            }
        }

        private static byte[] DecodeSignature(string signature, string encoding)
        {
            switch (encoding.ToLower())
            {
                case "base64":
                    return Convert.FromBase64String(signature);
                case "hex":
                    return HexStringToBytes(signature);
                default:
                    throw new ArgumentException($"Unsupported encoding: {encoding}");
            }
        }

        private static byte[] HexStringToBytes(string hex)
        {
            // Remove any spaces or dashes
            hex = hex.Replace(" ", "").Replace("-", "");

            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must have an even number of characters");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        private static void ImportRSAPublicKey(RSA rsa, string publicKeyPem)
        {
            try
            {
                // Use the modern ImportFromPem method in .NET 8
                rsa.ImportFromPem(publicKeyPem);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to import RSA public key: {ex.Message}", ex);
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