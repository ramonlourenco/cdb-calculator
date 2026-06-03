# Contributing to CDB Calculator

Thank you for your interest in contributing to the CDB Calculator project! This document provides guidelines for contributing.

## Code of Conduct

Be respectful, inclusive, and professional. We're committed to providing a welcoming environment.

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/your-username/cdb-calculator.git`
3. Create a feature branch: `git checkout -b feature/your-feature`
4. Follow the development setup in `DEVELOPMENT.md`

## Development Workflow

### Before You Start
- Check open issues to avoid duplicates
- Discuss major changes in an issue first
- Read `ARCHITECTURE.md` to understand the design

### Making Changes

#### Backend (.NET 8)
```bash
cd backend

# Create feature
dotnet build
dotnet test

# Ensure >90% test coverage
dotnet test /p:CollectCoverage=true

# Code style
dotnet format
```

#### Frontend (Angular 17+)
```bash
cd frontend

# Install if needed
npm install

# Make changes
npm start  # Test locally

# Run tests
npm test -- --watch=false

# Ensure >80% coverage
npm test -- --code-coverage --watch=false

# Lint (if configured)
npm run lint
```

### Commit Guidelines

Follow conventional commits:
```
<type>(<scope>): <subject>

<body>

<footer>
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `test`: Tests
- `refactor`: Code refactoring
- `perf`: Performance improvement
- `chore`: Dependencies, build config

Examples:
```
feat(backend): add CDB calculation with IR brackets
fix(frontend): fix loading state not resetting on error
docs: add Grafana query examples
test: add 5 new test cases for income tax calculation
```

### Pull Request Process

1. **Ensure all tests pass locally**
   - Backend: `dotnet test`
   - Frontend: `npm test -- --watch=false`
   - Coverage: >90% backend, >80% frontend

2. **Update documentation**
   - Update `README.md` if needed
   - Add docstrings to public APIs
   - Document architectural changes in `ARCHITECTURE.md`

3. **Create PR with clear description**
   - Reference related issues: `Fixes #123`
   - Describe changes clearly
   - Include before/after if applicable

4. **Address review feedback**
   - Respond to all comments
   - Push additional commits (don't force push)
   - Mark conversations as resolved

5. **PR approval & merge**
   - At least 1 approval required
   - All CI checks must pass
   - Maintainer merges to main

## Testing Requirements

### Backend Tests
- Minimum 90% line coverage
- All business logic tested
- Edge cases handled
- Error scenarios covered

### Frontend Tests
- Minimum 80% coverage
- Services tested
- Interceptors tested
- User interactions verified

### Example Test Template

**Backend (xUnit):**
```csharp
[Theory]
[InlineData(1000, 6, 0.225)]
[InlineData(1000, 12, 0.20)]
public void Execute_AppliesCorrectTaxRate(decimal initial, int months, decimal expectedRate)
{
    var result = _useCase.Execute(initial, months);
    var expectedTax = (result.GrossValue - initial) * expectedRate;
    Assert.Equal(expectedTax, result.IncomeTax, 2);
}
```

**Frontend (Jasmine):**
```typescript
it('should inject correlation ID header', () => {
    service.calculate({initialValue: 1000, months: 12}).subscribe();
    const req = httpMock.expectOne('http://localhost:8080/api/cdbcalculator/calculate');
    expect(req.request.headers.has('X-Correlation-ID')).toBe(true);
    req.flush({});
});
```

## Code Style

### Backend (.NET)
- Follow Microsoft C# conventions
- Use `dotnet format` before committing
- PascalCase for public members
- camelCase for private members
- SOLID principles mandatory

### Frontend (Angular/TypeScript)
- Follow Angular style guide
- Use strict mode (enabled by default)
- Signal-based for reactive state
- Standalone components (no NgModule)
- typed service methods

### Documentation
- Clear, concise comments
- Avoid over-commenting obvious code
- Document public APIs
- Include examples for complex logic

## Areas for Contribution

### Backend
- [ ] Add database persistence layer (EF Core)
- [ ] Implement caching strategy
- [ ] Add rate limiting middleware
- [ ] Improve error handling
- [ ] Add more business validations

### Frontend
- [ ] Improve UI/UX design
- [ ] Add dark mode
- [ ] Implement PWA features
- [ ] Add more visualizations
- [ ] Accessibility improvements (WCAG)

### Infrastructure
- [ ] Kubernetes deployment
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Performance benchmarks
- [ ] Security scanning
- [ ] Load testing setup

### Documentation
- [ ] API documentation improvement
- [ ] Video tutorials
- [ ] Architecture diagrams
- [ ] Deployment guides for cloud providers
- [ ] Performance tuning guide

## Running Tests Locally

### Full Test Suite
```bash
# Backend + Frontend
./run-all-tests.sh  # or .bat on Windows

# Expected: All tests passing
```

### Before Pushing
```bash
# Run comprehensive checks
dotnet test backend              # Must pass
npm test frontend                # Must pass
dotnet format backend            # Format code
# ng lint (if configured)        # Lint code
```

## Documentation Standards

- Use Markdown
- Include code examples
- Add table of contents for long docs
- Link to related files
- Keep README.md updated

## Reporting Issues

Use GitHub Issues with template:
1. **Title:** Clear, concise description
2. **Description:** Detailed explanation
3. **Steps to Reproduce:** Exact steps to reproduce
4. **Expected vs Actual:** What should happen vs what happens
5. **Environment:** OS, versions (Docker/local)
6. **Screenshots/Logs:** If applicable

Example:
```markdown
## Issue: Loading overlay not disappearing

### Description
After API error, loading overlay stays visible on screen

### Steps
1. Open http://localhost
2. Enter invalid value (send manually via API)
3. Observe loading state

### Expected
Loading overlay disappears after 3 seconds

### Actual
Overlay remains indefinitely

### Environment
- OS: Windows 11
- Browser: Chrome 120
- Backend: Running locally
```

## Security

### Reporting Security Issues
⚠️ **Do NOT open public issues for security vulnerabilities**

Instead:
1. Email: security@project.dev
2. Include: Description, reproduction steps, impact
3. Allow 48-72 hours for response

## License

By contributing, you agree your code is licensed under the same license as the project (MIT).

## Questions?

- Open an issue for clarification
- Check existing documentation
- Review similar PRs for patterns
- Discuss in project discussions

## Recognition

Contributors will be acknowledged in:
- README.md contributors section
- Release notes
- GitHub contributors page

## Helpful Resources

- [ARCHITECTURE.md](ARCHITECTURE.md) - Design patterns
- [DEVELOPMENT.md](DEVELOPMENT.md) - Setup guide
- [TESTING.md](TESTING.md) - Testing patterns
- [DEPLOYMENT.md](DEPLOYMENT.md) - Production guide
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md) - Common issues

Thank you for contributing! 🙏
