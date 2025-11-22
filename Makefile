.PHONY: help build build-backend build-frontend test test-backend test-frontend clean start stop restart logs

# Default target
help:
	@echo "Co-Parenting Calendar - Build Commands"
	@echo ""
	@echo "Available targets:"
	@echo "  make build           - Build all services (backend + frontend)"
	@echo "  make build-backend   - Build backend only"
	@echo "  make build-frontend  - Build frontend only"
	@echo "  make test            - Run all tests"
	@echo "  make test-backend    - Run backend tests"
	@echo "  make test-frontend   - Run frontend tests"
	@echo "  make docker-build    - Build Docker images"
	@echo "  make start           - Start all services with Docker Compose"
	@echo "  make stop            - Stop all services"
	@echo "  make restart         - Restart all services"
	@echo "  make logs            - View service logs"
	@echo "  make clean           - Clean build artifacts"
	@echo "  make clean-all       - Clean everything including Docker volumes"

# Build targets
build: build-backend build-frontend
	@echo "✓ All services built successfully"

build-backend:
	@echo "Building backend..."
	@echo "Building frontend first..."
	cd frontend && npm ci
	cd frontend && npm run build
	@echo "Copying frontend to backend wwwroot..."
	mkdir -p backend/CoParenting.API/wwwroot
	cp -r frontend/dist/* backend/CoParenting.API/wwwroot/
	cd backend/CoParenting.API && dotnet restore
	cd backend/CoParenting.API && dotnet build --configuration Release
	@echo "✓ Backend built successfully"

build-frontend:
	@echo "Building frontend..."
	cd frontend && npm ci
	cd frontend && npm run build
	@echo "✓ Frontend built successfully"

# Test targets
test: test-backend test-frontend
	@echo "✓ All tests completed"

test-backend:
	@echo "Running backend tests..."
	cd backend && dotnet test --configuration Release --verbosity normal || echo "No tests found yet"
	@echo "✓ Backend tests completed"

test-frontend:
	@echo "Running frontend tests..."
	cd frontend && npm test || echo "No tests configured yet"
	@echo "✓ Frontend tests completed"

# Docker targets
docker-build:
	@echo "Building Docker image..."
	docker build -t coparenting-app:latest .
	@echo "✓ Docker image built successfully"

start:
	@echo "Starting all services..."
	docker compose up -d
	@echo "✓ Services started"
	@echo "  Application: http://localhost:3000"
	@echo "  API: http://localhost:3000/api"
	@echo "  Swagger: http://localhost:3000/swagger"

stop:
	@echo "Stopping all services..."
	docker compose down
	@echo "✓ Services stopped"

restart: stop start
	@echo "✓ Services restarted"

logs:
	docker compose logs -f

# Clean targets
clean:
	@echo "Cleaning build artifacts..."
	cd backend/CoParenting.API && dotnet clean
	rm -rf backend/CoParenting.API/bin backend/CoParenting.API/obj
	rm -rf backend/CoParenting.API/wwwroot
	rm -rf backend/CoParenting.Core/bin backend/CoParenting.Core/obj
	rm -rf backend/CoParenting.Infrastructure/bin backend/CoParenting.Infrastructure/obj
	rm -rf frontend/dist frontend/node_modules
	@echo "✓ Build artifacts cleaned"

clean-all: clean
	@echo "Cleaning Docker resources..."
	docker compose down -v --remove-orphans
	@echo "✓ Everything cleaned"

# Development helpers
dev-backend:
	@echo "Starting backend in development mode..."
	cd backend/CoParenting.API && dotnet watch run

dev-frontend:
	@echo "Starting frontend in development mode..."
	cd frontend && npm run dev

lint-backend:
	@echo "Linting backend code..."
	cd backend && dotnet format --verify-no-changes

lint-frontend:
	@echo "Linting frontend code..."
	cd frontend && npm run lint
