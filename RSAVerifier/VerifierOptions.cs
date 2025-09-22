using System;

namespace RSAVerifier
{
    public class VerifierOptions
    {
        public string PayloadPath { get; set; }
        public string SignaturePath { get; set; }
        public string KeyPath { get; set; }
        public string Encoding { get; set; } = "base64";
        public bool IsFile { get; set; }
        public bool IsSigFile { get; set; }

        public static VerifierOptions ParseArguments(string[] args)
        {
            var options = new VerifierOptions();

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

                    case "-signature":
                        if (i + 1 >= args.Length) throw new ArgumentException("Missing value for -signature parameter");
                        options.SignaturePath = args[++i];
                        options.IsSigFile = false;
                        break;

                    case "-sigfile":
                        if (i + 1 >= args.Length) throw new ArgumentException("Missing value for -sigfile parameter");
                        options.SignaturePath = args[++i];
                        options.IsSigFile = true;
                        break;

                    case "-key":
                        if (i + 1 >= args.Length) throw new ArgumentException("Missing value for -key parameter");
                        options.KeyPath = args[++i];
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

        private static void ValidateOptions(VerifierOptions options)
        {
            if (string.IsNullOrEmpty(options.PayloadPath))
                throw new ArgumentException("Either -file or -payload parameter is required");

            if (string.IsNullOrEmpty(options.SignaturePath))
                throw new ArgumentException("Either -signature or -sigfile parameter is required");

            if (string.IsNullOrEmpty(options.KeyPath))
                throw new ArgumentException("-key parameter is required");
        }

        private static void ShowUsage()
        {
            Console.WriteLine("RSA API Signature Verifier");
            Console.WriteLine("Verifies API request signatures using RSA public keys");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  Verifier.exe -file <path> -signature <sig> -key <keyfile> [-encoding base64|hex]");
            Console.WriteLine("  Verifier.exe -file <path> -sigfile <sigfile> -key <keyfile> [-encoding base64|hex]");
            Console.WriteLine("  Verifier.exe -payload <string> -signature <sig> -key <keyfile> [-encoding base64|hex]");
            Console.WriteLine();
            Console.WriteLine("Parameters:");
            Console.WriteLine("  -file      Path to file containing payload to verify (mutually exclusive with -payload)");
            Console.WriteLine("  -payload   String data to verify directly (mutually exclusive with -file)");
            Console.WriteLine("  -signature Signature string (mutually exclusive with -sigfile)");
            Console.WriteLine("  -sigfile   Path to file containing signature (mutually exclusive with -signature)");
            Console.WriteLine("  -key       Path to RSA public key file (PEM format)");
            Console.WriteLine("  -encoding  Signature encoding: base64 (default) or hex");
            Console.WriteLine();
            Console.WriteLine("Exit Codes:");
            Console.WriteLine("  0 = Valid signature");
            Console.WriteLine("  1 = Invalid signature");
            Console.WriteLine("  2 = Invalid arguments");
            Console.WriteLine("  3 = File not found");
            Console.WriteLine("  4 = Invalid key format");
            Console.WriteLine("  5 = Verification error");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  Verifier.exe -file request.json -signature \"base64sig...\" -key public-key.pem");
            Console.WriteLine("  Verifier.exe -file request.json -sigfile signature.txt -key public-key.pem");
            Console.WriteLine("  Verifier.exe -payload \"API data\" -signature \"sig...\" -key key.pem");
        }
    }
}