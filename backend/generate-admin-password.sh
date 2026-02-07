#!/bin/bash

# Script to generate BCrypt hash for admin password
# Usage: ./generate-admin-password.sh <password>

if [ -z "$1" ]; then
    echo "Usage: ./generate-admin-password.sh <password>"
    echo "Example: ./generate-admin-password.sh MySecurePassword123"
    exit 1
fi

# Use Python to generate BCrypt hash (if bcrypt is installed)
if command -v python3 &> /dev/null; then
    python3 -c "import bcrypt; print(bcrypt.hashpw('$1'.encode(), bcrypt.gensalt()).decode())"
else
    echo "Python3 is required to generate password hash"
    echo "Install bcrypt: pip3 install bcrypt"
    exit 1
fi

echo ""
echo "Add this hash to appsettings.Development.json under Auth:AdminPasswordHash"
