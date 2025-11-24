# Multi-stage Dockerfile to build frontend and backend together
# Build context should be the repository root

# Stage 1: Build frontend
FROM node:20-alpine AS frontend-build
WORKDIR /frontend

# Copy frontend package files
COPY frontend/package*.json ./

# Install frontend dependencies
RUN npm ci

# Copy frontend source
COPY frontend/ ./

# Build frontend
RUN npm run build

# Stage 2: Build backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src

# Copy backend project files
COPY backend/CoParenting.Core/CoParenting.Core.csproj backend/CoParenting.Core/
COPY backend/CoParenting.Infrastructure/CoParenting.Infrastructure.csproj backend/CoParenting.Infrastructure/
COPY backend/CoParenting.API/CoParenting.API.csproj backend/CoParenting.API/

# Restore dependencies
RUN dotnet restore backend/CoParenting.API/CoParenting.API.csproj

# Copy backend source
COPY backend/ backend/

# Build the application
WORKDIR /src/backend/CoParenting.API
RUN dotnet build -c Release -o /app/build

# Stage 3: Publish backend
FROM backend-build AS publish
WORKDIR /src/backend/CoParenting.API
RUN dotnet publish -c Release -o /app/publish

# Stage 4: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy published backend
COPY --from=publish /app/publish .

# Copy frontend build to wwwroot
COPY --from=frontend-build /frontend/dist ./wwwroot

EXPOSE 8080

ENTRYPOINT ["dotnet", "CoParenting.API.dll"]
