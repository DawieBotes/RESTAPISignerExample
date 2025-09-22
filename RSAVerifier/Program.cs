using System;
using System.IO;

namespace RSAVerifier
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                // Parse command line arguments
                var options = VerifierOptions.ParseArguments(args);

                // Load payload (what was signed)
                string payload;
                if (options.IsFile)
                {
                    if (!File.Exists(options.PayloadPath))
                    {
                        Console.Error.WriteLine($"Error: Payload file not found: {options.PayloadPath}");
                        return 3; // File not found
                    }
                    payload = File.ReadAllText(options.PayloadPath);
                }
                else
                {
                    payload = options.PayloadPath; // Direct payload string
                }

                // Load signature
                string signature;
                if (options.IsSigFile)
                {
                    if (!File.Exists(options.SignaturePath))
                    {
                        Console.Error.WriteLine($"Error: Signature file not found: {options.SignaturePath}");
                        return 3; // File not found
                    }
                    signature = File.ReadAllText(options.SignaturePath).Trim();
                }
                else
                {
                    signature = options.SignaturePath; // Direct signature string
                }

                // Load public key
                if (!File.Exists(options.KeyPath))
                {
                    Console.Error.WriteLine($"Error: Public key file not found: {options.KeyPath}");
                    return 3; // File not found
                }

                string publicKeyPem = File.ReadAllText(options.KeyPath);

                // Verify signature
                bool isValid = RSAHelper.VerifySignature(payload, signature, publicKeyPem, options.Encoding);

                if (isValid)
                {
                    Console.WriteLine("VALID: Signature verification successful");
                    return 0; // Valid signature
                }
                else
                {
                    Console.WriteLine("INVALID: Signature verification failed");
                    return 1; // Invalid signature
                }
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 2; // Invalid arguments
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.WriteLine($"Error: File not found: {ex.FileName}");
                return 3; // File not found
            }
            catch (InvalidOperationException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 4; // Invalid key format or crypto error
            }
            catch (FormatException ex)
            {
                Console.Error.WriteLine($"Error: Invalid signature format: {ex.Message}");
                return 4; // Invalid signature format
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error: {ex.Message}");
                return 5; // General error
            }
        }
    }
}