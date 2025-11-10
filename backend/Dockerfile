FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY CoParenting.Core/CoParenting.Core.csproj CoParenting.Core/
COPY CoParenting.Infrastructure/CoParenting.Infrastructure.csproj CoParenting.Infrastructure/
COPY CoParenting.API/CoParenting.API.csproj CoParenting.API/

# Restore dependencies
RUN dotnet restore CoParenting.API/CoParenting.API.csproj

# Copy everything else
COPY . .

# Build the application
WORKDIR /src/CoParenting.API
RUN dotnet build -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

# Wait for SQL Server to be ready
COPY wait-for-it.sh /wait-for-it.sh
RUN chmod +x /wait-for-it.sh

EXPOSE 8080
ENTRYPOINT ["/wait-for-it.sh", "db:1433", "--", "dotnet", "CoParenting.API.dll"]
