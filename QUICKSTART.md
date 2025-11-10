# Quick Start Guide

Get the Co-Parenting Calendar running in under 5 minutes!

## For Developers New to the Project

### Step 1: Prerequisites ✓

**Install Docker Desktop** (easiest option):
- **Windows/Mac**: [Download Docker Desktop](https://www.docker.com/products/docker-desktop)
- **Linux**: Install Docker Engine and Docker Compose

**Or install development tools natively**:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20 LTS](https://nodejs.org/)
- SQL Server (or use Docker for just the database)

### Step 2: Clone the Repository

```bash
git clone https://github.com/twids/selmaschema.git
cd selmaschema
```

### Step 3: Start the Application

**Option A: Production Mode (Docker)**
```bash
# Build and start all services
docker compose up --build

# Wait 30 seconds for everything to start
# Then open http://localhost:3000 in your browser
```

**Option B: Development Mode (Docker with Hot Reload)**
```bash
# Start with hot reload enabled
docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build

# Frontend: http://localhost:5173 (Vite HMR)
# Backend: http://localhost:8080 (dotnet watch)
# Code changes reload automatically!
```

**Option C: Native Development (No Docker)**
```bash
# Terminal 1: Start database
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sql-server \
  mcr.microsoft.com/mssql/server:2022-latest

# Terminal 2: Start backend
cd backend/CoParenting.API
dotnet restore
dotnet run

# Terminal 3: Start frontend
cd frontend
npm install
npm run dev
```

### Step 4: Access the Application

- **Frontend**: http://localhost:3000 (production) or http://localhost:5173 (dev)
- **API**: http://localhost:8080
- **API Documentation**: http://localhost:8080/swagger

## For VS Code Users

### Quick Debug Setup

1. **Open project in VS Code**
   ```bash
   code .
   ```

2. **Install recommended extensions** (prompted automatically)

3. **Start debugging** by pressing `F5` and choosing:
   - "Full Stack (Backend + Frontend)" - Debug everything
   - "Debug Backend API (.NET)" - Backend only
   - "Debug Frontend (Chrome)" - Frontend only

### Quick Tasks

Open Command Palette (`Ctrl+Shift+P` or `Cmd+Shift+P`) and type "Tasks: Run Task":
- **Build Backend** - Compile C# code
- **Watch Backend** - Auto-rebuild on changes
- **Start Frontend Dev Server** - Start with HMR
- **Docker Compose Up** - Start all services
- **Docker Compose Down** - Stop all services

## Common Issues

### "Port already in use"
```bash
# Stop other services using these ports
docker compose down

# Or check what's using the port
lsof -i :8080     # Mac/Linux
netstat -ano | findstr :8080    # Windows
```

### "Database connection failed"
```bash
# Wait 30 seconds for SQL Server to fully start
# Check database is healthy
docker compose ps db
```

### "Docker build failed"
```bash
# Clean Docker cache and rebuild
docker compose down -v
docker system prune -f
docker compose up --build
```

### "Frontend won't start"
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
npm run dev
```

## Next Steps

Once you're up and running:

1. **Read the comprehensive guides**:
   - [DEVELOPMENT.md](./DEVELOPMENT.md) - Detailed development workflows
   - [README.md](./README.md) - Full project documentation
   - [BUILD.md](./BUILD.md) - Build instructions

2. **Explore the API**:
   - Open http://localhost:8080/swagger
   - Try the API endpoints
   - Review the API documentation

3. **Make your first change**:
   - Start with development mode (hot reload)
   - Edit a file and see changes instantly
   - Set breakpoints and debug

4. **Run the tests**:
   ```bash
   # Backend tests
   cd backend
   dotnet test

   # Frontend linting
   cd frontend
   npm run lint
   ```

## Getting Help

- **Development questions**: See [DEVELOPMENT.md](./DEVELOPMENT.md)
- **Build issues**: See [BUILD.md](./BUILD.md)
- **Found a bug**: Open an issue on GitHub
- **Need clarification**: Check the documentation or ask the team

## What's Available

### Features
- Month-by-month co-parenting calendar
- 50/50 split tracking
- VAB (child care leave) tracking
- Day comments and notes
- Parent name customization
- Import/Export functionality

### Tech Stack
- **Frontend**: React 18 + TypeScript + Vite
- **Backend**: .NET 8 C# Minimal API
- **Database**: SQL Server 2022
- **Infrastructure**: Docker + Docker Compose

### Development Tools
- VS Code debugging configurations
- Hot reload for backend and frontend
- Docker development environment
- Automated testing
- CI/CD pipeline

Happy coding! 🎉
