import { test, expect } from '@playwright/test';
import { config, waitForFrontend, waitForAPI, cleanupTestData } from './helpers/test-utils';

/**
 * Calendar Navigation and UI Tests
 * 
 * Tests the frontend calendar interface:
 * - Page loads correctly
 * - Month navigation works
 * - Year changes properly
 * - Calendar displays correctly
 */

test.describe('Calendar Navigation', () => {
  
  test.beforeAll(async () => {
    // Wait for services to be ready
    await waitForAPI();
    await waitForFrontend();
  });

  test.beforeEach(async ({ page }) => {
    // Navigate to the app
    await page.goto(config.baseURL);
    // Wait for the calendar to load
    await page.waitForSelector('.calendar-container', { timeout: config.defaultTimeout });
  });

  test.afterAll(async () => {
    await cleanupTestData();
  });

  test('should load the application with header and controls', async ({ page }) => {
    // Check for main UI elements
    await expect(page.locator('h1')).toContainText('Co-Parenting Calendar');
    
    // Check for controls
    await expect(page.locator('.controls-container')).toBeVisible();
    
    // Check for calendar
    await expect(page.locator('.calendar-container')).toBeVisible();
  });

  test('should display current month by default', async ({ page }) => {
    const now = new Date();
    const monthNames = [
      'January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December'
    ];
    const currentMonthName = monthNames[now.getMonth()];
    
    // Check that the current month is displayed
    await expect(page.locator('.month-header')).toContainText(currentMonthName);
  });

  test('should navigate to next month', async ({ page }) => {
    // Get initial month
    const initialMonth = await page.locator('.month-header').textContent();
    
    // Click next month button
    await page.click('button:has-text("→")');
    
    // Wait for the month to change
    await page.waitForTimeout(500);
    
    // Verify month changed
    const newMonth = await page.locator('.month-header').textContent();
    expect(newMonth).not.toBe(initialMonth);
  });

  test('should navigate to previous month', async ({ page }) => {
    // Get initial month
    const initialMonth = await page.locator('.month-header').textContent();
    
    // Click previous month button
    await page.click('button:has-text("←")');
    
    // Wait for the month to change
    await page.waitForTimeout(500);
    
    // Verify month changed
    const newMonth = await page.locator('.month-header').textContent();
    expect(newMonth).not.toBe(initialMonth);
  });

  test('should display calendar grid with days', async ({ page }) => {
    // Check for calendar grid
    await expect(page.locator('.calendar-grid')).toBeVisible();
    
    // Check for day cells
    const dayCells = page.locator('.day-cell');
    const count = await dayCells.count();
    expect(count).toBeGreaterThan(0);
    expect(count).toBeLessThanOrEqual(31);
  });

  test('should display legend with parent names', async ({ page }) => {
    // Check for legend
    await expect(page.locator('.legend-container')).toBeVisible();
    
    // Check for parent names in legend
    const legendText = await page.locator('.legend-container').textContent();
    expect(legendText).toBeTruthy();
  });

  test('should show month action buttons', async ({ page }) => {
    // Look for month action buttons
    const monthActions = page.locator('.month-actions');
    await expect(monthActions).toBeVisible();
    
    // Check for initialize button
    await expect(page.locator('button:has-text("Initialize with Defaults")')).toBeVisible();
  });

  test('should change year via dropdown', async ({ page }) => {
    // Find year selector
    const yearSelect = page.locator('select[name="year"], select#year, .year-selector select');
    
    if (await yearSelect.count() > 0) {
      const currentYear = await yearSelect.inputValue();
      const newYear = String(Number(currentYear) + 1);
      
      // Select new year
      await yearSelect.selectOption(newYear);
      
      // Wait for data to load
      await page.waitForTimeout(500);
      
      // Verify year changed (check if the value stuck)
      const updatedYear = await yearSelect.inputValue();
      expect(updatedYear).toBe(newYear);
    }
  });

  test('should change month via dropdown', async ({ page }) => {
    // Find month selector
    const monthSelect = page.locator('select[name="month"], select#month, .month-selector select');
    
    if (await monthSelect.count() > 0) {
      const currentMonth = await monthSelect.inputValue();
      
      // Select a different month (use index 5 for June)
      await monthSelect.selectOption('5');
      
      // Wait for data to load
      await page.waitForTimeout(500);
      
      // Verify month changed
      const updatedMonth = await monthSelect.inputValue();
      expect(updatedMonth).not.toBe(currentMonth);
      expect(updatedMonth).toBe('5');
    }
  });
});
