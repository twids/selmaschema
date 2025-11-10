import { test, expect } from '@playwright/test';
import { config, waitForAPI, cleanupTestData } from './helpers/test-utils';

/**
 * API Health and Configuration Tests
 * 
 * Tests the backend API endpoints for:
 * - Parent names configuration (GET and PUT)
 * - API health and connectivity
 * - Database integration
 */

test.describe('API Health and Configuration', () => {
  
  test.beforeAll(async () => {
    // Wait for API to be ready
    await waitForAPI();
  });

  test.afterAll(async () => {
    // Clean up test data
    await cleanupTestData();
  });

  test('should get default parent names', async ({ request }) => {
    const response = await request.get(`${config.apiURL}/api/config/parent-names`);
    expect(response.ok()).toBeTruthy();
    
    const data = await response.json();
    expect(data).toHaveProperty('parentAName');
    expect(data).toHaveProperty('parentBName');
    expect(typeof data.parentAName).toBe('string');
    expect(typeof data.parentBName).toBe('string');
  });

  test('should update parent names', async ({ request }) => {
    // Update parent names
    const updateResponse = await request.put(`${config.apiURL}/api/config/parent-names`, {
      data: {
        parentAName: config.testParentA,
        parentBName: config.testParentB,
      },
      headers: {
        'Content-Type': 'application/json',
      },
    });
    expect(updateResponse.ok()).toBeTruthy();

    // Verify the update
    const getResponse = await request.get(`${config.apiURL}/api/config/parent-names`);
    expect(getResponse.ok()).toBeTruthy();
    
    const data = await getResponse.json();
    expect(data.parentAName).toBe(config.testParentA);
    expect(data.parentBName).toBe(config.testParentB);
  });

  test('should get empty month data for new month', async ({ request }) => {
    const year = 2025;
    const month = 12;
    
    const response = await request.get(`${config.apiURL}/api/days/${year}/${month}`);
    expect(response.ok()).toBeTruthy();
    
    const data = await response.json();
    expect(data).toHaveProperty('year', year);
    expect(data).toHaveProperty('month', month);
    expect(data).toHaveProperty('days');
    expect(Array.isArray(data.days)).toBeTruthy();
  });

  test('should initialize month with default pattern', async ({ request }) => {
    const year = 2024;
    const month = 11;
    
    // Initialize the month
    const initResponse = await request.post(`${config.apiURL}/api/days/${year}/${month}/initialize`);
    expect(initResponse.ok()).toBeTruthy();

    // Get the month data
    const getResponse = await request.get(`${config.apiURL}/api/days/${year}/${month}`);
    expect(getResponse.ok()).toBeTruthy();
    
    const data = await getResponse.json();
    expect(data.days.length).toBeGreaterThan(0);
    
    // Verify the pattern has been applied
    const assignedDays = data.days.filter((day: any) => day.parent !== null);
    expect(assignedDays.length).toBeGreaterThan(0);
  });

  test('should update a specific day', async ({ request }) => {
    const year = 2024;
    const month = 11;
    const day = 15;
    
    // Update the day
    const updateResponse = await request.put(
      `${config.apiURL}/api/days/${year}/${month}/${day}`,
      {
        data: {
          parent: 'A',
          isVAB: true,
          comment: 'Test comment',
        },
        headers: {
          'Content-Type': 'application/json',
        },
      }
    );
    expect(updateResponse.ok()).toBeTruthy();

    // Get the month data to verify
    const getResponse = await request.get(`${config.apiURL}/api/days/${year}/${month}`);
    const data = await getResponse.json();
    
    const updatedDay = data.days.find((d: any) => {
      const dayDate = new Date(d.date);
      return dayDate.getDate() === day;
    });
    
    expect(updatedDay).toBeDefined();
    expect(updatedDay.parent).toBe('A');
    expect(updatedDay.isVAB).toBe(true);
    expect(updatedDay.comment).toBe('Test comment');
  });

  test('should get statistics for a year', async ({ request }) => {
    const year = 2024;
    
    const response = await request.get(`${config.apiURL}/api/statistics/${year}`);
    expect(response.ok()).toBeTruthy();
    
    const data = await response.json();
    expect(data).toHaveProperty('year', year);
    expect(data).toHaveProperty('totalDaysParentA');
    expect(data).toHaveProperty('totalDaysParentB');
    expect(data).toHaveProperty('totalVABDays');
    expect(data).toHaveProperty('totalComments');
  });
});
