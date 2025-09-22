@echo off
echo Generating RSA key pair for testing...
echo.

REM Check if OpenSSL is available
openssl version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: OpenSSL not found in PATH
    echo.
    echo Please install OpenSSL or ensure it's in your PATH.
    echo Windows: Download from https://slproweb.com/products/Win32OpenSSL.html
    echo Git Bash: Use "openssl" command in Git Bash terminal
    echo.
    pause
    exit /b 1
)

echo Step 1: Generating 2048-bit RSA private key...
openssl genrsa -out api-private-key.pem 2048
if %errorlevel% neq 0 (
    echo Error generating private key
    pause
    exit /b 1
)

echo Step 2: Extracting corresponding public key...
openssl rsa -in api-private-key.pem -pubout -out api-public-key.pem
if %errorlevel% neq 0 (
    echo Error extracting public key
    pause
    exit /b 1
)

echo Step 3: Verifying key pair...
openssl rsa -in api-private-key.pem -check -noout
if %errorlevel% neq 0 (
    echo Error verifying key pair
    pause
    exit /b 1
)

echo.
echo Key generation completed successfully!
echo.
echo Files created:
echo   api-private-key.pem  - RSA private key (keep secure!)
echo   api-public-key.pem   - RSA public key (can be shared)
echo.
echo Security reminder:
echo   Set restrictive permissions on the private key file
echo.
pause