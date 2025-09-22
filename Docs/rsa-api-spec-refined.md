# RSA API Signing Console Applications Specification

## Overview

Two standalone console applications implementing industry-standard RSA signature operations for REST API authentication:

- **Signer.exe**: Client-side tool that signs API requests using a private key
- **Verifier.exe**: Server-side tool that verifies API signatures using the corresponding public key

This follows the common pattern where the **API client holds the private key** and **API server holds the public key**.

## RSA Key Pair Generation with OpenSSL

### Prerequisites

**Install OpenSSL**:
- **Windows**: Download from https://slproweb.com/products/Win32OpenSSL.html
- **macOS**: `brew install openssl`
- **Linux**: Usually pre-installed, or `sudo apt-get install openssl`
- **Git Bash**: Includes OpenSSL on Windows

### Key Generation Process

```bash
# Step 1: Generate 2048-bit RSA private key
openssl genrsa -out api-private-key.pem 2048

# Step 2: Extract the corresponding public key
openssl rsa -in api-private-key.pem -pubout -out api-public-key.pem

# Step 3: Verify the key pair
openssl rsa -in api-private-key.pem -check -noout
```

### Key Files Explained

| File | Format | Contains | Used By | Purpose |
|------|--------|----------|---------|---------|
| `api-private-key.pem` | PEM | Private key | **API Client** (Signer.exe) | Signs outgoing requests |
| `api-public-key.pem` | PEM | Public key | **API Server** (Verifier.exe) | Verifies incoming signatures |

### Key Distribution in Practice

1. **Private Key** (`api-private-key.pem`):
   - Stays with the API client application
   - **Never transmitted** over the network
   - Stored securely on client systems
   - Used to sign requests before sending

2. **Public Key** (`api-public-key.pem`):
   - Shared with the API server during client registration
   - Can be transmitted safely (not secret)
   - Stored in server's client registry/database
   - Used to verify signatures from this specific client

### Security Best Practices

```bash
# Set restrictive permissions on private key (Unix/Linux/macOS)
chmod 600 api-private-key.pem

# Windows: Right-click → Properties → Security → Remove inheritance, grant only current user
```

## Signer.exe Specification (Client-Side)

### Purpose
Signs API request payloads using the client's private key. This proves to the server that the request came from the legitimate client.

### Command Line Interface

```bash
# Sign a JSON API request payload
Signer.exe -file request.json -key api-private-key.pem

# Sign string data directly
Signer.exe -payload "POST /api/data HTTP/1.1\nContent-Type: application/json\n{\"data\":\"value\"}" -key api-private-key.pem

# Output signature to file for API transmission
Signer.exe -file request.json -key api-private-key.pem -output signature.b64
```

### Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `-payload` | Yes* | String data to sign (mutually exclusive with -file) |
| `-file` | Yes* | Path to file containing payload to sign |
| `-key` | Yes | Path to private key file (PEM format) |
| `-output` | No | Output file path (default: console output) |
| `-encoding` | No | Output encoding: `base64` (default) or `hex` |

### Typical API Integration

```bash
# 1. Client creates request payload
echo '{"userId": 123, "action": "transfer", "amount": 500}' > request.json

# 2. Client signs the payload
Signer.exe -file request.json -key api-private-key.pem > signature.b64

# 3. Client sends both payload and signature to API server
# Headers: X-Signature: <content of signature.b64>
# Body: <content of request.json>
```

### Implementation (Signer.cs)

```csharp
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Signer
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var options = ParseArguments(args);
                
                // Load payload (what we're signing)
                string payload = options.IsFile 
                    ? File.ReadAllText(options.PayloadPath)
                    : options.PayloadPath;
                
                // Load private key (must be PEM format)
                string privateKeyPem = File.ReadAllText(options.KeyPath);
                
                // Sign the payload
                string signature = SignPayload(payload, privateKeyPem);
                
                // Output signature
                if (!string.IsNullOrEmpty(options.OutputPath))
                {
                    File.WriteAllText(options.OutputPath, signature);
                    Console.WriteLine($"Signature written to: {options.OutputPath}");
                }
                else
                {
                    Console.WriteLine(signature);
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return GetErrorCode(ex);
            }
        }
        
        static string SignPayload(string payload, string privateKeyPem)
        {
            using (var rsa = RSA.Create())
            {
                // Import PEM private key
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(
                    ExtractKeyFromPem(privateKeyPem, "PRIVATE KEY")), 
                    out _);
                
                // Sign payload with SHA256
                byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
                byte[] signature = rsa.SignData(
                    payloadBytes, 
                    HashAlgorithmName.SHA256, 
                    RSASignaturePadding.Pkcs1);
                
                return Convert.ToBase64String(signature);
            }
        }
        
        static string ExtractKeyFromPem(string pem, string keyType)
        {
            string header = $"-----BEGIN {keyType}-----";
            string footer = $"-----END {keyType}-----";
            
            int start = pem.IndexOf(header) + header.Length;
            int end = pem.IndexOf(footer);
            
            return pem.Substring(start, end - start)
                      .Replace("\n", "")
                      .Replace("\r", "")
                      .Replace(" ", "");
        }
        
        // Additional helper methods for argument parsing and error codes...
    }
}
```

## Verifier.exe Specification (Server-Side)

### Purpose
Verifies that an API request signature was created by the holder of the corresponding private key. This authenticates the client to the server.

### Command Line Interface

