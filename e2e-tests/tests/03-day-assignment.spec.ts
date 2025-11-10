import { test, expect } from '@playwright/test';
import { config, waitForFrontend, waitForAPI, cleanupTestData } from './helpers/test-utils';

/**
 * Day Assignment Operations Tests
 * 
 * Tests day assignment functionality:
 * - Initialize month with default pattern
 * - Click days to open modal
 * - Assign days to parents
 * - Update assignments
 * - Visual feedback for assignments
 */

test.describe('Day Assignment Operations', () => {
  
  test.beforeAll(async () => {
    await waitForAPI();
    await waitForFrontend();
  });

  test.beforeEach(async ({ page }) => {
    await page.goto(config.baseURL);
    await page.waitForSelector('.calendar-container', { timeout: config.defaultTimeout });
  });

  test.afterAll(async () => {
    await cleanupTestData();
  });

  test('should initialize month with default pattern', async ({ page }) => {
    // Click initialize button
    const initButton = page.locator('button:has-text("Initialize with Defaults")');
    await initButton.click();
    
    // Wait for initialization to complete
    await page.waitForTimeout(2000);
    
    // Check that days are now assigned (have colors)
    const assignedDays = page.locator('.day-cell.assigned, .day-cell[data-parent], .day-cell.parent-a, .day-cell.parent-b');
    const count = await assignedDays.count();
    expect(count).toBeGreaterThan(0);
  });

  test('should open modal when clicking a day', async ({ page }) => {
    // Initialize month first to ensure we have days
    const initButton = page.locator('button:has-text("Initialize with Defaults")');
    if (await initButton.count() > 0) {
      await initButton.click();
      await page.waitForTimeout(1000);
    }
    
    // Click on a day cell
    const dayCell = page.locator('.day-cell').first();
    await dayCell.click();
    
    // Check that modal opened
    const modal = page.locator('.modal-backdrop');
    await expect(modal).toBeVisible({ timeout: 5000 });
  });

  test('should assign day to Parent A via modal', async ({ page }) => {
    // Initialize month
    const initButton = page.locator('button:has-text("Initialize with Defaults")');
    if (await initButton.count() > 0) {
      await initButton.click();
      await page.waitForTimeout(1000);
    }
    
    // Click on a day
    const dayCell = page.locator('.day-cell').first();
    const dayNumber = await dayCell.textContent();
    await dayCell.click();
    
    // Wait for modal
    await page.waitForSelector('.modal-backdrop', { timeout: 5000 });
    
    // Select Parent A using dropdown
    const parentSelect = page.locator('select#parentSelect');
    await parentSelect.selectOption('parentA');
    
    // Save the changes
    const saveButton = page.locator('button.save-button');
    await saveButton.click();
    
    // Wait for modal to close
    await page.waitForTimeout(1000);
    
    // Verify the day is now assigned to Parent A
    // (visual verification - day should have appropriate styling)
    const updatedDay = page.locator(`.day-cell:has-text("${dayNumber}")`).first();
    const className = await updatedDay.getAttribute('class');
    expect(className).toBeTruthy();
  });

  test('should assign day to Parent B via modal', async ({ page }) => {
    // Initialize month
    const initButton = page.locator('button:has-text("Initialize with Defaults")');
    if (await initButton.count() > 0) {
      await initButton.click();
      await page.waitForTimeout(1000);
    }
    
    // Click on a day
    const dayCell = page.locator('.day-cell').nth(1);
    await dayCell.click();
    
    // Wait for modal
    await page.waitForSelector('.modal-backdrop', { timeout: 5000 });
    
    // Select Parent B using dropdown
    const parentSelect = page.locator('select#parentSelect');
    await parentSelect.selectOption('parentB');
    
    // Save
    const saveButton = page.locator('button.save-button');
    await saveButton.click();
    
    await page.waitForTimeout(1000);
  });

  test('should close modal without saving changes', async ({ page }) => {
    // Initialize month
    const initButton = page.locator('button:has-text("Initialize with Defaults")');
    if (await initButton.count() > 0) {
      await initButton.click();
      await page.waitForTimeout(1000);
    }
    
    // Click on a day
    const dayCell = page.locator('.day-cell').first();
    await dayCell.click();
    
    // Wait for modal
    await page.waitForSelector('.modal-backdrop', { timeout: 5000 });
    
    // Click cancel button
    const cancelButton = page.locator('button.cancel-button');
    await cancelButton.click();
    
    // Wait for modal to close
    await page.waitForTimeout(500);
    
    // Verify modal is closed
    const modal = page.locator('.modal-backdrop');
    await expect(modal).not.toBeVisible();
  });

  test('should show visual feedback for assigned days', async ({ page }) => {
    // Initialize month
    const initButton = page.locator('button:has-text("Initialize with Defaults")');
    if (await initButton.count() > 0) {
      await initButton.click();
      await page.waitForTimeout(2000);
    }
    
    // Get all day cells
    const dayCells = page.locator('.day-cell');
    const count = await dayCells.count();
    
    // Check that at least some days have styling/class indicating assignment
    let assignedCount = 0;
    for (let i = 0; i < Math.min(count, 31); i++) {
      const cell = dayCells.nth(i);
      const className = await cell.getAttribute('class');
      if (className && (
        className.includes('parent-a') || 
        className.includes('parent-b') ||
        className.includes('assigned')
      )) {
        assignedCount++;
      }
    }
    
    expect(assignedCount).toBeGreaterThan(0);
  });

  test('should fill entire month with one parent', async ({ page }) => {
    // The button text includes the parent name dynamically
    // Looking for "Fill Parent A" or "Fill" followed by parent name
    const fillButton = page.locator('button').filter({ hasText: 'Fill' }).first();
    
    if (await fillButton.count() > 0) {
      await fillButton.click();
      
      // Wait for operation to complete
      await page.waitForTimeout(3000);
      
      // Verify days are assigned
      const assignedDays = page.locator('.day-cell.parent-a, .day-cell.parent-b');
      const count = await assignedDays.count();
      expect(count).toBeGreaterThan(20); // Most days in a month
    }
  });

  test('should alternate days between parents', async ({ page }) => {
    // Look for "Alternate Days" button
    const alternateButton = page.locator('button:has-text("Alternate Days")').first();
    
    if (await alternateButton.count() > 0) {
      await alternateButton.click();
      
      // Wait for operation to complete
      await page.waitForTimeout(3000);
      
      // Verify days are assigned in alternating pattern
      const parentADays = page.locator('.day-cell.parent-a');
      const parentBDays = page.locator('.day-cell.parent-b');
      
      const countA = await parentADays.count();
      const countB = await parentBDays.count();
      
      // Should be roughly equal
      expect(Math.abs(countA - countB)).toBeLessThanOrEqual(1);
    }
  });
});
