# E2E Tests for Co-Parenting Calendar

This directory contains end-to-end (E2E) tests for the Co-Parenting Calendar application using [Playwright](https://playwright.dev/).

## Overview

The E2E test suite covers all major functionality of the application:

1. **API Health and Configuration** (`01-api-health.spec.ts`)
   - Parent names GET and PUT endpoints
   - Database connectivity
   - Month data retrieval
   - Day assignment updates
   - Statistics endpoint

2. **Calendar Navigation** (`02-calendar-navigation.spec.ts`)
   - Page load and UI elements
   - Month navigation (forward/backward)
   - Year changes
   - Calendar grid display
   - Legend display

3. **Day Assignment Operations** (`03-day-assignment.spec.ts`)
   - Initialize month with default pattern
   - Open day modal
   - Assign days to parents
   - Update assignments
   - Fill month operations
   - Alternate days pattern

4. **VAB and Comments** (`04-vab-comments.spec.ts`)
   - Mark days as VAB (child care leave)
   - Unmark VAB days
   - Add comments to days
   - Update comments
   - Remove comments
   - Combine VAB and comments

5. **Parent Names and Statistics** (`05-parent-names-statistics.spec.ts`)
   - Display parent names in legend
   - Update parent names
   - View statistics dashboard
   - Day distribution statistics
   - VAB count in statistics
   - Comment count in statistics
   - Data persistence across page refresh

6. **Import/Export** (`06-import-export.spec.ts`)
   - Export calendar data
   - Verify exported file format
   - Import data
   - Data preservation

## Prerequisites

### For Local Development

- Node.js 18+ installed
- Docker and Docker Compose installed
- Application services running via `docker compose up`

### For CI/CD

- GitHub Actions will handle all setup automatically

## Installation

Install dependencies:

```bash
cd e2e-tests
npm install
```

Install Playwright browsers:

```bash
npx playwright install chromium
```

## Running Tests Locally

### 1. Start the Application Services

First, make sure all services are running:

```bash
cd ..  # Go to project root
docker compose up
```

Wait for all services to be healthy (database, API, frontend).

### 2. Run the Tests

In a new terminal, from the `e2e-tests` directory:

```bash
# Run all tests
npm test

# Run tests in headed mode (with browser visible)
npm run test:headed

# Run tests in debug mode
npm run test:debug

# Run tests with UI mode
npm run test:ui

# Run specific test file
npx playwright test tests/01-api-health.spec.ts

# Run tests matching a pattern
npx playwright test --grep "should get default parent names"
```

### 3. View Test Reports

After running tests, view the HTML report:

```bash
npm run test:report
```

This will open an interactive HTML report in your browser.

## Test Configuration

The tests are configured in `playwright.config.ts`:

- **Base URL**: `http://localhost:3000` (frontend)
- **API URL**: `http://localhost:8080` (backend)
- **Browser**: Chromium (Chrome)
- **Timeout**: 60 seconds per test
- **Workers**: 1 (sequential execution to avoid database conflicts)
- **Retries**: 0 locally, 2 in CI

### Environment Variables

You can customize the test environment:

```bash
# Change base URL
BASE_URL=http://localhost:5000 npm test

# Change API URL
API_URL=http://localhost:9000 npm test

# Run in CI mode
CI=true npm test
```

## Test Structure

```
e2e-tests/
├── tests/
│   ├── helpers/
│   │   └── test-utils.ts         # Shared test utilities
│   ├── 01-api-health.spec.ts     # API tests
│   ├── 02-calendar-navigation.spec.ts
│   ├── 03-day-assignment.spec.ts
│   ├── 04-vab-comments.spec.ts
│   ├── 05-parent-names-statistics.spec.ts
│   └── 06-import-export.spec.ts
├── playwright.config.ts           # Playwright configuration
├── package.json                   # NPM dependencies
└── tsconfig.json                  # TypeScript configuration
```

## Debugging Failed Tests

### 1. Run in Debug Mode

```bash
npm run test:debug
```

This opens Playwright Inspector where you can step through tests.

### 2. View Screenshots and Videos

Failed tests automatically capture:
- Screenshots (in `test-results/`)
- Videos (in `test-results/`)
- Traces (in `test-results/`)

### 3. View Trace Files

```bash
npx playwright show-trace test-results/path-to-trace.zip
```

### 4. Check Logs

Test output includes:
- Console logs from the application
- Network requests
- Test execution details

### 5. Run Single Test

```bash
npx playwright test tests/01-api-health.spec.ts --debug
```

## Writing New Tests

### Example Test Structure

```typescript
import { test, expect } from '@playwright/test';
import { config, waitForAPI, waitForFrontend } from './helpers/test-utils';

test.describe('My Feature Tests', () => {
  
  test.beforeAll(async () => {
    await waitForAPI();
    await waitForFrontend();
  });

  test.beforeEach(async ({ page }) => {
    await page.goto(config.baseURL);
    await page.waitForSelector('.calendar-container');
  });

  test('should do something', async ({ page }) => {
    // Your test code here
    await expect(page.locator('.my-element')).toBeVisible();
  });
});
```

### Best Practices

1. **Use descriptive test names**: Clearly describe what the test does
2. **Wait for elements**: Use `waitForSelector` to ensure elements are ready
3. **Clean up test data**: Reset state after tests when needed
4. **Use helpers**: Utilize `test-utils.ts` for common operations
5. **Keep tests independent**: Each test should work standalone
6. **Avoid hardcoded waits**: Use `waitFor*` methods instead of `waitForTimeout`

## CI/CD Integration

Tests run automatically on every pull request via GitHub Actions. See `.github/workflows/e2e-tests.yml` for configuration.

### CI Environment

- Tests run in a Docker container
- Services started via `docker compose`
- Browsers installed automatically
- Test reports uploaded as artifacts

### Viewing CI Results

1. Go to the PR on GitHub
2. Click "Checks" tab
3. Select "E2E Tests" workflow
4. View test results and download artifacts (screenshots, videos, reports)

## Troubleshooting

### Services Not Ready

If tests fail with connection errors:

```bash
# Check services are running
docker compose ps

# View service logs
docker compose logs

# Restart services
docker compose down -v
docker compose up
```

### Browser Install Issues

If Playwright browsers fail to install:

```bash
# Install system dependencies
npx playwright install-deps

# Install browsers
npx playwright install
```

### Port Conflicts

If ports are already in use:

```bash
# Find what's using the port
lsof -i :3000
lsof -i :8080

# Change ports in docker-compose.yml or use environment variables
```

### Database Connection Issues

If database tests fail:

```bash
# Ensure database is healthy
docker compose ps db

# Reset database
docker compose down -v
docker compose up db
```

### Test Timeouts

If tests timeout:

1. Increase timeout in `playwright.config.ts`
2. Check if services are slow to start
3. Verify network connectivity

## Performance

### Test Execution Time

Approximate times:
- Full suite: 2-4 minutes
- Single test file: 20-40 seconds
- Single test: 5-15 seconds

### Optimization Tips

1. Run tests in parallel when possible (be careful with database state)
2. Use `test.describe.configure({ mode: 'parallel' })` for independent tests
3. Reuse browser contexts when appropriate
4. Minimize page reloads

## Support

For issues or questions:

1. Check this README
2. Review [Playwright documentation](https://playwright.dev/)
3. Check test output and logs
4. Open an issue on GitHub

## Contributing

When adding new features:

1. Write E2E tests for the feature
2. Ensure tests pass locally
3. Verify tests pass in CI
4. Update this README if needed
