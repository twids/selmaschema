import { request as playwrightRequest } from '@playwright/test';

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
  for (let i = 0; i < 30; i++) {
    try {
      const response = await fetch(`${config.apiURL}/api/auth/me`);
      if (response.ok || response.status === 401) return;
    } catch {
      // Service not ready yet
    }
    await new Promise(resolve => setTimeout(resolve, 1000));
  }
  throw new Error(`API at ${config.apiURL} did not become ready in time`);
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
  const api = await playwrightRequest.newContext({
    baseURL: config.apiURL,
    storageState: 'test-results/.auth/admin.json',
  });
  try {
    await api.put('/api/config/parent-names', {
      data: {
        parentAName: 'Parent A',
        parentBName: 'Parent B',
      },
    });
  } catch (error) {
    console.warn('Failed to cleanup test data:', error);
  } finally {
    await api.dispose();
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
