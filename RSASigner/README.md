# RSA Signer (Signer.exe)

Client-side console application for signing API requests using RSA private keys.

## Overview

RSA Signer signs API request payloads to prove authenticity to API servers. It uses industry-standard RSA-2048 with SHA-256 hashing and PKCS#1 v1.5 padding.

## Requirements

- Windows with .NET Framework 4.8
- RSA private key in PEM format
- OpenSSL (for key generation)

## Installation

1. Build the project:
   ```batch
   msbuild RSASigner.csproj /p:Configuration=Release
   ```

2. The executable will be created at: `bin\Release\Signer.exe`

## Usage

### Command Line Syntax

```batch
# Sign a file
Signer.exe -file <path> -key <keyfile> [-output <file>] [-encoding base64|hex]

# Sign string data
Signer.exe -payload <string> -key <keyfile> [-output <file>] [-encoding base64|hex]
```

### Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `-file` | Yes* | Path to file containing payload to sign |
| `-payload` | Yes* | String data to sign directly |
| `-key` | Yes | Path to RSA private key file (PEM format) |
| `-output` | No | Output file path (default: console output) |
| `-encoding` | No | Output encoding: `base64` (default) or `hex` |

*Either `-file` or `-payload` is required (mutually exclusive)

### Examples

```batch
# Sign a JSON API request
Signer.exe -file request.json -key api-private-key.pem

# Sign and save to file
Signer.exe -file request.json -key api-private-key.pem -output signature.txt

# Sign string data directly
Signer.exe -payload "POST /api/data HTTP/1.1" -key api-private-key.pem

# Output in hex format
Signer.exe -file data.json -key key.pem -encoding hex
```

## Key Generation

Generate a private key using OpenSSL:

```batch
# Generate 2048-bit private key
openssl genrsa -out api-private-key.pem 2048

# Extract public key (for server)
openssl rsa -in api-private-key.pem -pubout -out api-public-key.pem
```

## Integration Example

### API Client Workflow

1. **Create request payload**:
   ```batch
   echo {"userId": 123, "action": "transfer", "amount": 500} > request.json
   ```

2. **Sign the payload**:
   ```batch
   Signer.exe -file request.json -key api-private-key.pem -output signature.b64
   ```

3. **Send HTTP request**:
   ```http
   POST /api/transfer HTTP/1.1
   Host: api.example.com
   X-Signature: <content_of_signature.b64>
   Content-Type: application/json

   {"userId": 123, "action": "transfer", "amount": 500}
   ```

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 2 | Invalid arguments |
| 3 | File not found |
| 4 | Invalid key format |
| 5 | General error |

## Security Considerations

- **Private Key Security**: Never share or transmit private keys
- **File Permissions**: Restrict access to private key files
- **Key Storage**: Use secure key stores in production
- **Key Rotation**: Implement regular key updates

## Troubleshooting

### Common Issues

1. **"File not found"**
   - Check file paths are correct
   - Use absolute paths if needed

2. **"Invalid key format"**
   - Ensure key is in PEM format
   - Check for proper BEGIN/END markers

3. **"Failed to import RSA private key"**
   - Verify key was generated correctly
   - Try regenerating the key pair

### Getting Help

```batch
Signer.exe -help
```

## Technical Details

- **Algorithm**: RSA-2048
- **Hash Function**: SHA-256
- **Padding**: PKCS#1 v1.5
- **Key Format**: PEM (RFC 7468)
- **Output**: Base64 or Hexadecimal encoding