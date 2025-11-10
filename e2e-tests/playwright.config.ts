import { defineConfig, devices } from '@playwright/test';

/**
 * E2E Testing Configuration for Co-Parenting Calendar
 * 
 * This configuration supports:
 * - Running tests against Docker Compose services
 * - Running tests locally during development
 * - Running tests in CI/CD pipelines
 */

const baseURL = process.env.BASE_URL || 'http://localhost:3000';
const apiURL = process.env.API_URL || 'http://localhost:8080';

export default defineConfig({
  testDir: './tests',
  
  // Maximum time one test can run for
  timeout: 60 * 1000,
  
  // Test execution settings
  fullyParallel: false, // Run tests sequentially to avoid database conflicts
  forbidOnly: !!process.env.CI, // Fail the build on CI if you accidentally left test.only
  retries: process.env.CI ? 2 : 0, // Retry failed tests in CI
  workers: process.env.CI ? 1 : 1, // Single worker to avoid database conflicts
  
  // Reporter to use
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['junit', { outputFile: 'test-results/junit.xml' }],
    ['list']
  ],
  
  // Shared settings for all projects
  use: {
    // Base URL for navigation
    baseURL,
    
    // Collect trace when retrying the failed test
    trace: 'on-first-retry',
    
    // Screenshot on failure
    screenshot: 'only-on-failure',
    
    // Video on failure
    video: 'retain-on-failure',
    
    // Maximum time each action can take
    actionTimeout: 10 * 1000,
    
    // Navigation timeout
    navigationTimeout: 30 * 1000,
  },

  // Configure projects for different browsers
  projects: [
    {
      name: 'chromium',
      use: { 
        ...devices['Desktop Chrome'],
        // Pass API URL to tests via context options
        contextOptions: {
          // Allow tests to access the API URL
          extraHTTPHeaders: {
            'X-Test-API-URL': apiURL,
          },
        },
      },
    },
  ],

  // Run web server before starting tests (for local development)
  // In CI, services are started via docker-compose
  webServer: process.env.CI ? undefined : {
    command: 'echo "Please start services with: docker compose up"',
    url: baseURL,
    reuseExistingServer: true,
    timeout: 120 * 1000,
  },
});
