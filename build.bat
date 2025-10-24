@echo off
REM Build script for Co-Parenting Calendar (Windows)
REM Usage: build.bat [backend|frontend|all|docker]

setlocal enabledelayedexpansion

set COMMAND=%1
if "%COMMAND%"=="" set COMMAND=all

if "%COMMAND%"=="backend" goto build_backend
if "%COMMAND%"=="frontend" goto build_frontend
if "%COMMAND%"=="all" goto build_all
if "%COMMAND%"=="docker" goto build_docker
if "%COMMAND%"=="test" goto run_tests
if "%COMMAND%"=="clean" goto clean
if "%COMMAND%"=="help" goto show_help
if "%COMMAND%"=="-h" goto show_help
if "%COMMAND%"=="--help" goto show_help

echo Error: Unknown command "%COMMAND%"
goto show_help

:build_backend
echo.
echo [INFO] Building backend (.NET 8)...
echo.

cd backend\CoParenting.API

echo [INFO] Restoring dependencies...
dotnet restore
if errorlevel 1 goto error

echo [INFO] Building in Release mode...
dotnet build --configuration Release --no-restore
if errorlevel 1 goto error

echo [INFO] Publishing...
dotnet publish --configuration Release --output .\publish --no-build
if errorlevel 1 goto error

cd ..\..

echo.
echo [SUCCESS] Backend built successfully
echo.
goto end

:build_frontend
echo.
echo [INFO] Building frontend (React + TypeScript)...
echo.

cd frontend

if not exist node_modules (
    echo [INFO] Installing dependencies...
    call npm ci
    if errorlevel 1 goto error
) else (
    echo [INFO] Dependencies already installed
)

echo [INFO] Building production bundle...
call npm run build
if errorlevel 1 goto error

cd ..

echo.
echo [SUCCESS] Frontend built successfully
echo.
goto end

:build_all
call :build_backend
call :build_frontend
echo.
echo [SUCCESS] All components built successfully!
echo.
goto end

:build_docker
echo.
echo [INFO] Building Docker images...
echo.

echo [INFO] Building backend image...
docker build -t coparenting-api:latest .\backend
if errorlevel 1 goto error

echo [INFO] Building frontend image...
docker build -t coparenting-frontend:latest .\frontend
if errorlevel 1 goto error

echo.
echo [SUCCESS] Docker images built successfully
echo.
goto end

:run_tests
echo.
echo [INFO] Running tests...
echo.

echo [INFO] Backend tests...
cd backend
dotnet test --configuration Release --verbosity normal
cd ..

echo [INFO] Frontend tests...
cd frontend
call npm test
cd ..

echo.
echo [SUCCESS] Tests completed
echo.
goto end

:clean
echo.
echo [INFO] Cleaning build artifacts...
echo.

REM Backend
if exist backend\CoParenting.API\bin rmdir /s /q backend\CoParenting.API\bin
if exist backend\CoParenting.API\obj rmdir /s /q backend\CoParenting.API\obj
if exist backend\CoParenting.API\publish rmdir /s /q backend\CoParenting.API\publish

if exist backend\CoParenting.Core\bin rmdir /s /q backend\CoParenting.Core\bin
if exist backend\CoParenting.Core\obj rmdir /s /q backend\CoParenting.Core\obj

if exist backend\CoParenting.Infrastructure\bin rmdir /s /q backend\CoParenting.Infrastructure\bin
if exist backend\CoParenting.Infrastructure\obj rmdir /s /q backend\CoParenting.Infrastructure\obj

REM Frontend
if exist frontend\dist rmdir /s /q frontend\dist

echo.
echo [SUCCESS] Cleaned successfully
echo.
goto end

:show_help
echo Co-Parenting Calendar Build Script
echo.
echo Usage: build.bat [command]
echo.
echo Commands:
echo   backend    - Build backend only
echo   frontend   - Build frontend only
echo   all        - Build both backend and frontend (default)
echo   docker     - Build Docker images
echo   test       - Run all tests
echo   clean      - Clean build artifacts
echo   help       - Show this help message
echo.
goto end

:error
echo.
echo [ERROR] Build failed!
echo.
exit /b 1

:end
endlocal
