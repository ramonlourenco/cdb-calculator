@echo off
REM CDB Calculator - Build & Deploy (Windows)

echo ================================
echo CDB Calculator - Build and Deploy
echo ================================
echo.

echo Step 1: Building images...
docker-compose build

echo.
echo Step 2: Starting services...
docker-compose up -d

echo.
echo Waiting for services to be healthy...
timeout /t 10 /nobreak

echo.
echo ================================
echo ✅ Services Started Successfully!
echo ================================
echo.
echo Access URLs:
echo   Frontend (SPA):       http://localhost
echo   Backend (API):        http://localhost:8080
echo   Swagger/OpenAPI:      http://localhost:8080/swagger
echo   Grafana (Logs):       http://localhost:3000
echo   Loki API:             http://localhost:3100
echo.
echo Default Grafana credentials:
echo   Username: admin
echo   Password: admin
echo.
echo To view logs:
echo   docker-compose logs -f backend
echo   docker-compose logs -f frontend
echo.
echo To stop:
echo   docker-compose down
echo.
