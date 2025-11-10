# End-to-End Testing Guide

This guide provides comprehensive information about the E2E testing setup for the Co-Parenting Calendar application.

## Table of Contents

1. [Overview](#overview)
2. [Test Coverage](#test-coverage)
3. [Setup](#setup)
4. [Running Tests](#running-tests)
5. [Understanding Test Results](#understanding-test-results)
6. [Debugging Failed Tests](#debugging-failed-tests)
7. [Writing New Tests](#writing-new-tests)
8. [CI/CD Integration](#cicd-integration)
9. [Best Practices](#best-practices)
10. [Troubleshooting](#troubleshooting)

## Overview

The E2E test suite uses [Playwright](https://playwright.dev/) to test the complete application stack:
- Frontend (React/TypeScript)
- Backend API (.NET 8)
- Database (SQL Server)
- Integration between all components

**Benefits:**
- Validates entire user workflows
- Catches integration issues
- Ensures API contracts are maintained
- Verifies UI functionality
- Tests in real browser environment
- Runs automatically on PRs

## Test Coverage

The E2E test suite covers all major functionality:

### 1. API Health and Configuration (`01-api-health.spec.ts`)

Tests backend API functionality:
- ✅ GET parent names configuration
- ✅ PUT update parent names
- ✅ GET month data
- ✅ POST initialize month with default pattern
- ✅ PUT update specific day assignment
- ✅ GET year statistics

**Why it matters:** Ensures the backend API is working correctly and can handle all required operations.

### 2. Calendar Navigation (`02-calendar-navigation.spec.ts`)

Tests UI navigation and display:
- ✅ Application loads with header and controls
- ✅ Current month displays by default
- ✅ Navigate to next month
- ✅ Navigate to previous month
- ✅ Calendar grid displays days
- ✅ Legend shows parent names
- ✅ Month action buttons visible
- ✅ Change year via dropdown
- ✅ Change month via dropdown

**Why it matters:** Ensures users can navigate the calendar and access all months/years.

### 3. Day Assignment Operations (`03-day-assignment.spec.ts`)

Tests day assignment workflows:
- ✅ Initialize month with default pattern (odd/even weeks)
- ✅ Open modal when clicking a day
- ✅ Assign day to Parent A via modal
- ✅ Assign day to Parent B via modal
- ✅ Close modal without saving
- ✅ Visual feedback for assigned days
- ✅ Fill entire month with one parent
- ✅ Alternate days between parents

**Why it matters:** Core functionality for managing the 50/50 parenting schedule.

### 4. VAB and Comments (`04-vab-comments.spec.ts`)

Tests VAB tracking and comments:
- ✅ Mark day as VAB (child care leave)
- ✅ Unmark day as VAB
- ✅ Add comment to a day
- ✅ Update existing comment
- ✅ Remove comment
- ✅ Handle VAB and comment together

**Why it matters:** Essential features for tracking child care and adding notes to specific days.

### 5. Parent Names and Statistics (`05-parent-names-statistics.spec.ts`)

Tests configuration and statistics:
- ✅ Display default parent names in legend
- ✅ Update parent names via settings
- ✅ Display statistics section
- ✅ Show day distribution in statistics
- ✅ Update statistics when days are assigned
- ✅ Show VAB count in statistics
- ✅ Show comment count in statistics
- ✅ Persist parent names across page refresh

**Why it matters:** Personalization and data insights are important for user experience.

### 6. Import/Export (`06-import-export.spec.ts`)

Tests data portability:
- ✅ Export button visible
- ✅ Import button/input visible
- ✅ Export calendar data as JSON
- ✅ Import interaction
- ✅ Preserve data after export and reimport

**Why it matters:** Users need to backup and restore their calendar data.

## Setup

### Prerequisites

**Local Development:**
- Node.js 20+
- Docker and Docker Compose
- Git

**CI/CD:**
- Automatically handled by GitHub Actions

### Installation

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd selmaschema
   ```

2. **Install E2E test dependencies:**
   ```bash
   cd e2e-tests
   npm install
   ```

3. **Install Playwright browsers:**
   ```bash
   npx playwright install chromium
   ```

4. **Start application services:**
   ```bash
   cd ..  # Go to project root
   docker compose up
   ```

   Wait for all services to be healthy (30-60 seconds).

## Running Tests

### Basic Commands

```bash
# From e2e-tests directory

# Run all tests
npm test

# Run with visible browser
npm run test:headed

# Run in debug mode
npm run test:debug

# Run with UI mode (interactive)
npm run test:ui

# View test report
npm run test:report
```

### Advanced Usage

```bash
# Run specific test file
npx playwright test tests/01-api-health.spec.ts

# Run tests matching a pattern
npx playwright test --grep "should get default parent names"

# Run with custom base URL
BASE_URL=http://localhost:5000 npm test

# Run with custom timeout
npx playwright test --timeout=90000
```

### Test Execution Flow

1. **Setup Phase:**
   - Tests wait for API to be ready
   - Tests wait for frontend to be ready
   - Browser context is created

2. **Test Execution:**
   - Each test runs independently
   - Tests use fresh browser context
   - Screenshots/videos captured on failure

3. **Teardown Phase:**
   - Test data cleaned up
   - Browser context closed
   - Report generated

## Understanding Test Results

### Console Output

```
Running 45 tests using 1 worker

✓ tests/01-api-health.spec.ts:15:3 › should get default parent names (1s)
✓ tests/01-api-health.spec.ts:28:3 › should update parent names (2s)
...

45 passed (2m)
```

### HTML Report

The HTML report provides:
- Test execution timeline
- Screenshots and videos
- Network requests
- Console logs
- Trace viewer

Open with: `npm run test:report`

### Test Artifacts

Located in `test-results/`:
- **Screenshots**: Captured on failure
- **Videos**: Recorded for failed tests
- **Traces**: Full execution trace for debugging

### CI Results

On GitHub:
1. Go to your PR
2. Click "Checks" tab
3. Select "E2E Tests" workflow
4. View results and download artifacts

## Debugging Failed Tests

### Strategy 1: Use Debug Mode

```bash
npm run test:debug
```

This opens Playwright Inspector where you can:
- Step through test execution
- Pause and resume tests
- Inspect page state
- View console logs

### Strategy 2: View Trace

```bash
npx playwright show-trace test-results/path-to-trace.zip
```

Trace viewer shows:
- Timeline of actions
- DOM snapshots
- Network requests
- Console logs
- Screenshots at each step

### Strategy 3: Run Headed Mode

```bash
npm run test:headed
```

Watch the browser as tests execute to see what's happening.

### Strategy 4: Add Debug Points

In your test file:

```typescript
await page.pause(); // Pauses execution, opens inspector
console.log(await page.content()); // Log page content
await page.screenshot({ path: 'debug.png' }); // Take screenshot
```

### Common Issues

**Issue: Services not ready**
```bash
# Check services
docker compose ps

# View logs
docker compose logs api
docker compose logs frontend
docker compose logs db

# Restart services
docker compose down -v
docker compose up
```

**Issue: Element not found**
- Check selector is correct
- Add `waitForSelector` before interacting
- Verify element exists in DOM

**Issue: Test timeout**
- Increase timeout in test
- Check if service is slow
- Verify network connectivity

**Issue: Flaky test**
- Add proper waits
- Check for race conditions
- Ensure test isolation

## Writing New Tests

### Test Template

```typescript
import { test, expect } from '@playwright/test';
import { config, waitForAPI, waitForFrontend } from './helpers/test-utils';

test.describe('Feature Name', () => {
  
  test.beforeAll(async () => {
    await waitForAPI();
    await waitForFrontend();
  });

  test.beforeEach(async ({ page }) => {
    await page.goto(config.baseURL);
    await page.waitForSelector('.main-container');
  });

  test('should do something specific', async ({ page }) => {
    // Arrange: Set up test data
    const testData = 'test value';
    
    // Act: Perform action
    await page.fill('input[name="field"]', testData);
    await page.click('button[type="submit"]');
    
    // Assert: Verify result
    await expect(page.locator('.result')).toContainText(testData);
  });
});
```

### Best Practices

1. **Use descriptive test names:**
   ```typescript
   test('should display error message when form is submitted with empty fields', ...)
   ```

2. **Wait for elements properly:**
   ```typescript
   await page.waitForSelector('.modal');
   await expect(page.locator('.modal')).toBeVisible();
   ```

3. **Use locators wisely:**
   ```typescript
   // Good: Specific and stable
   page.locator('button[aria-label="Save"]')
   
   // Avoid: Too generic
   page.locator('button')
   ```

4. **Keep tests independent:**
   - Each test should work alone
   - Don't rely on test execution order
   - Clean up test data

5. **Use test utilities:**
   ```typescript
   import { config, waitForAPI, cleanupTestData } from './helpers/test-utils';
   ```

6. **Handle async properly:**
   ```typescript
   // Always await async operations
   await page.click('button');
   await page.waitForTimeout(1000);
   ```

## CI/CD Integration

### Workflow File

Located at `.github/workflows/e2e-tests.yml`

### Triggers

- Push to `main`, `develop`, or `copilot/**` branches
- Pull requests to `main` or `develop`

### Execution Steps

1. Checkout code
2. Setup Node.js
3. Install dependencies
4. Install Playwright browsers
5. Start services with Docker Compose
6. Wait for services to be healthy
7. Run E2E tests
8. Upload test results (artifacts)
9. Show logs on failure
10. Stop services

### PR Builds

**When E2E tests fail:**
- ❌ PR build fails
- Cannot merge until tests pass
- Review test results in artifacts

**When E2E tests pass:**
- ✅ PR build passes
- Safe to merge
- All functionality validated

### Viewing Results

1. **GitHub UI:**
   - Go to Actions tab
   - Select workflow run
   - View job details

2. **Artifacts:**
   - Download test report (HTML)
   - Download test results (screenshots, videos)
   - View in browser offline

3. **Logs:**
   - Job logs show test output
   - Service logs available on failure

## Best Practices

### Test Design

- **Single Responsibility**: Each test should test one thing
- **Clear Assertions**: Use descriptive expect messages
- **No Hardcoded Waits**: Use `waitFor*` methods
- **Data Independence**: Tests shouldn't share state
- **Readable Code**: Use helpers and clear variable names

### Test Maintenance

- **Keep Tests Updated**: Update when UI changes
- **Remove Obsolete Tests**: Delete tests for removed features
- **Refactor Common Code**: Use helpers and utilities
- **Document Complex Tests**: Add comments for non-obvious logic

### Performance

- **Minimize Page Loads**: Reuse page when possible
- **Parallel Execution**: Use for independent tests
- **Smart Waits**: Wait only as long as needed
- **Optimize Selectors**: Use efficient locators

### Reliability

- **Stable Selectors**: Use `data-testid` or stable attributes
- **Proper Waits**: Ensure elements are ready
- **Error Handling**: Handle expected errors gracefully
- **Retries**: Use retries for flaky external dependencies

## Troubleshooting

### Services Issues

**Problem: Database connection failed**
```bash
# Check database is running
docker compose ps db

# View database logs
docker compose logs db

# Restart database
docker compose restart db
```

**Problem: API not responding**
```bash
# Check API health
curl http://localhost:8080/api/config/parent-names

# View API logs
docker compose logs api

# Restart API
docker compose restart api
```

**Problem: Frontend not loading**
```bash
# Check frontend
curl http://localhost:3000

# View frontend logs
docker compose logs frontend

# Restart frontend
docker compose restart frontend
```

### Test Issues

**Problem: Tests timing out**
- Increase timeout in `playwright.config.ts`
- Check if services are slow to start
- Add more specific waits in tests

**Problem: Flaky tests**
- Add proper waits instead of `waitForTimeout`
- Check for race conditions
- Ensure test isolation

**Problem: Element not found**
- Verify selector is correct
- Check element is visible
- Add `waitForSelector` before interaction

### Browser Issues

**Problem: Browser won't start**
```bash
# Reinstall browsers
npx playwright install --force chromium

# Install system dependencies
npx playwright install-deps
```

**Problem: Headless failures but headed works**
- Check viewport size
- Verify animations don't cause issues
- Look for timing-related problems

## Support

For help:

1. **Documentation:**
   - This guide
   - [Playwright docs](https://playwright.dev/)
   - Project README

2. **Debugging:**
   - Use debug mode
   - Check test artifacts
   - Review service logs

3. **Issues:**
   - Open GitHub issue
   - Include error messages
   - Attach test results

4. **Contributing:**
   - Write tests for new features
   - Keep this guide updated
   - Share debugging tips