```bash
# Verify an API request and its signature
Verifier.exe -file request.json -signature "base64signature..." -key api-public-key.pem

# Verify with signature from file
Verifier.exe -file request.json -sigfile signature.b64 -key api-public-key.pem

# Verify string payload directly
Verifier.exe -payload "request data" -signature "base64sig..." -key api-public-key.pem
```

### Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `-payload` | Yes* | String data to verify (mutually exclusive with -file) |
| `-file` | Yes* | Path to file containing payload |
| `-signature` | Yes* | Signature string (mutually exclusive with -sigfile) |
| `-sigfile` | Yes* | Path to file containing signature |
| `-key` | Yes | Path to public key file (PEM format) |
| `-encoding` | No | Signature encoding: `base64` (default) or `hex` |

### Typical API Server Integration

```bash
# Server receives request with signature header
# 1. Save request payload to temp file
echo "$REQUEST_BODY" > temp_payload.json

# 2. Extract signature from X-Signature header
echo "$SIGNATURE_HEADER" > temp_signature.b64

# 3. Verify signature against client's public key
Verifier.exe -file temp_payload.json -sigfile temp_signature.b64 -key client123-public-key.pem

# Exit code 0 = valid signature, proceed with request
# Exit code 1 = invalid signature, reject request
```

### Output and Exit Codes

- **Valid Signature** (Exit Code 0):
  ```
  VALID: Signature verification successful
  ```
- **Invalid Signature** (Exit Code 1):
  ```
  INVALID: Signature verification failed
  ```
- **Error Codes**:
  - 2: Invalid arguments
  - 3: File not found
  - 4: Invalid key format
  - 5: Verification error

### Implementation (Verifier.cs)

```csharp
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Verifier
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var options = ParseArguments(args);
                
                // Load payload (what was signed)
                string payload = options.IsFile 
                    ? File.ReadAllText(options.PayloadPath)
                    : options.PayloadPath;
                
                // Load signature
                string signatureText = options.IsSigFile
                    ? File.ReadAllText(options.SignaturePath).Trim()
                    : options.SignaturePath;
                
                // Load public key (must be PEM format)
                string publicKeyPem = File.ReadAllText(options.KeyPath);
                
                // Verify signature
                bool isValid = VerifySignature(payload, signatureText, publicKeyPem);
                
                if (isValid)
                {
                    Console.WriteLine("VALID: Signature verification successful");
                    return 0;
                }
                else
                {
                    Console.WriteLine("INVALID: Signature verification failed");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return GetErrorCode(ex);
            }
        }
        
        static bool VerifySignature(string payload, string signature, string publicKeyPem)
        {
            using (var rsa = RSA.Create())
            {
                // Import PEM public key
                rsa.ImportRSAPublicKey(Convert.FromBase64String(
                    ExtractKeyFromPem(publicKeyPem, "PUBLIC KEY")), 
                    out _);
                
                // Verify signature
                byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
                byte[] signatureBytes = Convert.FromBase64String(signature);
                
                return rsa.VerifyData(
                    payloadBytes, 
                    signatureBytes, 
                    HashAlgorithmName.SHA256, 
                    RSASignaturePadding.Pkcs1);
            }
        }
        
        static string ExtractKeyFromPem(string pem, string keyType)
        {
            string header = $"-----BEGIN {keyType}-----";
            string footer = $"-----END {keyType}-----";
            
            int start = pem.IndexOf(header) + header.Length;
            int end = pem.IndexOf(footer);
            
            return pem.Substring(start, end - start)
                      .Replace("\n", "")
                      .Replace("\r", "")
                      .Replace(" ", "");
        }
        
        // Additional helper methods...
    }
}
```

## Real-World API Workflow

### Client Registration Process

1. **Client generates key pair**:
   ```bash
   openssl genrsa -out client-private-key.pem 2048
   openssl rsa -in client-private-key.pem -pubout -out client-public-key.pem
   ```

2. **Client registers with API server**:
   - Submits `client-public-key.pem` during registration
   - Server stores public key associated with client ID
   - Client keeps `client-private-key.pem` secret

### API Request Flow

1. **Client creates request**:
   ```bash
   # Prepare API request payload
   echo '{"transfer": {"from": "A", "to": "B", "amount": 100}}' > request.json
   ```

2. **Client signs request**:
   ```bash
   # Sign with private key
   Signer.exe -file request.json -key client-private-key.pem > signature.b64
   ```

3. **Client sends HTTP request**:
   ```http
   POST /api/transfer HTTP/1.1
   Host: api.example.com
   X-Client-ID: client123
   X-Signature: <base64_signature_from_file>
   Content-Type: application/json
   
   {"transfer": {"from": "A", "to": "B", "amount": 100}}
   ```

4. **Server verifies request**:
   ```bash
   # Server looks up client123's public key and verifies
   Verifier.exe -file request.json -signature "$X_SIGNATURE" -key client123-public-key.pem
   ```

5. **Server processes or rejects** based on verification result

## Key Management Best Practices

### For API Clients (Private Key Holders)
- Store private keys in secure key stores (Windows Certificate Store, macOS Keychain, etc.)
- Never log or transmit private keys
- Use environment variables or secure configuration for key file paths
- Implement key rotation policies

### For API Servers (Public Key Holders)
- Store public keys in database with client associations
- Implement key versioning for rotation support
- Log verification attempts for security monitoring
- Cache public keys for performance

### Standard Compliance
- Uses **RSA-2048** minimum key size (industry standard)
- Uses **SHA-256** hashing (recommended by NIST)
- Uses **PKCS#1 v1.5** padding (widely supported)
- Follows **PEM format** (RFC 7468 standard)
- Compatible with standard OpenSSL toolchain