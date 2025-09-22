# RSA Verifier (Verifier.exe)

Server-side console application for verifying API request signatures using RSA public keys.

## Overview

RSA Verifier validates that API request signatures were created by the holder of the corresponding private key. This authenticates clients to the API server using industry-standard RSA cryptography.

## Requirements

- Windows with .NET Framework 4.8
- RSA public key in PEM format
- Signature to verify (Base64 or hex encoded)

## Installation

1. Build the project:
   ```batch
   msbuild RSAVerifier.csproj /p:Configuration=Release
   ```

2. The executable will be created at: `bin\Release\Verifier.exe`

## Usage

### Command Line Syntax

```batch
# Verify with signature string
Verifier.exe -file <path> -signature <sig> -key <keyfile> [-encoding base64|hex]
Verifier.exe -payload <string> -signature <sig> -key <keyfile> [-encoding base64|hex]

# Verify with signature file
Verifier.exe -file <path> -sigfile <sigfile> -key <keyfile> [-encoding base64|hex]
```

### Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `-file` | Yes* | Path to file containing payload to verify |
| `-payload` | Yes* | String data to verify directly |
| `-signature` | Yes** | Signature string |
| `-sigfile` | Yes** | Path to file containing signature |
| `-key` | Yes | Path to RSA public key file (PEM format) |
| `-encoding` | No | Signature encoding: `base64` (default) or `hex` |

*Either `-file` or `-payload` is required (mutually exclusive)
**Either `-signature` or `-sigfile` is required (mutually exclusive)

### Examples

```batch
# Verify request with signature string
Verifier.exe -file request.json -signature "base64signature..." -key api-public-key.pem

# Verify with signature from file
Verifier.exe -file request.json -sigfile signature.txt -key api-public-key.pem

# Verify string payload
Verifier.exe -payload "API request data" -signature "signature" -key key.pem

# Verify hex-encoded signature
Verifier.exe -file data.json -signature "hexsignature" -key key.pem -encoding hex
```

## Integration Example

### API Server Workflow

1. **Receive HTTP request** with signature header:
   ```http
   POST /api/transfer HTTP/1.1
   X-Signature: base64_signature_here
   Content-Type: application/json

   {"userId": 123, "action": "transfer", "amount": 500}
   ```

2. **Save request body** to temporary file:
   ```batch
   # (Done by your web application)
   echo %REQUEST_BODY% > temp_request.json
   ```

3. **Verify signature**:
   ```batch
   Verifier.exe -file temp_request.json -signature "%X_SIGNATURE%" -key client-public-key.pem
   ```

4. **Process based on result**:
   ```batch
   if %errorlevel% equ 0 (
       echo Valid signature - process request
   ) else (
       echo Invalid signature - reject request
   )
   ```

## Exit Codes

| Code | Meaning | Action |
|------|---------|--------|
| 0 | Valid signature | Authenticate and process request |
| 1 | Invalid signature | Reject request |
| 2 | Invalid arguments | Check command syntax |
| 3 | File not found | Verify file paths |
| 4 | Invalid key format | Check public key file |
| 5 | Verification error | Check logs, retry |

## Output

### Valid Signature
```
VALID: Signature verification successful
```
Exit code: 0

### Invalid Signature
```
INVALID: Signature verification failed
```
Exit code: 1

## Client Registration Process

1. **Client generates key pair**:
   ```batch
   openssl genrsa -out client-private-key.pem 2048
   openssl rsa -in client-private-key.pem -pubout -out client-public-key.pem
   ```

2. **Client registers with server**:
   - Submit `client-public-key.pem` during registration
   - Server stores public key with client ID

3. **Server verifies requests**:
   - Look up client's public key by client ID
   - Use this tool to verify request signatures

## Security Considerations

- **Public Key Management**: Store public keys securely with client associations
- **Key Versioning**: Support key rotation with versioned storage
- **Monitoring**: Log verification attempts for security analysis
- **Performance**: Cache public keys for high-traffic scenarios

## Troubleshooting

### Common Issues

1. **"File not found"**
   - Check payload, signature, and key file paths
   - Ensure files exist and are accessible

2. **"Invalid key format"**
   - Verify public key is in PEM format
   - Check for proper BEGIN/END PUBLIC KEY markers

3. **"Invalid signature format"**
   - Ensure signature encoding matches `-encoding` parameter
   - Check for whitespace or formatting issues in signature

4. **Verification always fails**
   - Verify payload matches exactly what was signed
   - Check that public key corresponds to private key used for signing
   - Ensure same encoding is used for both signing and verification

### Getting Help

```batch
Verifier.exe -help
```

## Performance Tips

- **Key Caching**: Load and cache public keys instead of reading from file each time
- **Batch Processing**: Process multiple verifications in a single application instance
- **Async Operations**: Use asynchronous verification for high-throughput scenarios

## Technical Details

- **Algorithm**: RSA-2048
- **Hash Function**: SHA-256
- **Padding**: PKCS#1 v1.5
- **Key Format**: PEM (RFC 7468)
- **Input**: Base64 or Hexadecimal encoded signatures