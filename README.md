# Co-Parenting Calendar

A full-stack web application for managing 50/50 co-parenting schedules with React/TypeScript frontend, C# backend, and PostgreSQL database.

**🚀 New to the project?** Check out the [Quick Start Guide](./QUICKSTART.md) to get running in under 5 minutes!

## Architecture

### Technology Stack

**Frontend:**
- React 18 with TypeScript
- Vite build tool
- Component-based architecture
- Served by backend as static files

**Backend:**
- .NET 8 C# Minimal API
- Entity Framework Core (Database First)
- PostgreSQL 16
- Well-structured layered architecture
- Serves frontend static files from wwwroot

**Infrastructure:**
- Docker & Docker Compose
- PostgreSQL in container
- Multi-stage Docker builds
- Unified application container (frontend + backend)
- Health checks and dependency management

## Features

### Core Functionality
- **Single Month View**: View and manage one month at a time with navigation
- **Default Week Assignment**: Automatically assigns odd weeks (Monday-Sunday) to Parent A and even weeks to Parent B
- **50/50 Split Management**: Easily assign days to either parent
- **Day Comments**: Add notes and comments to specific days
- **VAB Tracking**: Mark and track "Vård av Barn" (child care leave) days
- **Day Swapping**: Change and reassign days between parents as needed

### Additional Features
- **Custom Parent Names**: Personalize the calendar with actual parent names (stored in database)
- **Month Actions**: 
  - Initialize month with defaults (odd/even week pattern)
  - Fill entire months with one parent
  - Alternate days automatically
- **Statistics Dashboard**: View summary of day distribution, VAB days, and comments
- **Data Persistence**: All data stored in PostgreSQL database
- **Import/Export**: Backup and restore your calendar data
- **API Documentation**: Swagger UI available in development mode

## Quick Start

### Prerequisites
- Docker Desktop installed (includes Docker Compose V2)
- Ports 5432 and 3000 available

**Note:** This project requires Docker Compose V2 (comes with Docker Desktop 3.0+ or Docker Engine 20.10+). Use `docker compose` (without hyphen). Docker Compose V1 (`docker-compose` with hyphen) is not supported due to the modern compose file syntax.

### Running the Application

1. **Clone the repository**
```bash
git clone <repository-url>
cd selmaschema
```

2. **Start all services**
```bash
# Docker Compose V2 (recommended)
docker compose up --build

# Or Docker Compose V1
docker-compose up --build
```

This will:
- Start PostgreSQL database on port 5432
- Initialize database schema automatically
- Build and start unified application (backend + frontend) on port 3000

3. **Access the application**
- Application: http://localhost:3000
- API: http://localhost:3000/api
- Swagger UI: http://localhost:3000/swagger

### Stopping the Application

```bash
docker compose down
# or: docker-compose down
```

To remove data volumes:
```bash
docker compose down -v
# or: docker-compose down -v
```

## Build Instructions

### Using Build Scripts (Recommended)

**Linux/Mac:**
```bash
# Build everything
./build.sh all

# Build backend only
./build.sh backend

# Build frontend only
./build.sh frontend

# Build Docker images
./build.sh docker

# Run tests
./build.sh test

# Clean build artifacts
./build.sh clean
```

**Windows:**
```cmd
REM Build everything
build.bat all

REM Build backend only
build.bat backend

REM Build frontend only
build.bat frontend

REM Build Docker images
build.bat docker

REM Run tests
build.bat test

REM Clean build artifacts
build.bat clean
```

### Using Make (Linux/Mac)

```bash
# Show all available commands
make help

# Build everything
make build

# Build and run with Docker
make docker-build
make start

# View logs
make logs

# Stop services
make stop

# Clean everything
make clean-all
```

### Manual Build Steps

#### Backend Build

**Prerequisites:**
- .NET 8 SDK installed
- SQL Server running (or use Docker)

**Steps:**
```bash
# Navigate to backend API project
cd backend/CoParenting.API

# Restore dependencies
dotnet restore

# Build in Release mode
dotnet build --configuration Release

# Run the API
dotnet run

# Or publish for deployment
dotnet publish --configuration Release --output ./publish
```

**Run tests:**
```bash
cd backend
dotnet test --configuration Release
```

#### Frontend Build

**Prerequisites:**
- Node.js 18+ installed
- npm installed

**Steps:**
```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm ci

# Development build with hot reload
npm run dev

# Production build
npm run build

# Preview production build
npm run preview

# Lint code
npm run lint
```

**Output:**
- Development server: http://localhost:5173
- Production build: `frontend/dist/` directory

#### Docker Build

