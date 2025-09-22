using System;

namespace RSASigner
{
    public class SignerOptions
    {
        public string PayloadPath { get; set; }
        public string KeyPath { get; set; }
        public string OutputPath { get; set; }
        public string Encoding { get; set; } = "base64";
        public bool IsFile { get; set; }

        public static SignerOptions ParseArguments(string[] args)
        {
            var options = new SignerOptions();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-file":
                        if (i + 1 >= args.Length) throw new ArgumentException("Missing value for -file parameter");
                        options.PayloadPath = args[++i];
                        options.IsFile = true;
                        break;

                    case "-payload":
                        if (i + 1 >= args.Length) throw new ArgumentException("Missing value for -payload parameter");
                        options.PayloadPath = args[++i];
                        options.IsFile = false;
                        break;

                    case "-key":
                        if (i + 1 >= args.Length) throw new ArgumentException("Missing value for -key parameter");
                        options.KeyPath = args[++i];
                        break;

                    case "-output":
                        if (i + 1 >= args.Length) throw new ArgumentException("Missing value for -output parameter");
                        options.OutputPath = args[++i];
                        break;

                    case "-encoding":
                        if (i + 1 >= args.Length) throw new ArgumentException("Missing value for -encoding parameter");
                        options.Encoding = args[++i].ToLower();
                        if (options.Encoding != "base64" && options.Encoding != "hex")
                            throw new ArgumentException("Encoding must be 'base64' or 'hex'");
                        break;

                    case "-h":
                    case "-help":
                    case "--help":
                        ShowUsage();
                        Environment.Exit(0);
                        break;

                    default:
                        throw new ArgumentException($"Unknown parameter: {args[i]}");
                }
            }

            ValidateOptions(options);
            return options;
        }

        private static void ValidateOptions(SignerOptions options)
        {
            if (string.IsNullOrEmpty(options.PayloadPath))
                throw new ArgumentException("Either -file or -payload parameter is required");

            if (string.IsNullOrEmpty(options.KeyPath))
                throw new ArgumentException("-key parameter is required");
        }

        private static void ShowUsage()
        {
            Console.WriteLine("RSA API Request Signer");
            Console.WriteLine("Signs API request payloads using RSA private keys");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  Signer.exe -file <path> -key <keyfile> [-output <file>] [-encoding base64|hex]");
            Console.WriteLine("  Signer.exe -payload <string> -key <keyfile> [-output <file>] [-encoding base64|hex]");
            Console.WriteLine();
            Console.WriteLine("Parameters:");
            Console.WriteLine("  -file      Path to file containing payload to sign (mutually exclusive with -payload)");
            Console.WriteLine("  -payload   String data to sign directly (mutually exclusive with -file)");
            Console.WriteLine("  -key       Path to RSA private key file (PEM format)");
            Console.WriteLine("  -output    Output file path (default: console output)");
            Console.WriteLine("  -encoding  Output encoding: base64 (default) or hex");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  Signer.exe -file request.json -key private-key.pem");
            Console.WriteLine("  Signer.exe -payload \"API request data\" -key private-key.pem -output signature.txt");
            Console.WriteLine("  Signer.exe -file data.json -key key.pem -encoding hex");
        }
    }
}