# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a specification and example repository for RSA API signing and verification console applications. The repository contains comprehensive documentation for implementing two C# console applications:

- **Signer.exe**: Client-side tool that signs API requests using RSA private keys
- **Verifier.exe**: Server-side tool that verifies API signatures using RSA public keys

## Repository Structure

This is currently a documentation-only repository with the following structure:

```
├── README.md                           # Brief project description
├── Docs/
│   └── rsa-api-spec-refined.md        # Complete technical specification
└── LICENSE                            # MIT License
```

## Architecture Overview

The project follows the industry-standard pattern where:
- **API clients hold private keys** and use them to sign outgoing requests
- **API servers hold public keys** and use them to verify incoming request signatures

### Key Components (as specified in documentation)

1. **RSA Key Management**:
   - Uses OpenSSL for 2048-bit RSA key pair generation
   - PEM format for both private and public keys
   - Follows NIST recommendations (SHA-256, PKCS#1 v1.5 padding)

2. **Signer Application Architecture**:
   - Command-line interface for signing payloads
   - Supports file-based and direct string input
   - Base64 signature output (configurable to hex)
   - Uses .NET RSA cryptography libraries

3. **Verifier Application Architecture**:
   - Command-line interface for signature verification
   - Supports multiple input methods (file, string, signature file)
   - Clear exit codes for integration (0=valid, 1=invalid, 2-5=errors)
   - Uses .NET RSA cryptography libraries

## Key Generation Commands

The documentation specifies these OpenSSL commands for key generation:

```bash
# Generate 2048-bit RSA private key
openssl genrsa -out api-private-key.pem 2048

# Extract corresponding public key
openssl rsa -in api-private-key.pem -pubout -out api-public-key.pem

# Verify key pair
openssl rsa -in api-private-key.pem -check -noout
```

## Build Commands

**Build entire solution:**
```batch
msbuild RESTAPISignerExample.sln /p:Configuration=Release
```

**Build individual projects:**
```batch
msbuild RSASigner\RSASigner.csproj /p:Configuration=Release
msbuild RSAVerifier\RSAVerifier.csproj /p:Configuration=Release
```

**Debug builds:**
```batch
msbuild RESTAPISignerExample.sln /p:Configuration=Debug
```

## Executables Location

After building, the executables are located at:
- `RSASigner\bin\Release\Signer.exe` - Client-side signing tool
- `RSAVerifier\bin\Release\Verifier.exe` - Server-side verification tool

## Testing Commands

**Generate test keys:**
```batch
cd TestKeys
generate-keys.bat
```

**Test signing and verification:**
```batch
# Sign a sample request
RSASigner\bin\Release\Signer.exe -file Examples\sample-request.json -key TestKeys\api-private-key.pem -output signature.txt

# Verify the signature
RSAVerifier\bin\Release\Verifier.exe -file Examples\sample-request.json -sigfile signature.txt -key TestKeys\api-public-key.pem
```

## Implementation Status

**Completed**: Both console applications are fully implemented with:

- Complete C# source code for both Signer and Verifier
- Command-line argument parsing and validation
- RSA cryptographic operations using .NET Framework 4.8
- PEM key file parsing and import
- Base64 and hexadecimal encoding support
- Comprehensive error handling with specific exit codes
- Detailed documentation and usage examples

## Project Dependencies

- **Target Framework**: .NET Framework 4.8
- **Cryptography**: System.Security.Cryptography (built-in)
- **No external dependencies**: Self-contained executables
- **Key Generation**: OpenSSL (separate tool for key generation)