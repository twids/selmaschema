#!/bin/bash

# Generate a BCrypt hash for the separate break-glass administrator.
# The password is read without echo and is never placed in the process arguments.

# Use Python to generate BCrypt hash (if bcrypt is installed)
if command -v python3 &> /dev/null; then
    IFS= read -r -s -p "Break-glass password: " break_glass_password
    echo ""
    printf '%s' "$break_glass_password" | python3 -c 'import bcrypt, sys; print(bcrypt.hashpw(sys.stdin.buffer.read(), bcrypt.gensalt(rounds=12)).decode())'
    unset break_glass_password
else
    echo "Python3 is required to generate password hash"
    echo "Install bcrypt: pip3 install bcrypt"
    exit 1
fi

echo ""
echo "Store this as BREAK_GLASS_ADMIN_PASSWORD_HASH (Auth:BreakGlass:PasswordHash)."
