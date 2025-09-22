# Integration Examples

This document provides examples of how to integrate the RSA Signer and Verifier tools into your API workflow.

## Basic Workflow

### 1. Generate RSA Key Pair

```batch
cd TestKeys
generate-keys.bat
```

This creates:
- `api-private-key.pem` - Keep secure, used by client
- `api-public-key.pem` - Share with server, used for verification

### 2. Client Signs Request

```batch
# Sign a JSON file
Signer.exe -file Examples\sample-request.json -key TestKeys\api-private-key.pem

# Sign string data directly
Signer.exe -payload "POST /api/transfer HTTP/1.1" -key TestKeys\api-private-key.pem

# Save signature to file
Signer.exe -file Examples\sample-request.json -key TestKeys\api-private-key.pem -output signature.txt
```

### 3. Server Verifies Signature

```batch
# Verify with signature string
Verifier.exe -file Examples\sample-request.json -signature "base64_signature_here" -key TestKeys\api-public-key.pem

# Verify with signature file
Verifier.exe -file Examples\sample-request.json -sigfile signature.txt -key TestKeys\api-public-key.pem

# Verify string payload
Verifier.exe -payload "POST /api/transfer HTTP/1.1" -signature "signature" -key TestKeys\api-public-key.pem
```

## HTTP API Integration

### Client Side (Sending Request)

```batch
# 1. Create request payload
echo {"userId": 123, "action": "transfer", "amount": 500} > request.json

# 2. Sign the payload
Signer.exe -file request.json -key private-key.pem > signature.b64

# 3. Send HTTP request with signature header
# curl -X POST https://api.example.com/transfer \
#      -H "Content-Type: application/json" \
#      -H "X-Signature: $(cat signature.b64)" \
#      -d @request.json
```

### Server Side (Receiving Request)

```batch
# 1. Save request body to temp file
# (This would be done by your web server/application)

# 2. Extract signature from header
# set SIGNATURE=%X_SIGNATURE_HEADER%

# 3. Verify signature
Verifier.exe -file temp_request.json -signature "%SIGNATURE%" -key client-public-key.pem

# 4. Check exit code
# if %errorlevel% equ 0 (
#     echo Request authenticated - process it
# ) else (
#     echo Invalid signature - reject request
# )
```

## Error Handling

### Signer Exit Codes
- 0: Success
- 2: Invalid arguments
- 3: File not found
- 4: Invalid key format
- 5: General error

### Verifier Exit Codes
- 0: Valid signature (authenticate request)
- 1: Invalid signature (reject request)
- 2: Invalid arguments
- 3: File not found
- 4: Invalid key format
- 5: Verification error

## Security Best Practices

### Key Management
- **Private keys**: Store securely, never transmit
- **Public keys**: Can be shared, store in client registry
- **Key rotation**: Implement regular key updates

### File Permissions
```batch
# Windows: Set private key permissions
# Right-click api-private-key.pem → Properties → Security
# Remove inheritance, grant access only to current user
```

### Production Considerations
- Use environment variables for key file paths
- Implement logging for verification attempts
- Cache public keys for performance
- Monitor for signature verification failures