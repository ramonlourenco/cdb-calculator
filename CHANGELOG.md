# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-06-02

### Added
- Initial release of CDB B3 Calculator
- Hexagonal architecture backend (.NET 8)
  - Domain layer with pure business logic
  - Application layer with use cases
  - WebApi adapter with REST endpoints
- Angular 17+ frontend with Signals
  - Standalone component architecture
  - Reactive forms with validation
  - Functional HTTP interceptors (Correlation ID, Loading)
- CDB calculation engine
  - Monthly compound interest calculation
  - Regressive income tax rates (4 brackets)
  - Precision to 2 decimal places
- Swagger/OpenAPI documentation
- Correlation ID middleware for distributed tracing
- Structured JSON logging with Serilog
- Observability stack
  - Loki for log aggregation
  - Promtail for log scraping
  - Grafana for visualization and dashboards
- Docker containerization
  - Multi-stage builds for optimization
  - Alpine Linux for minimal images
  - Docker Compose orchestration
- Comprehensive testing
  - Backend: xUnit tests with >90% coverage
  - Frontend: Jasmine tests with >80% coverage
  - All income tax brackets tested
- Documentation
  - Architecture guide
  - Development setup guide
  - Testing guide
  - Deployment guide
  - Troubleshooting guide
  - Contributing guidelines

### Features
- [x] CDB calculation with accurate compounding
- [x] Income tax calculation (regressively)
- [x] REST API with Swagger
- [x] Frontend form validation
- [x] Results display card
- [x] Loading state management with Signals
- [x] Correlation ID tracking
- [x] Structured logging
- [x] Docker deployment
- [x] Observability integration
- [x] Unit tests (backend & frontend)
- [x] Health checks
- [x] CORS configuration

## Planned Features (v1.1.0)
- [ ] Database persistence (EF Core)
- [ ] User authentication
- [ ] Historical calculations
- [ ] Portfolio simulation
- [ ] Export reports (PDF/CSV)
- [ ] Performance benchmarks
- [ ] Redis caching
- [ ] Rate limiting
- [ ] Kubernetes deployment
- [ ] GraphQL API support
- [ ] Dark mode UI
- [ ] Mobile responsive improvements
- [ ] Multi-language support

## Known Issues
None reported at this time.

## Contributing
See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines.

## License
MIT License - See LICENSE file for details.

## Support
- Documentation: See README.md and related guides
- Issues: GitHub Issues
- Security: See CONTRIBUTING.md for security reporting
