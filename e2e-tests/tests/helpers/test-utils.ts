/**
 * Test configuration and constants
 */

export const config = {
  // Base URLs
  baseURL: process.env.BASE_URL || 'http://localhost:3000',
  apiURL: process.env.API_URL || 'http://localhost:8080',
  
  // Timeouts
  defaultTimeout: 30000,
  apiTimeout: 10000,
  
  // Test data
  testParentA: 'Test Parent A',
  testParentB: 'Test Parent B',
  
  // Wait times for services
  serviceStartupWait: 5000,
  dbInitWait: 10000,
};

/**
 * Wait for a service to be ready by checking its health endpoint
 */
export async function waitForService(url: string, maxAttempts = 30): Promise<void> {
  for (let i = 0; i < maxAttempts; i++) {
    try {
      const response = await fetch(url);
      if (response.ok) {
        console.log(`Service at ${url} is ready`);
        return;
      }
    } catch (error) {
      // Service not ready yet
    }
    await new Promise(resolve => setTimeout(resolve, 1000));
  }
  throw new Error(`Service at ${url} did not become ready in time`);
}

/**
 * Wait for the API to be ready
 */
export async function waitForAPI(): Promise<void> {
  await waitForService(`${config.apiURL}/api/config/parent-names`);
}

/**
 * Wait for the frontend to be ready
 */
export async function waitForFrontend(): Promise<void> {
  await waitForService(config.baseURL);
}

/**
 * Clean up test data by resetting parent names
 */
export async function cleanupTestData(): Promise<void> {
  try {
    await fetch(`${config.apiURL}/api/config/parent-names`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        parentAName: 'Parent A',
        parentBName: 'Parent B',
      }),
    });
  } catch (error) {
    console.warn('Failed to cleanup test data:', error);
  }
}

/**
 * Get a date key in YYYY-MM-DD format
 */
export function getDateKey(year: number, month: number, day: number): string {
  return `${year}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
}

/**
 * Get the current year and month for testing
 */
export function getCurrentYearMonth(): { year: number; month: number } {
  const now = new Date();
  return {
    year: now.getFullYear(),
    month: now.getMonth() + 1, // 1-12
  };
}
