using System;
using System.IO;

namespace RSASigner
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                // Parse command line arguments
                var options = SignerOptions.ParseArguments(args);

                // Load payload (what we're signing)
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

                // Load private key
                if (!File.Exists(options.KeyPath))
                {
                    Console.Error.WriteLine($"Error: Private key file not found: {options.KeyPath}");
                    return 3; // File not found
                }

                string privateKeyPem = File.ReadAllText(options.KeyPath);

                // Sign the payload
                string signature = RSAHelper.SignPayloadWithEncoding(payload, privateKeyPem, options.Encoding);

                // Output signature
                if (!string.IsNullOrEmpty(options.OutputPath))
                {
                    try
                    {
                        File.WriteAllText(options.OutputPath, signature);
                        Console.WriteLine($"Signature written to: {options.OutputPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error writing to output file: {ex.Message}");
                        return 5; // File write error
                    }
                }
                else
                {
                    Console.WriteLine(signature);
                }

                return 0; // Success
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
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error: {ex.Message}");
                return 5; // General error
            }
        }
    }
}