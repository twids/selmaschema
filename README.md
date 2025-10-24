# Co-Parenting Calendar

A full-stack web application for managing 50/50 co-parenting schedules with React/TypeScript frontend, C# backend, and SQL Server database.

## Architecture

### Technology Stack

**Frontend:**
- React 18 with TypeScript
- Vite build tool
- Nginx web server
- Component-based architecture

**Backend:**
- .NET 8 C# Minimal API
- Entity Framework Core (Database First)
- SQL Server 2022
- Well-structured layered architecture

**Infrastructure:**
- Docker & Docker Compose
- SQL Server in container
- Multi-stage Docker builds
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
- **Data Persistence**: All data stored in SQL Server database
- **Import/Export**: Backup and restore your calendar data
- **API Documentation**: Swagger UI available in development mode

## Quick Start

### Prerequisites
- Docker Desktop installed
- Docker Compose installed
- Ports 1433, 8080, and 3000 available

### Running the Application

1. **Clone the repository**
```bash
git clone <repository-url>
cd selmaschema
```

2. **Start all services**
```bash
docker-compose up --build
```

This will:
- Start SQL Server database on port 1433
- Initialize database schema
- Start C# API on port 8080
- Start React frontend on port 3000

3. **Access the application**
- Frontend: http://localhost:3000
- API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger

### Stopping the Application

```bash
docker-compose down
```

To remove data volumes:
```bash
docker-compose down -v
```

## Development

### Frontend Development

```bash
cd frontend
npm install
npm run dev
```

The frontend will be available at http://localhost:5173

### Backend Development

```bash
cd backend/CoParenting.API
dotnet restore
dotnet run
```

The API will be available at http://localhost:8080

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
│   │   └── DTOs/                 # Data transfer objects
│   ├── CoParenting.Core/         # Domain entities
│   │   └── Entities/            
│   ├── CoParenting.Infrastructure/ # Data access layer
│   │   └── Data/                 # EF Core DbContext
│   ├── init-db.sql               # Database initialization script
│   ├── Dockerfile                # Backend container image
│   └── wait-for-it.sh           # Database readiness script
├── frontend/
│   ├── src/
│   │   ├── components/           # React components
│   │   ├── types.ts              # TypeScript interfaces
│   │   ├── App.tsx               # Main application
│   │   └── main.tsx              # Entry point
│   ├── Dockerfile                # Frontend container image
│   ├── nginx.conf                # Nginx configuration
│   └── package.json              # NPM dependencies
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
Server=db;Database=CoParentingCalendar;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
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

## Troubleshooting

### Database Connection Issues
- Ensure SQL Server container is healthy: `docker-compose ps`
- Check logs: `docker-compose logs db`
- Verify connection string in appsettings.json

### API Not Starting
- Check API logs: `docker-compose logs api`
- Ensure port 8080 is available
- Verify database is accessible

### Frontend Build Errors
- Check node version (requires Node 18+)
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