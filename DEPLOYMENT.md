# CDB Calculator Deployment Guide

## Production Deployment

### Prerequisites
- Docker & Docker Compose installed
- Sufficient disk space (10GB minimum)
- Ports 80, 443, 8080, 3000, 3100 available
- HTTPS certificate (for production)

### Pre-deployment Checklist
- [ ] Environment variables configured
- [ ] Sensitive data (passwords, keys) in .env file (not committed)
- [ ] DNS records pointing to server
- [ ] SSL certificate obtained
- [ ] Database backups configured (if applicable)
- [ ] Monitoring alerts configured

## Deployment Steps

### 1. Clone Repository
```bash
git clone <repo-url>
cd cdb-calculator
```

### 2. Configure Environment
Create `.env.production`:
```
GF_SECURITY_ADMIN_PASSWORD=secure_password_here
ASPNETCORE_ENVIRONMENT=Production
```

### 3. Build & Deploy
```bash
# Build images
docker-compose build

# Start services
docker-compose up -d

# Verify health
docker-compose ps

# Check logs
docker-compose logs -f
```

### 4. Verification
```bash
# Test API
curl https://api.yourdomain.com/health

# Test Frontend
curl https://yourdomain.com/

# Check logs in Grafana
# http://yourdomain.com:3000
```

## HTTPS Setup

### Using Let's Encrypt with Nginx Reverse Proxy

#### Create nginx.conf (Reverse Proxy)
```nginx
upstream backend {
  server cdb-backend:8080;
}

upstream grafana {
  server grafana:3000;
}

server {
  listen 80;
  server_name yourdomain.com;
  return 301 https://$server_name$request_uri;
}

server {
  listen 443 ssl http2;
  server_name yourdomain.com;

  ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
  ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

  location / {
    proxy_pass http://cdb-frontend:80;
  }

  location /api/ {
    proxy_pass http://backend;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
  }

  location /grafana/ {
    proxy_pass http://grafana;
    proxy_set_header Host $host;
  }
}
```

#### Update docker-compose.yml
```yaml
nginx-reverse-proxy:
  image: nginx:alpine
  ports:
    - "80:80"
    - "443:443"
  volumes:
    - ./nginx.conf:/etc/nginx/nginx.conf:ro
    - /etc/letsencrypt:/etc/letsencrypt:ro
  networks:
    - cdb-network
```

#### Obtain Certificate (Certbot)
```bash
docker run --rm \
  -v /etc/letsencrypt:/etc/letsencrypt \
  certbot/certbot certonly \
  --standalone \
  -d yourdomain.com \
  -d api.yourdomain.com
```

## Scaling Considerations

### Horizontal Scaling
```yaml
# docker-compose.yml with load balancing
services:
  backend:
    deploy:
      replicas: 3
  nginx:
    image: nginx:alpine
    ports:
      - "8080:8080"
    volumes:
      - ./nginx-lb.conf:/etc/nginx/nginx.conf
```

### Load Balancer Config
```nginx
upstream backend-pool {
  least_conn;
  server cdb-backend-1:8080;
  server cdb-backend-2:8080;
  server cdb-backend-3:8080;
}

server {
  location /api/ {
    proxy_pass http://backend-pool;
  }
}
```

## Backup & Recovery

### Backup Volumes
```bash
# Backup Grafana
docker run --rm \
  -v grafana-storage:/data \
  -v $(pwd):/backup \
  busybox tar czf /backup/grafana-backup.tar.gz /data

# Backup Loki
docker run --rm \
  -v loki-storage:/data \
  -v $(pwd):/backup \
  busybox tar czf /backup/loki-backup.tar.gz /data
```

### Restore Volumes
```bash
# Restore Grafana
docker run --rm \
  -v grafana-storage:/data \
  -v $(pwd):/backup \
  busybox tar xzf /backup/grafana-backup.tar.gz -C /

# Restore Loki
docker run --rm \
  -v loki-storage:/data \
  -v $(pwd):/backup \
  busybox tar xzf /backup/loki-backup.tar.gz -C /
```

## Monitoring & Alerting

### Health Checks
```bash
# Monitor endpoint health
while true; do
  curl http://localhost:8080/health && echo "✅ API OK"
  curl http://localhost && echo "✅ Frontend OK"
  sleep 30
done
```

### Log Aggregation
- Use Grafana Alerts for critical errors
- Set up webhooks to Slack/Teams
- Monitor memory and CPU usage

### Prometheus (Optional Addition)
```yaml
prometheus:
  image: prom/prometheus:latest
  volumes:
    - ./prometheus.yml:/etc/prometheus/prometheus.yml
  ports:
    - "9090:9090"
```

## Updates & Maintenance

### Zero-Downtime Updates
```bash
# Pull new code
git pull

# Rebuild affected services only
docker-compose build cdb-backend

# Rolling restart
docker-compose up -d --no-deps --build cdb-backend

# Verify
curl http://localhost:8080/health
```

### Database Migrations (if added)
```bash
# Run migration container
docker-compose run cdb-backend dotnet ef database update

# Restart API
docker-compose restart cdb-backend
```

### Cleanup Old Images
```bash
docker system prune -a --volumes
```

## Performance Tuning

### Resource Limits
```yaml
services:
  cdb-backend:
    deploy:
      resources:
        limits:
          cpus: '1'
          memory: 512M
        reservations:
          cpus: '0.5'
          memory: 256M
```

### Database Connection Pooling (if added)
```csharp
services.AddDbContext<DbContext>(options =>
  options.UseSqlServer(
    connectionString,
    sqlOptions => sqlOptions.EnableRetryOnFailure()
  )
);
```

### Caching
- Implement Redis for session/calculation cache
- Configure Nginx cache headers
- Use CDN for static assets

## Security Hardening

### Secrets Management
```bash
# Use Docker secrets
echo "admin_password" | docker secret create admin_password -
```

### Network Isolation
```yaml
networks:
  public:
    driver: bridge
    driver_opts:
      com.docker.network.bridge.enable_ip_masquerade: "true"
  internal:
    driver: bridge
    driver_opts:
      com.docker.network.bridge.enable_ip_masquerade: "false"
```

### Firewall Rules
```bash
# Allow only necessary ports
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow 22/tcp
sudo ufw enable
```

## Rollback Procedure

```bash
# Get previous image SHA
docker images --format "{{.Repository}}\t{{.Tag}}\t{{.ID}}"

# Stop current
docker-compose down

# Update docker-compose.yml with previous image SHA
# Or use: docker-compose pull && docker-compose up -d

# Verify
docker-compose ps
```

## Support & Troubleshooting

See `TROUBLESHOOTING.md` for common issues.

For production issues:
1. Check logs: `docker-compose logs --tail=100`
2. Verify resources: `docker stats`
3. Check health endpoints
4. Review Grafana dashboards
5. Scale if needed
