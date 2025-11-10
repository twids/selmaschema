# Local Development Guide

This guide provides detailed instructions for setting up and running the Co-Parenting Calendar application locally for development.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Development Workflows](#development-workflows)
4. [VS Code Setup](#vs-code-setup)
5. [Debugging](#debugging)
6. [Hot Reload Development](#hot-reload-development)
7. [Common Tasks](#common-tasks)
8. [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Software

- **Docker Desktop** (recommended): Simplest way to get started
  - [Download Docker Desktop](https://www.docker.com/products/docker-desktop)
  - Requires Docker Engine 20.10+ and Docker Compose V2 (Docker Desktop 3.0+)
  - Available for Windows, macOS, and Linux

**OR** for native development:

- **.NET 8 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 20.x LTS**: [Download here](https://nodejs.org/)
- **SQL Server 2022**: Or use Docker for just the database

### Recommended IDE

- **Visual Studio Code** with extensions (see [VS Code Setup](#vs-code-setup))

## Quick Start

### Option 1: Docker Compose (Recommended)

The easiest way to get started is using Docker Compose:

```bash
# Clone the repository
git clone <repository-url>
cd selmaschema

# Start all services (database, API, frontend)
docker compose up --build

# Access the application
# Frontend: http://localhost:3000
# API: http://localhost:8080
# Swagger: http://localhost:8080/swagger
```

### Option 2: Docker with Hot Reload (Development Mode)

For active development with automatic code reloading:

```bash
# Start services with development configuration
docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build

# Frontend with Vite hot reload: http://localhost:5173
# API with dotnet watch: http://localhost:8080
```

### Option 3: Native Development (No Docker)

Run services natively for maximum flexibility:

```bash
# Terminal 1: Start SQL Server (Docker)
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sql-server \
  mcr.microsoft.com/mssql/server:2022-latest

# Terminal 2: Start Backend API
cd backend/CoParenting.API
dotnet restore
dotnet watch run

# Terminal 3: Start Frontend
cd frontend
npm install
npm run dev
```

## Development Workflows

### Workflow 1: Full Docker Development

Best for: Consistency across team, production-like environment

```bash
# Build and start all services
docker compose up --build

# View logs
docker compose logs -f api        # Backend logs
docker compose logs -f frontend   # Frontend logs
docker compose logs -f db         # Database logs

# Rebuild after changes
docker compose up --build

# Stop services
docker compose down

# Clean restart (removes volumes)
docker compose down -v
docker compose up --build
```

### Workflow 2: Hot Reload Development

Best for: Active development, fast iteration

```bash
# Start with development configuration
docker compose -f docker-compose.yml -f docker-compose.dev.yml up

# Code changes automatically reload
# Backend: dotnet watch detects changes and restarts
# Frontend: Vite HMR updates browser instantly

# View specific service logs
docker compose -f docker-compose.yml -f docker-compose.dev.yml logs -f api
```

### Workflow 3: Hybrid Development

Best for: Debugging specific component, using native tools

```bash
# Start only database in Docker
docker compose up db

# Run backend natively (easier debugging)
cd backend/CoParenting.API
dotnet watch run

# Run frontend natively (faster HMR)
cd frontend
npm run dev
```

### Workflow 4: VS Code Integrated Development

Best for: Full IDE integration, debugging support

1. Open project in VS Code
2. Install recommended extensions (prompted automatically)
3. Press `F5` or use "Run and Debug" panel
4. Select launch configuration (see [Debugging](#debugging))

## VS Code Setup

### Recommended Extensions

When you open the project in VS Code, you'll be prompted to install recommended extensions:

**Backend Development:**
- C# Dev Kit
- C# for Visual Studio Code
- .NET Test Explorer

**Frontend Development:**
- ESLint
- Prettier - Code formatter
- TypeScript Vue Plugin

**General Development:**
- Docker
- GitLens
- Error Lens
- Code Spell Checker

### Workspace Settings

The project includes VS Code settings (`.vscode/settings.json`) that:
- Hide build artifacts from file explorer
- Configure auto-formatting on save
- Set up TypeScript and C# formatting
- Configure ESLint auto-fix

## Debugging

### Backend Debugging

#### Option 1: Debug in Container (F5)

1. Open VS Code
2. Press `F5`
3. Select "Launch Backend API (Docker)"
4. Set breakpoints in C# code
5. Debug as normal

#### Option 2: Debug Native Process

1. Press `F5`
2. Select "Debug Backend API (.NET)"
3. Backend starts with debugger attached
4. Set breakpoints and debug

**Debug URL:** http://localhost:8080

### Frontend Debugging

#### Debug in Browser

1. Start frontend: `npm run dev` (in frontend directory)
2. Press `F5` in VS Code
3. Select "Debug Frontend (Chrome)" or "Debug Frontend (Edge)"
4. Browser opens with debugger attached
5. Set breakpoints in TypeScript/React code

**Debug URL:** http://localhost:5173

### Full Stack Debugging

Debug both backend and frontend simultaneously:

1. Press `F5`
2. Select "Full Stack (Backend + Frontend)"
3. Both services start with debuggers attached
4. Set breakpoints in both backend and frontend code

### Attach to Running Container

If services are already running in Docker:

1. Start services: `docker compose up`
2. Press `F5`
3. Select "Attach to Backend Container"
4. Select the process from the list
5. Set breakpoints and debug

## Hot Reload Development

### Backend Hot Reload

Using `dotnet watch`, the backend automatically rebuilds and restarts when code changes:

**In Docker:**
```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml up api
```

**Native:**
```bash
cd backend/CoParenting.API
dotnet watch run
```

**What triggers reload:**
- C# source file changes (.cs)
- Configuration changes (appsettings.json)
- Project file changes (.csproj)

**Does NOT trigger reload:**
- Comments-only changes
- Whitespace changes

### Frontend Hot Reload

Using Vite HMR (Hot Module Replacement), the frontend updates instantly:

**In Docker:**
```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml up frontend
```

**Native:**
```bash
cd frontend
npm run dev
```

**Features:**
- Instant browser updates (no full reload)
- Preserves application state
- Shows build errors in browser
- Fast refresh for React components

## Common Tasks

### Build Commands

```bash
# Build everything
./build.sh all          # Linux/Mac
build.bat all          # Windows

# Build backend only
./build.sh backend
cd backend/CoParenting.API && dotnet build

# Build frontend only
./build.sh frontend
cd frontend && npm run build

# Build Docker images
./build.sh docker
docker compose build
```

### Testing

```bash
# Run backend tests
cd backend
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run frontend tests (if configured)
cd frontend
npm test
```

### Code Quality

```bash
# Lint frontend
cd frontend
npm run lint

# Auto-fix linting issues
npm run lint -- --fix

# Format code with Prettier
npm run format
```

### Database Management

```bash
# Connect to database
docker exec -it coparenting-db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd"

# View database logs
docker compose logs db

# Reset database (WARNING: deletes all data)
docker compose down -v
docker compose up db
```

### VS Code Tasks

Use the Command Palette (`Ctrl+Shift+P` or `Cmd+Shift+P`):

- **Tasks: Run Task** → Select from available tasks:
  - Build Backend
  - Build Frontend
  - Watch Backend
  - Start Frontend Dev Server
  - Docker Compose Up
  - Docker Compose Down
  - Docker Logs

## Troubleshooting

### Port Already in Use

**Problem:** Error message "port already in use" or "address already in use"

**Solution:**
```bash
# Find process using port (Linux/Mac)
lsof -i :8080
lsof -i :3000

# Find process using port (Windows)
netstat -ano | findstr :8080

# Kill the process or change ports in docker-compose.yml
```

### Docker Build Fails

**Problem:** Docker build fails with "no space left on device" or cache issues

**Solution:**
```bash
# Clean Docker system
docker system prune -a --volumes

# Build without cache
docker compose build --no-cache

# Check disk space
df -h    # Linux/Mac
```

### Backend Won't Start

**Problem:** Backend container crashes or won't start

**Solution:**
```bash
# Check logs
docker compose logs api

# Common issues:
# 1. Database not ready - wait 30 seconds
# 2. Port conflict - check port 8080
# 3. Connection string wrong - check docker-compose.yml

# Restart just the API
docker compose restart api
```

### Frontend Build Errors

**Problem:** Frontend fails to build or shows TypeScript errors

**Solution:**
```bash
cd frontend

# Clear node_modules and reinstall
rm -rf node_modules package-lock.json
npm install

# Clear Vite cache
rm -rf node_modules/.vite

# Check Node version (should be 20.x)
node --version
```

### Database Connection Issues

**Problem:** API can't connect to database

**Solution:**
```bash
# Check database is running
docker compose ps db

# Check database health
docker compose exec db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1"

# Ensure services start in correct order
docker compose down
docker compose up db    # Wait for healthy
docker compose up       # Start everything
```

### Hot Reload Not Working

**Problem:** Code changes don't trigger reload

**Backend:**
```bash
# Ensure using dotnet watch
docker compose -f docker-compose.yml -f docker-compose.dev.yml logs api

# Should see "dotnet watch" in logs
# Check volumes are mounted: docker compose config
```

**Frontend:**
```bash
# Ensure Vite dev server is running
curl http://localhost:5173

# Check browser console for HMR errors
# Try hard refresh: Ctrl+Shift+R
```

### VS Code Debugging Not Working

**Problem:** Breakpoints not hit or debugger won't attach

**Solution:**

1. **Verify debugger installation:**
   ```bash
   # For C# debugging, install C# Dev Kit extension
   # For JS/TS debugging, use Chrome or Edge DevTools
   ```

2. **Check launch configuration:**
   - Open `.vscode/launch.json`
   - Verify paths are correct
   - Check ports match running services

3. **Rebuild with debug symbols:**
   ```bash
   cd backend/CoParenting.API
   dotnet build --configuration Debug
   ```

### Environment Variables Not Loading

**Problem:** Configuration not loading correctly

**Solution:**
```bash
# Check .env files are not in .gitignore (they are)
# Use docker-compose environment or appsettings.json

# View current environment
docker compose config

# Override environment variables
docker compose -f docker-compose.yml \
  -f docker-compose.dev.yml \
  -f docker-compose.override.yml up
```

### Permission Denied (Linux/Mac)

**Problem:** Build scripts or Docker commands fail with permission denied

**Solution:**
```bash
# Make build scripts executable
chmod +x build.sh

# Add user to docker group (Linux)
sudo usermod -aG docker $USER
# Log out and back in

# Or run with sudo
sudo docker compose up
```

## Performance Tips

### Speed Up Docker Builds

1. **Use BuildKit:**
   ```bash
   export DOCKER_BUILDKIT=1
   docker compose build
   ```

2. **Use build cache:**
   ```bash
   # BuildKit caches between builds automatically
   # For CI/CD, use cache-from/cache-to
   ```

3. **Increase Docker resources:**
   - Open Docker Desktop settings
   - Increase Memory to 8GB+ (from 2GB default)
   - Increase CPUs to 4+ cores

### Speed Up Hot Reload

1. **Use volume mounts** (already configured in docker-compose.dev.yml)
2. **Exclude node_modules and bin/obj** (already configured)
3. **Use native development** for maximum speed

### Reduce Build Times

1. **Use npm ci instead of npm install** (deterministic, faster)
2. **Leverage multi-stage builds** (already implemented)
3. **Use .dockerignore** to exclude unnecessary files

## Additional Resources

- [.NET Hot Reload](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-watch)
- [Vite HMR](https://vitejs.dev/guide/features.html#hot-module-replacement)
- [Docker Compose](https://docs.docker.com/compose/)
- [VS Code Debugging](https://code.visualstudio.com/docs/editor/debugging)

## Getting Help

If you encounter issues not covered here:

1. Check the main [README.md](./README.md)
2. Check the [BUILD.md](./BUILD.md) guide
3. Review GitHub Actions logs for CI examples
4. Open an issue on GitHub with:
   - Command you ran
   - Full error message
   - Environment details (OS, Docker version, etc.)
   - Steps to reproduce
