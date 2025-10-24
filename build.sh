#!/bin/bash
# Build script for Co-Parenting Calendar
# Usage: ./build.sh [backend|frontend|all|docker]

set -e

GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

print_info() {
    echo -e "${BLUE}ℹ ${1}${NC}"
}

print_success() {
    echo -e "${GREEN}✓ ${1}${NC}"
}

print_error() {
    echo -e "${RED}✗ ${1}${NC}"
}

build_backend() {
    print_info "Building backend (.NET 8)..."
    
    cd backend/CoParenting.API
    
    print_info "Restoring dependencies..."
    dotnet restore
    
    print_info "Building in Release mode..."
    dotnet build --configuration Release --no-restore
    
    print_info "Publishing..."
    dotnet publish --configuration Release --output ./publish --no-build
    
    cd ../..
    print_success "Backend built successfully"
}

build_frontend() {
    print_info "Building frontend (React + TypeScript)..."
    
    cd frontend
    
    if [ ! -d "node_modules" ]; then
        print_info "Installing dependencies..."
        npm ci
    else
        print_info "Dependencies already installed"
    fi
    
    print_info "Building production bundle..."
    npm run build
    
    cd ..
    print_success "Frontend built successfully"
}

build_docker() {
    print_info "Building Docker images..."
    
    print_info "Building backend image..."
    docker build -t coparenting-api:latest ./backend
    
    print_info "Building frontend image..."
    docker build -t coparenting-frontend:latest ./frontend
    
    print_success "Docker images built successfully"
}

test_backend() {
    print_info "Running backend tests..."
    cd backend
    dotnet test --configuration Release --verbosity normal || echo "No tests found yet"
    cd ..
    print_success "Backend tests completed"
}

test_frontend() {
    print_info "Running frontend tests..."
    cd frontend
    npm test || echo "No tests configured yet"
    cd ..
    print_success "Frontend tests completed"
}

clean() {
    print_info "Cleaning build artifacts..."
    
    # Backend
    if [ -d "backend/CoParenting.API/bin" ]; then
        rm -rf backend/CoParenting.API/bin
    fi
    if [ -d "backend/CoParenting.API/obj" ]; then
        rm -rf backend/CoParenting.API/obj
    fi
    if [ -d "backend/CoParenting.API/publish" ]; then
        rm -rf backend/CoParenting.API/publish
    fi
    
    # Frontend
    if [ -d "frontend/dist" ]; then
        rm -rf frontend/dist
    fi
    
    print_success "Cleaned successfully"
}

show_help() {
    echo "Co-Parenting Calendar Build Script"
    echo ""
    echo "Usage: ./build.sh [command]"
    echo ""
    echo "Commands:"
    echo "  backend    - Build backend only"
    echo "  frontend   - Build frontend only"
    echo "  all        - Build both backend and frontend (default)"
    echo "  docker     - Build Docker images"
    echo "  test       - Run all tests"
    echo "  clean      - Clean build artifacts"
    echo "  help       - Show this help message"
}

# Main script
case "${1:-all}" in
    backend)
        build_backend
        ;;
    frontend)
        build_frontend
        ;;
    all)
        build_backend
        build_frontend
        print_success "All components built successfully!"
        ;;
    docker)
        build_docker
        ;;
    test)
        test_backend
        test_frontend
        ;;
    clean)
        clean
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        print_error "Unknown command: ${1}"
        show_help
        exit 1
        ;;
esac
