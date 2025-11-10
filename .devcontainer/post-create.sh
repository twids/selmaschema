#!/bin/bash

# Post-create script for Co-Parenting Calendar DevContainer
# This script runs after the container is created to set up the development environment

set -e

echo "=========================================="
echo "Running DevContainer Post-Create Setup..."
echo "=========================================="

# Color codes for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored messages
print_status() {
    echo -e "${BLUE}==>${NC} $1"
}

print_success() {
    echo -e "${GREEN}✓${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}!${NC} $1"
}

# Check if we're in the right directory
if [ ! -f "docker-compose.yml" ]; then
    print_warning "docker-compose.yml not found, changing to /workspace"
    cd /workspace || exit 1
fi

# Restore .NET dependencies
print_status "Restoring .NET dependencies..."
if cd backend/CoParenting.API 2>/dev/null; then
    dotnet restore
    cd ../..
    print_success ".NET dependencies restored"
else
    print_warning "Backend directory not found, skipping .NET restore"
fi

# Install frontend dependencies
print_status "Installing frontend dependencies..."
if cd frontend 2>/dev/null; then
    # Check if pnpm is available
    if command -v pnpm &> /dev/null; then
        print_status "Using pnpm for package installation..."
        pnpm install
        print_success "Frontend dependencies installed with pnpm"
    else
        print_warning "pnpm not found, using npm..."
        npm ci
        print_success "Frontend dependencies installed with npm"
    fi
    cd ..
else
    print_warning "Frontend directory not found, skipping npm install"
fi

# Configure git
print_status "Configuring git..."
git config --global --add safe.directory /workspace
git config --global core.autocrlf input
print_success "Git configured"

# Display useful information
echo ""
echo "=========================================="
print_success "DevContainer setup complete!"
echo "=========================================="
echo ""
echo "Available commands:"
echo "  Backend:"
echo "    cd backend/CoParenting.API && dotnet run"
echo "    cd backend && dotnet test"
echo ""
echo "  Frontend:"
echo "    cd frontend && npm run dev (Vite dev server on port 5173)"
echo "    cd frontend && npm run build"
echo "    cd frontend && npm run lint"
echo ""
echo "  Docker:"
echo "    docker-compose up --build (Start all services)"
echo "    docker-compose down (Stop all services)"
echo ""
echo "  Build Scripts:"
echo "    ./build.sh all (Build everything)"
echo "    ./build.sh test (Run tests)"
echo "    make help (Show all make targets)"
echo ""
echo "Available ports:"
echo "  - 3000: Frontend (production)"
echo "  - 5173: Vite dev server"
echo "  - 8080: Backend API"
echo "  - 1433: SQL Server"
echo ""
print_success "Happy coding!"
echo "=========================================="