**Build unified image:**
```bash
# Build application image (backend + frontend)
docker build -t coparenting-app:latest .
```

**Build with Docker Compose:**
```bash
# Build all services
docker compose build

# Build and start
docker compose up --build
```

### Continuous Integration

The project includes GitHub Actions workflows for automated building and testing:

**Workflow: `.github/workflows/ci-cd.yml`**

Triggers on:
- Push to `main`, `develop`, or `copilot/**` branches
- Pull requests to `main` or `develop`

Build jobs:
1. **Build** - Builds frontend and backend together, runs tests, creates artifacts
2. **Docker Build** - Builds unified Docker image and validates Docker Compose configuration
3. **Integration Tests** - Tests services working together
4. **Docker Publish** - Publishes image to GitHub Container Registry (on merge to main)

**Docker Images:**

After successful merge to `main`, Docker image is automatically published to GitHub Container Registry:
- `ghcr.io/twids/selmaschema/coparenting-app:latest`

Images are tagged with:
- `latest` - Latest stable version from main branch
- `main-<commit-sha>` - Specific commit from main branch

**Workflow: `.github/workflows/e2e-tests.yml`**

Triggers on:
- Push to `main`, `develop`, or `copilot/**` branches
- Pull requests to `main` or `develop`

E2E Testing jobs:
1. **E2E Tests** - Runs comprehensive end-to-end tests with Playwright
   - Starts all services with Docker Compose
   - Waits for services to be healthy
   - Runs full test suite covering all functionality
   - Uploads test results and reports as artifacts
   - **Fails PR builds if tests fail**

**Artifacts generated:**
- `application-build` - Published .NET application with frontend
- `e2e-test-results` - Test execution results and screenshots
- `e2e-test-report` - HTML test report (7 days retention)

**Note on Docker Compose:**
- CI/CD uses Docker Compose V2 (`docker compose` command)
- Local development supports both `docker compose` (V2) and `docker-compose` (V1)
- If using older Docker versions, install Docker Compose V1 separately

### Build Requirements

**Backend:**
- .NET 8 SDK
- C# 10 or later
- SQL Server 2022 (or Docker)

**Frontend:**
- Node.js 18+ (20 recommended)
- npm 9+
- Modern browser for testing

**Docker:**
- Docker Engine 20+
- Docker Compose 2+

**Build times (approximate):**
- Backend: 30-60 seconds
- Frontend: 60-90 seconds
- Docker images: 3-5 minutes

### Stopping the Application

```bash
docker-compose down
```

To remove data volumes:
```bash
docker-compose down -v
```

## Development

For detailed local development instructions, debugging, and troubleshooting, see the **[Development Guide](./DEVELOPMENT.md)**.

### Quick Development Setup

**Option 1: Docker with Hot Reload (Recommended for Development)**
```bash
# Start all services with hot reload enabled
docker compose -f docker-compose.yml -f docker-compose.dev.yml up

# Access the application:
# - Frontend (with Vite HMR): http://localhost:5173
# - API (with dotnet watch): http://localhost:8080
# - Swagger: http://localhost:8080/swagger
#
# Note: In production, frontend is served by backend on port 3000
```

**Option 2: Native Development**

Frontend:
```bash
cd frontend
npm install
npm run dev
# Available at http://localhost:5173
```

Backend:
```bash
cd backend/CoParenting.API
dotnet restore
dotnet watch run
# Available at http://localhost:8080
# Note: Requires SQL Server running (see Option 1 for database setup)
```

### VS Code Development

