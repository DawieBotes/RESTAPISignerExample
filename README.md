# REST API Signer Example

Two .NET 8 console applications implementing industry-standard RSA signature operations for REST API authentication:

- **Signer.exe**: Tool that signs API requests using RSA private keys
- **Verifier.exe**: Tool that verifies API signatures using RSA public keys

## Quick Start

### 1. Build the Applications

```batch
# Build entire solution
dotnet build RESTAPISignerExample.sln --configuration Release

# Or build individually
dotnet build RSASigner\RSASigner.csproj --configuration Release
dotnet build RSAVerifier\RSAVerifier.csproj --configuration Release
```

### 2. Generate Test Keys

```batch
cd TestKeys
generate-keys.bat
```

### 3. Test the Workflow

```batch
# Sign a request
RSASigner\bin\Release\net8.0\Signer.exe -file Examples\sample-request.json -key TestKeys\api-private-key.pem -output signature.txt

# Verify the signature
RSAVerifier\bin\Release\net8.0\Verifier.exe -file Examples\sample-request.json -sigfile signature.txt -key TestKeys\api-public-key.pem
```

## Executable Locations

After building, the executable files are located at:

- **Signer.exe**: `RSASigner\bin\Release\net8.0\Signer.exe`
- **Verifier.exe**: `RSAVerifier\bin\Release\net8.0\Verifier.exe`

These are native Windows executables that can be run directly without requiring `dotnet` command.

## Project Structure

```
RESTAPISignerExample/
├── RSASigner/              # Client-side signing application
│   ├── Program.cs         # Main application logic
│   ├── SignerOptions.cs   # Command line parsing
│   ├── RSAHelper.cs       # RSA cryptographic operations
│   └── README.md          # Detailed usage documentation
├── RSAVerifier/           # Server-side verification application
│   ├── Program.cs         # Main application logic
│   ├── VerifierOptions.cs # Command line parsing
│   ├── RSAHelper.cs       # RSA cryptographic operations
│   └── README.md          # Detailed usage documentation
├── TestKeys/              # Sample RSA keys and generation script
│   └── generate-keys.bat  # Windows batch script for key generation
├── Examples/              # Sample data and integration examples
│   ├── sample-request.json
│   └── integration-examples.md
└── Docs/                  # Technical specification
    └── rsa-api-spec-refined.md
```

## Executable Usage

### Signer Application

The Signer application creates RSA signatures for API payloads:

```batch
# Basic usage
RSASigner\bin\Release\net8.0\Signer.exe -file <payload-file> -key <private-key.pem>

# With output file
RSASigner\bin\Release\net8.0\Signer.exe -file Examples\sample-request.json -key TestKeys\api-private-key.pem -output signature.txt

# Direct payload signing
RSASigner\bin\Release\net8.0\Signer.exe -payload "Hello World" -key TestKeys\api-private-key.pem

# Hex encoding output
RSASigner\bin\Release\net8.0\Signer.exe -file Examples\sample-request.json -key TestKeys\api-private-key.pem -encoding hex
```

**Signer Parameters:**
- `-file <path>` - Path to file containing payload to sign (mutually exclusive with -payload)
- `-payload <string>` - String data to sign directly (mutually exclusive with -file)
- `-key <path>` - Path to RSA private key file (PEM format)
- `-output <path>` - Output file path (default: console output)
- `-encoding <format>` - Output encoding: base64 (default) or hex

### Verifier Application

The Verifier application validates RSA signatures against payloads:

```batch
# Basic usage with signature file
RSAVerifier\bin\Release\net8.0\Verifier.exe -file <payload-file> -sigfile <signature-file> -key <public-key.pem>

# Direct signature verification
RSAVerifier\bin\Release\net8.0\Verifier.exe -file Examples\sample-request.json -signature "base64sig..." -key TestKeys\api-public-key.pem

# Direct payload verification
RSAVerifier\bin\Release\net8.0\Verifier.exe -payload "Hello World" -signature "base64sig..." -key TestKeys\api-public-key.pem

# Hex signature format
RSAVerifier\bin\Release\net8.0\Verifier.exe -file Examples\sample-request.json -signature "hexsig..." -key TestKeys\api-public-key.pem -encoding hex
```

**Verifier Parameters:**
- `-file <path>` - Path to file containing payload to verify (mutually exclusive with -payload)
- `-payload <string>` - String data to verify directly (mutually exclusive with -file)
- `-signature <string>` - Signature string (mutually exclusive with -sigfile)
- `-sigfile <path>` - Path to file containing signature (mutually exclusive with -signature)
- `-key <path>` - Path to RSA public key file (PEM format)
- `-encoding <format>` - Signature encoding: base64 (default) or hex

**Verifier Exit Codes:**
- `0` = Valid signature
- `1` = Invalid signature
- `2` = Invalid arguments
- `3` = File not found
- `4` = Invalid key format
- `5` = Verification error

## Requirements

- .NET 8 Runtime or SDK
- OpenSSL (for key generation)
- Windows, Linux, or macOS

## Key Features

- **Industry Standard**: RSA-2048, SHA-256, PKCS#1 v1.5 padding
- **Cross-Platform**: Runs on Windows, Linux, and macOS with .NET 8
- **Single Executables**: No external dependencies beyond .NET 8 runtime
- **Flexible Input**: File-based or direct string payload signing/verification
- **Multiple Encodings**: Base64 (default) or hexadecimal output
- **Clear Exit Codes**: Proper integration with scripts and automation
- **Modern .NET APIs**: Uses latest RSA cryptography features
- **Comprehensive Documentation**: Detailed usage guides and examples

## Security Architecture

- **Client holds private key**: Signs outgoing API requests
- **Server holds public key**: Verifies incoming request signatures
- **Standard PEM format**: Compatible with OpenSSL and other tools
- **No key transmission**: Private keys never leave the client system

## Documentation

- [RSA Signer Documentation](RSASigner/README.md) - Client-side signing tool
- [RSA Verifier Documentation](RSAVerifier/README.md) - Server-side verification tool
- [Integration Examples](Examples/integration-examples.md) - Real-world usage scenarios
- [Technical Specification](Docs/rsa-api-spec-refined.md) - Complete implementation details
