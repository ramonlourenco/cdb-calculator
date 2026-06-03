# Troubleshooting Guide

## Backend Issues

### Container fails to start
- Check Docker logs: `docker-compose logs cdb-backend`
- Verify Dockerfile compiles locally: `dotnet build src/CdbCalc.Adapters.Primary.WebApi`
- Check SDK version: `dotnet --version` (should be 8.0+)

### Health check failing
- Ensure API is listening on 8080
- Check appsettings.json: `ASPNETCORE_URLS=http://+:8080`
- Test locally: `curl http://localhost:8080/health`

### Serilog not logging
- Verify JSON formatter is configured in Program.cs
- Check LogLevel in appsettings.json (should be `Information` or lower)
- Ensure output to console with `.WriteTo.Console(new JsonFormatter())`

## Frontend Issues

### API connection refused
- Check backend is running: `docker ps | grep cdb-backend`
- Verify backend URL in `nginx.conf`: `proxy_pass http://cdb-backend:8080`
- Test API directly: `curl http://localhost:8080/health`

### Signals not updating
- Verify component imports `signal` from `@angular/core`
- Check Signal is accessed with `()`: `isLoading()` not `isLoading`
- Ensure interceptor calls `set()` on signal

### Tests failing
- Run with verbose: `ng test --verbose`
- Check TestBed configuration includes required providers
- Verify mock data matches service return types

## Docker Issues

### Build fails
- Clear Docker cache: `docker system prune -a`
- Check internet connection for downloading images
- Verify Dockerfile syntax: `docker build --no-cache -t test:latest .`

### Port conflicts
- Check existing containers: `docker ps -a`
- Free ports: `docker-compose down`
- Alternative ports in docker-compose.yml

### Volume permissions
- Linux: Check ownership `ls -la` on volumes
- Windows: Ensure Docker Desktop has file sharing enabled
- Fix: `sudo chown -R 1000:1000 volume-path`

## Observability Issues

### Loki not receiving logs
- Check Promtail is running: `docker-compose ps | grep promtail`
- Verify Promtail config points to correct Loki URL
- Check logs: `docker-compose logs promtail | head -50`

### Grafana dashboard empty
- Verify Loki datasource is configured: Grafana → Configuration → Data Sources
- Test query manually: Explore → Loki → `{job="cdb-backend"}`
- Check logs exist: `curl http://localhost:3100/loki/api/v1/query_range?query={job="cdb-backend"}`

## Performance Optimization

### Reduce image size
- Use Alpine variants
- Remove build dependencies in Docker multistage
- Clear npm cache: `npm ci --prefer-offline`

### Faster startup
- Pre-warm JIT in backend: Not needed for Alpine, use chiseled distroless
- Cache Docker layers: Keep Dockerfile dependencies at top

## Clean Rebuild

```bash
# Remove everything
docker-compose down -v
docker system prune -a

# Rebuild from scratch
docker-compose up --build

# Check status
docker-compose ps
```