1. Open project in Visual Studio Code
2. Install recommended extensions (you'll be prompted)
3. Press `F5` to start debugging
4. Choose a launch configuration:
   - **Full Stack (Backend + Frontend)** - Debug both simultaneously
   - **Debug Backend API (.NET)** - Debug backend only
   - **Debug Frontend (Chrome)** - Debug frontend in Chrome

See [DEVELOPMENT.md](./DEVELOPMENT.md) for comprehensive debugging guides and troubleshooting.

### Database Migrations

The database schema is automatically created using the init-db.sql script. The schema includes:

**Tables:**
- `DayAssignments` - Stores day-by-day parenting assignments
- `Configurations` - Stores application settings (parent names, etc.)

To modify the schema, update:
1. Entity models in `backend/CoParenting.Core/Entities/`
2. DbContext in `backend/CoParenting.Infrastructure/Data/`
3. SQL script in `backend/init-db.sql`

## Project Structure

```
selmaschema/
├── backend/
│   ├── CoParenting.API/          # API endpoints and configuration
│   │   ├── Endpoints/            # Minimal API endpoints
│   │   ├── Services/             # Business logic layer
│   │   ├── DTOs/                 # Data transfer objects
│   │   └── wwwroot/              # Frontend build output (generated)
│   ├── CoParenting.Core/         # Domain entities
│   │   └── Entities/            
│   ├── CoParenting.Infrastructure/ # Data access layer
│   │   └── Data/                 # EF Core DbContext
│   ├── init-db.sql               # Database initialization script
│   └── wait-for-it.sh           # Database readiness script
├── frontend/
│   ├── src/
│   │   ├── components/           # React components
│   │   ├── types.ts              # TypeScript interfaces
│   │   ├── App.tsx               # Main application
│   │   └── main.tsx              # Entry point
│   └── package.json              # NPM dependencies
├── Dockerfile                    # Unified application container
└── docker-compose.yml            # Multi-container orchestration
```

## API Endpoints

### Day Assignments
- `GET /api/days/{year}/{month}` - Get all assignments for a month
- `GET /api/days/{year}/{month}/{day}` - Get specific day assignment
- `PUT /api/days/{year}/{month}/{day}` - Create/update day assignment
- `POST /api/days/{year}/{month}/initialize` - Initialize month with default pattern

### Configuration
- `GET /api/config/parent-names` - Get parent names
- `PUT /api/config/parent-names` - Update parent names

### Statistics
- `GET /api/statistics/{year}` - Get year statistics

## Default Week Assignment Logic

The application uses ISO 8601 week numbering (weeks start on Monday):
- **Odd weeks (1, 3, 5...)**: Assigned to Parent A
- **Even weeks (2, 4, 6...)**: Assigned to Parent B

This provides a fair 50/50 split over time.

## Database Configuration

**Connection String:** (in docker-compose.yml and appsettings.json)
```
Host=db;Database=CoParentingCalendar;Username=postgres;Password=YourStrong@Passw0rd
```

**Security Note:** Change the default password in production!

## Testing

### Backend Tests
```bash
cd backend
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm test
```

### End-to-End (E2E) Tests

Comprehensive E2E tests cover all major functionality using Playwright.

**Prerequisites:**
- Application services running via `docker compose up`

**Run tests:**
```bash
cd e2e-tests
npm install
npx playwright install chromium
npm test
```

**View test report:**
```bash
cd e2e-tests
npm run test:report
```

**Run tests in headed mode (with visible browser):**
```bash
cd e2e-tests
npm run test:headed
```

**Debug tests:**
```bash
cd e2e-tests
npm run test:debug
```

**Test Coverage:**
- API health and configuration endpoints
- Calendar navigation and UI
- Day assignment operations
- VAB (child care leave) tracking
- Comments functionality
- Parent names configuration
- Statistics dashboard
- Import/Export functionality

See [e2e-tests/README.md](e2e-tests/README.md) for detailed documentation.

**CI/CD Integration:**
E2E tests run automatically on every pull request. Check the "E2E Tests" workflow in the GitHub Actions tab for results.

## Troubleshooting

### Build Issues

**Backend build fails:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with verbose logging
dotnet restore --verbosity detailed

# Check .NET version
dotnet --version  # Should be 8.0.x
```

**Frontend build fails:**
```bash
# Clear npm cache
npm cache clean --force

# Remove node_modules and reinstall
rm -rf node_modules package-lock.json
npm install

# Check Node version
node --version  # Should be 18+ or 20+
```

**Docker build fails:**
```bash
# Check Docker is running
docker ps

# Clear Docker build cache
docker system prune -a

# Rebuild without cache
docker-compose build --no-cache
```

### Database Connection Issues
- Ensure SQL Server container is healthy: `docker-compose ps`
- Check logs: `docker-compose logs db`
- Verify connection string in appsettings.json
- Wait 30 seconds for SQL Server to fully start

### API Not Starting
- Check API logs: `docker-compose logs api`
- Ensure port 8080 is available: `lsof -i :8080` (Linux/Mac) or `netstat -ano | findstr :8080` (Windows)
- Verify database is accessible
- Check for .NET runtime: `dotnet --info`

### Frontend Build Errors
- Check node version (requires Node 18+): `node --version`
- Clear node_modules: `rm -rf node_modules && npm install`
- Check frontend logs: `docker-compose logs frontend`

## Color Coding

- **Blue** (Light blue background): Days assigned to Parent A
- **Pink** (Light pink background): Days assigned to Parent B
- **Gray**: Unassigned days
- **Yellow border**: VAB (child care leave) days

## License

[Your License Here]

## Support

For issues or questions, please open an issue on the GitHub repository.