@echo off
REM CDB Calculator - Cleanup (Windows)

echo Cleaning up CDB Calculator...
docker-compose down
docker system prune -f --volumes
echo ✅ Cleanup completed!
