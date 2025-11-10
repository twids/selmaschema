# DevContainer Configuration for Co-Parenting Calendar

This directory contains the configuration for developing the Co-Parenting Calendar application in GitHub Codespaces or VS Code Remote - Containers.

## What is a DevContainer?

A development container (DevContainer) is a Docker container specifically configured for development. It includes all the tools, runtimes, and dependencies needed to work on this project without installing anything on your local machine.

## Features

This DevContainer includes:

### Development Tools
- **.NET 8 SDK**: For C# backend development
- **Node.js 20**: For React/TypeScript frontend development
- **pnpm**: Fast, disk-efficient package manager (also supports npm)
- **Docker CLI**: For building and running containers inside the DevContainer
- **Git**: Version control
- **GitHub CLI**: Interact with GitHub from the command line

### VS Code Extensions
- **C# Development**: C# and C# Dev Kit
- **JavaScript/TypeScript**: ESLint, Prettier, TypeScript support
- **React**: Auto-rename tags, path intellisense
- **Docker**: Docker extension for managing containers
- **Git**: GitLens for enhanced Git features
- **Code Quality**: Spell checker

### Ports
The following ports are automatically forwarded:
- **3000**: Frontend (production build with nginx)
- **5173**: Vite dev server (hot reload)
- **5000**: Alternative .NET port
- **8080**: Backend API
- **9229**: Node.js debugger port
- **1433**: SQL Server

## Quick Start

### Option 1: GitHub Codespaces (Recommended)

1. Go to the repository on GitHub
2. Click the green **Code** button
3. Select **Codespaces** tab
4. Click **Create codespace on main** (or your branch)
5. Wait for the container to build and initialize (~3-5 minutes first time)
6. Start coding! All dependencies are already installed.

### Option 2: VS Code Remote - Containers (Local)

**Prerequisites:**
- [Docker Desktop](https://www.docker.com/products/docker-desktop) installed and running
- [VS Code](https://code.visualstudio.com/) with [Remote - Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

**Steps:**
1. Clone the repository
   ```bash
   git clone https://github.com/twids/selmaschema.git
   cd selmaschema
   ```

2. Open in VS Code
   ```bash
   code .
   ```

3. When prompted, click **Reopen in Container**
   - Or press `F1`, type "Remote-Containers: Reopen in Container", and press Enter

4. Wait for the container to build (~5-10 minutes first time)
5. The post-create script will automatically:
   - Restore .NET dependencies
   - Install npm/pnpm packages
   - Configure git

## Development Workflow

### Backend Development

```bash
# Navigate to backend
cd backend/CoParenting.API

# Run the API
dotnet run

# Run tests
cd ..
dotnet test

# Build for release
dotnet build --configuration Release
```

The API will be available at `http://localhost:8080`

### Frontend Development

```bash
# Navigate to frontend
cd frontend

# Start dev server (with hot reload)
npm run dev
# or
pnpm dev

# Build for production
npm run build

# Lint code
npm run lint
```

The dev server will be available at `http://localhost:5173`

### Full Stack with Docker Compose

```bash
# Start all services (database, API, frontend)
docker compose up --build

# Stop all services
docker compose down

# Stop and remove volumes
docker compose down -v
```

Access the application:
- Frontend: `http://localhost:3000`
- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`

### Build Scripts

```bash
# Build everything
./build.sh all

# Build backend only
./build.sh backend

# Build frontend only
./build.sh frontend

# Run tests
./build.sh test

# Clean build artifacts
./build.sh clean
```

### Using Make

```bash
# Show all available commands
make help

# Build all
make build

# Start with Docker Compose
make start

# View logs
make logs

# Stop services
make stop
```

## Customization

### Adding VS Code Extensions

Edit `.devcontainer/devcontainer.json` and add extension IDs to the `extensions` array:

```json
"extensions": [
  "ms-dotnettools.csharp",
  "your-publisher.your-extension"
]
```

### Changing Forwarded Ports

Edit the `forwardPorts` array in `devcontainer.json`:

```json
"forwardPorts": [3000, 5173, 8080]
```

### Modifying Environment Variables

Edit the `remoteEnv` section in `devcontainer.json`:

```json
"remoteEnv": {
  "MY_VARIABLE": "value"
}
```

### Adding Development Tools

Edit `.devcontainer/Dockerfile` to install additional tools:

```dockerfile
RUN apt-get update && apt-get install -y \
    your-package-here
```

## Docker Support

This DevContainer is configured with **Docker-outside-of-Docker**, which means:

- You can run Docker commands inside the DevContainer
- Docker containers run on the host machine (or Codespaces VM)
- The Docker socket is mounted from the host
- You can build and run `docker-compose` services

Example:
```bash
# Build and run the application with Docker
docker compose up --build

# View running containers
docker ps

# Build custom images
docker build -t my-image ./backend
```

## Troubleshooting

### Container Build Fails

1. Make sure Docker Desktop is running (local development)
2. Try rebuilding without cache:
   - VS Code: `F1` → "Remote-Containers: Rebuild Container Without Cache"
   - Codespaces: Delete the codespace and create a new one

### Extensions Not Installing

1. Check your internet connection
2. Rebuild the container
3. Manually install extensions from the Extensions panel

### Port Already in Use

1. Check if another process is using the port:
   ```bash
   # On Linux/Mac
   lsof -i :8080
   
   # On Windows (in PowerShell)
   netstat -ano | findstr :8080
   ```

2. Stop the process or change the port in the application configuration

### Docker Commands Not Working

1. Ensure Docker Desktop is running
2. Verify Docker socket is mounted:
   ```bash
   ls -la /var/run/docker.sock
   ```

3. Check Docker daemon:
   ```bash
   docker ps
   ```

### Slow Container Build

First-time container builds can take 5-10 minutes. Subsequent builds are faster due to layer caching.

To speed up:
1. Use a faster internet connection
2. Close unnecessary applications
3. Increase Docker Desktop memory allocation

## File Structure

```
.devcontainer/
├── Dockerfile           # Container image definition
├── devcontainer.json    # DevContainer configuration
├── post-create.sh       # Post-creation setup script
└── README.md           # This file
```

## Additional Resources

- [VS Code Remote - Containers Documentation](https://code.visualstudio.com/docs/remote/containers)
- [GitHub Codespaces Documentation](https://docs.github.com/en/codespaces)
- [DevContainers Specification](https://containers.dev/)
- [Main Project README](../README.md)
- [Build Guide](../BUILD.md)

## Support

If you encounter issues:

1. Check this README
2. Review the [main project documentation](../README.md)
3. Check existing GitHub issues
4. Open a new issue with:
   - DevContainer configuration being used (Codespaces or local)
   - Steps to reproduce
   - Error messages
   - Screenshots if applicable

## Contributing

When modifying the DevContainer configuration:

1. Test changes locally with VS Code Remote - Containers
2. Test in GitHub Codespaces if possible
3. Update this README if adding new features
4. Document any new requirements or tools
5. Keep the configuration minimal and focused

---

**Happy coding in your fully configured development environment!** 🚀
