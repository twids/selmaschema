# Build Guide - Co-Parenting Calendar

This document provides comprehensive build instructions for the Co-Parenting Calendar application.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Quick Build](#quick-build)
3. [Detailed Build Steps](#detailed-build-steps)
4. [Build Automation](#build-automation)
5. [CI/CD Pipeline](#cicd-pipeline)
6. [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Software

**Backend:**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2022 or Docker

**Frontend:**
- [Node.js 20.x](https://nodejs.org/) (LTS recommended)
- npm 9+ (comes with Node.js)

**Docker (Optional but Recommended):**
- [Docker Desktop](https://www.docker.com/products/docker-desktop) 20+
- Docker Compose 2+ (included with Docker Desktop)

### System Requirements

- **OS**: Windows 10/11, macOS 10.15+, or Linux (Ubuntu 20.04+)
- **RAM**: 8GB minimum, 16GB recommended
- **Disk Space**: 10GB free space
- **Ports**: 1433, 8080, 3000, 5173 must be available

## Quick Build

### Option 1: Using Build Scripts (Recommended)

**Linux/macOS:**
```bash
chmod +x build.sh
./build.sh all
```

**Windows:**
```cmd
build.bat all
```

### Option 2: Using Make (Linux/macOS)

```bash
make build
```

### Option 3: Using Docker Compose

```bash
docker-compose up --build
```

## Detailed Build Steps

### Backend Build

#### Step 1: Navigate to Backend

```bash
cd backend/CoParenting.API
```

#### Step 2: Restore Dependencies

```bash
dotnet restore
```

This downloads all NuGet packages defined in the .csproj files.

**Expected output:**
```
Determining projects to restore...
Restored /path/to/CoParenting.API.csproj (in XXX ms).
Restored /path/to/CoParenting.Core.csproj (in XXX ms).
Restored /path/to/CoParenting.Infrastructure.csproj (in XXX ms).
```

#### Step 3: Build the Project

```bash
dotnet build --configuration Release --no-restore
```

**Build configurations:**
- `Debug` - Includes debugging symbols, no optimizations
- `Release` - Optimized for production, smaller size

**Expected output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

#### Step 4: Run Tests (Optional)

```bash
cd ..
dotnet test --configuration Release
```

#### Step 5: Publish for Deployment

```bash
cd CoParenting.API
dotnet publish --configuration Release --output ./publish
```

This creates a self-contained deployment in the `publish/` folder.

**Output location:** `backend/CoParenting.API/publish/`

#### Step 6: Run the API

```bash
dotnet ./publish/CoParenting.API.dll
```

**Or during development:**
```bash
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:8080
```

**Access:**
- API: http://localhost:8080
- Swagger: http://localhost:8080/swagger

### Frontend Build

#### Step 1: Navigate to Frontend

```bash
cd frontend
```

#### Step 2: Install Dependencies

```bash
npm ci
```

Use `npm ci` for clean installs (recommended for builds) or `npm install` for development.

**Expected output:**
```
added XXX packages in XXs
```

**Dependencies installed:**
- React 18
- TypeScript 5
- Vite 5
- ESLint and plugins

#### Step 3: Lint the Code (Optional)

```bash
npm run lint
```

This checks for code quality issues.

#### Step 4: Build for Production

```bash
npm run build
```

This compiles TypeScript and bundles the application.

**Build process:**
1. TypeScript compilation (`tsc`)
2. Vite bundling and optimization
3. Asset optimization and minification

**Expected output:**
```
vite v5.x.x building for production...
✓ XXX modules transformed.
dist/index.html                   0.XX kB
dist/assets/index-XXXXXXXX.css   XX.XX kB │ gzip: X.XX kB
dist/assets/index-XXXXXXXX.js   XXX.XX kB │ gzip: XX.XX kB
✓ built in XXXms
```

**Output location:** `frontend/dist/`

#### Step 5: Preview Production Build (Optional)

```bash
npm run preview
```

**Access:** http://localhost:4173

#### Step 6: Run Development Server

```bash
npm run dev
```

**Access:** http://localhost:5173

**Features:**
- Hot Module Replacement (HMR)
- Fast refresh
- Source maps for debugging

### Docker Build

#### Build All Services

```bash
docker-compose build
```

#### Build Individual Services

```bash
# Backend only
docker-compose build api

# Frontend only
docker-compose build frontend

# Database only (pulls image)
docker-compose pull db
```

#### Build with No Cache

```bash
docker-compose build --no-cache
```

#### Build and Start

```bash
docker-compose up --build -d
```

**Flags:**
- `--build` - Build images before starting
- `-d` - Detached mode (run in background)

## Build Automation

### Build Scripts

The project includes cross-platform build scripts:

#### build.sh (Linux/macOS)

```bash
./build.sh [command]
```

**Commands:**
- `backend` - Build backend only
- `frontend` - Build frontend only
- `all` - Build everything (default)
- `docker` - Build Docker images
- `test` - Run all tests
- `clean` - Remove build artifacts

**Features:**
- Color-coded output
- Error handling
- Dependency checking
- Progress indicators

#### build.bat (Windows)

```cmd
build.bat [command]
```

Same commands as build.sh but for Windows.

### Makefile

```bash
make [target]
```

**Common targets:**
- `make build` - Build all services
- `make test` - Run tests
- `make docker-build` - Build Docker images
- `make start` - Start with Docker Compose
- `make stop` - Stop services
- `make clean` - Clean build artifacts
- `make help` - Show all targets

**Advanced targets:**
- `make dev-backend` - Start backend with hot reload
- `make dev-frontend` - Start frontend with hot reload
- `make lint-backend` - Lint backend code
- `make lint-frontend` - Lint frontend code

## CI/CD Pipeline

### GitHub Actions Workflow

**File:** `.github/workflows/ci-cd.yml`

**Triggered by:**
- Push to `main`, `develop`, or `copilot/**` branches
- Pull requests to `main` or `develop`

**Jobs:**

1. **backend-build**
   - Setup .NET 8
   - Restore dependencies
   - Build in Release mode
   - Run tests
   - Publish artifacts

2. **frontend-build**
   - Setup Node.js 20
   - Install dependencies (npm ci)
   - Lint code
   - Build production bundle
   - Upload artifacts

3. **docker-build**
   - Setup Docker Buildx
   - Build backend image
   - Build frontend image
   - Validate docker-compose

4. **integration-test**
   - Start services with docker-compose
   - Check health
   - Run integration tests
   - Cleanup

**Artifacts:**
- `backend-build` - Published .NET application (7 days retention)
- `frontend-build` - Built React application (7 days retention)

**Viewing workflow runs:**
1. Go to repository on GitHub
2. Click "Actions" tab
3. Select workflow run to view details

### Local CI Simulation

Test the CI pipeline locally:

```bash
# Install act (GitHub Actions local runner)
# https://github.com/nektos/act

# Run all jobs
act

# Run specific job
act -j backend-build
act -j frontend-build
```

## Troubleshooting

### Common Build Errors

#### Backend: "SDK not found"

**Error:**
```
The specified SDK 'Microsoft.NET.Sdk.Web' was not found
```

**Solution:**
```bash
# Check .NET version
dotnet --version

# Should be 8.0.x - if not, install .NET 8 SDK
```

#### Backend: "Package restore failed"

**Error:**
```
error NU1301: Unable to load the service index
```

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Try restore again
dotnet restore --verbosity detailed
```

#### Frontend: "Module not found"

**Error:**
```
Error: Cannot find module 'react'
```

**Solution:**
```bash
# Remove and reinstall dependencies
rm -rf node_modules package-lock.json
npm install
```

#### Frontend: "TypeScript errors"

**Error:**
```
error TS2307: Cannot find module './App'
```

**Solution:**
```bash
# Check TypeScript version
npm list typescript

# Reinstall dependencies
npm ci

# Clear cache
npm cache clean --force
```

#### Docker: "Build failed - no space"

**Error:**
```
ERROR: failed to solve: write /var/lib/docker/tmp/...: no space left on device
```

**Solution:**
```bash
# Clean Docker system
docker system prune -a --volumes

# Check disk space
df -h
```

#### Docker: "Port already in use"

**Error:**
```
Error: bind: address already in use
```

**Solution:**
```bash
# Find process using port (Linux/Mac)
lsof -i :8080

# Find process using port (Windows)
netstat -ano | findstr :8080

# Kill the process or change port in docker-compose.yml
```

### Performance Issues

**Slow builds:**
1. Use Docker BuildKit: `DOCKER_BUILDKIT=1 docker-compose build`
2. Increase Docker memory allocation (Docker Desktop settings)
3. Use `npm ci` instead of `npm install`
4. Enable NuGet package caching

**Large Docker images:**
1. Use multi-stage builds (already implemented)
2. Add `.dockerignore` files
3. Clean up after each RUN command
4. Use Alpine Linux base images where possible

### Getting Help

If you encounter issues:

1. Check this build guide
2. Check the main README.md
3. Review GitHub Actions logs for CI failures
4. Open an issue on GitHub with:
   - Build command used
   - Full error message
   - Environment details (OS, versions)
   - Steps to reproduce

## Advanced Topics

### Custom Build Configurations

**Backend:**
```bash
# Debug build
dotnet build --configuration Debug

# Specific framework
dotnet build --framework net8.0

# Specific runtime
dotnet publish -r linux-x64 --self-contained
```

**Frontend:**
```bash
# Development build
npm run dev

# Production build with source maps
npm run build -- --sourcemap

# Analyze bundle size
npm run build -- --mode=analyze
```

### Build Optimization

**Backend:**
- Enable ReadyToRun: Add `<PublishReadyToRun>true</PublishReadyToRun>` to .csproj
- Trim unused code: Add `<PublishTrimmed>true</PublishTrimmed>`
- Use native AOT compilation (experimental)

**Frontend:**
- Code splitting (already configured in Vite)
- Tree shaking (automatic)
- Asset optimization (automatic)
- Lazy loading routes

### Environment-Specific Builds

**Backend:**
```bash
# Development
dotnet run --environment Development

# Staging
dotnet run --environment Staging

# Production
dotnet run --environment Production
```

**Frontend:**
```bash
# Development
npm run dev

# Production preview
npm run preview

# Custom environment
VITE_API_URL=https://api.example.com npm run build
```

## Build Checklist

Before deploying:

- [ ] Backend builds without errors
- [ ] Frontend builds without errors
- [ ] All tests pass
- [ ] No linting errors
- [ ] Docker images build successfully
- [ ] Docker Compose starts all services
- [ ] Health checks pass
- [ ] Database schema is up to date
- [ ] Environment variables are configured
- [ ] Secrets are not in code
- [ ] Build artifacts are under 100MB
- [ ] Documentation is updated

## Support

For build-related issues:
- Check GitHub Actions logs
- Review this guide
- Open an issue on GitHub
- Contact maintainers
