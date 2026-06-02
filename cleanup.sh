#!/bin/bash

echo "Cleaning up CDB Calculator..."
docker-compose down
docker system prune -f --volumes
echo "✅ Cleanup completed!"
