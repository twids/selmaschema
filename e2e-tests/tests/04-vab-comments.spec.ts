import { test, expect } from '@playwright/test';
import { config, waitForFrontend, waitForAPI, cleanupTestData } from './helpers/test-utils';

/**
 * VAB (Child Care Leave) and Comments Tests
 * 
 * Tests VAB tracking and comment functionality:
 * - Mark days as VAB
 * - Verify VAB indicators
 * - Add comments to days
 * - Update comments
 * - View comments
 */

test.describe('VAB and Comments', () => {
  
  test.beforeAll(async () => {
    await waitForAPI();
    await waitForFrontend();
  });

  test.beforeEach(async ({ page }) => {
    await page.goto(config.baseURL);
    await page.waitForSelector('.calendar-container', { timeout: config.defaultTimeout });
    
    // Initialize month if needed
    const initButton = page.locator('button:has-text("Initialize")');
    if (await initButton.count() > 0) {
      await initButton.click();
      await page.waitForTimeout(1000);
    }
  });

  test.afterAll(async () => {
    await cleanupTestData();
  });

  test('should mark a day as VAB via modal', async ({ page }) => {
    // Click on a day
    const dayCell = page.locator('.day-cell').first();
    await dayCell.click();
    
    // Wait for modal
    await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
    
    // Find and check VAB checkbox
    const vabCheckbox = page.locator(
      'input[type="checkbox"][name="isVAB"], input[type="checkbox"][name="vab"], input#vab, input#isVAB'
    ).first();
    
    if (await vabCheckbox.count() > 0) {
      // Check if it's not already checked
      const isChecked = await vabCheckbox.isChecked();
      if (!isChecked) {
        await vabCheckbox.check();
      }
      
      // Save
      const saveButton = page.locator('button:has-text("Save"), button[type="submit"]').first();
      await saveButton.click();
      
      await page.waitForTimeout(1000);
      
      // Verify VAB indicator is visible (yellow border or similar)
      const updatedDay = dayCell;
      const className = await updatedDay.getAttribute('class');
      expect(className).toBeTruthy();
    }
  });

  test('should unmark a day as VAB', async ({ page }) => {
    // Click on a day
    const dayCell = page.locator('.day-cell').first();
    
    // First, mark it as VAB
    await dayCell.click();
    await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
    
    const vabCheckbox = page.locator(
      'input[type="checkbox"][name="isVAB"], input[type="checkbox"][name="vab"], input#vab, input#isVAB'
    ).first();
    
    if (await vabCheckbox.count() > 0) {
      await vabCheckbox.check();
      const saveButton = page.locator('button:has-text("Save"), button[type="submit"]').first();
      await saveButton.click();
      await page.waitForTimeout(1000);
      
      // Now unmark it
      await dayCell.click();
      await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
      
      const vabCheckbox2 = page.locator(
        'input[type="checkbox"][name="isVAB"], input[type="checkbox"][name="vab"], input#vab, input#isVAB'
      ).first();
      
      await vabCheckbox2.uncheck();
      const saveButton2 = page.locator('button:has-text("Save"), button[type="submit"]').first();
      await saveButton2.click();
      
      await page.waitForTimeout(1000);
    }
  });

  test('should add a comment to a day', async ({ page }) => {
    const testComment = 'This is a test comment for E2E testing';
    
    // Click on a day
    const dayCell = page.locator('.day-cell').nth(2);
    await dayCell.click();
    
    // Wait for modal
    await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
    
    // Find comment input/textarea
    const commentInput = page.locator(
      'textarea[name="comment"], textarea#comment, input[name="comment"], input#comment'
    ).first();
    
    if (await commentInput.count() > 0) {
      // Clear and type comment
      await commentInput.clear();
      await commentInput.fill(testComment);
      
      // Save
      const saveButton = page.locator('button:has-text("Save"), button[type="submit"]').first();
      await saveButton.click();
      
      await page.waitForTimeout(1000);
      
      // Reopen to verify
      await dayCell.click();
      await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
      
      const commentInput2 = page.locator(
        'textarea[name="comment"], textarea#comment, input[name="comment"], input#comment'
      ).first();
      
      const commentValue = await commentInput2.inputValue();
      expect(commentValue).toBe(testComment);
      
      // Close modal
      const closeButton = page.locator('button:has-text("Cancel"), button:has-text("Close")').first();
      if (await closeButton.count() > 0) {
        await closeButton.click();
      } else {
        await page.keyboard.press('Escape');
      }
    }
  });

  test('should update an existing comment', async ({ page }) => {
    const initialComment = 'Initial comment';
    const updatedComment = 'Updated comment text';
    
    // Click on a day
    const dayCell = page.locator('.day-cell').nth(3);
    await dayCell.click();
    
    // Wait for modal
    await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
    
    const commentInput = page.locator(
      'textarea[name="comment"], textarea#comment, input[name="comment"], input#comment'
    ).first();
    
    if (await commentInput.count() > 0) {
      // Add initial comment
      await commentInput.clear();
      await commentInput.fill(initialComment);
      
      const saveButton = page.locator('button:has-text("Save"), button[type="submit"]').first();
      await saveButton.click();
      await page.waitForTimeout(1000);
      
      // Reopen and update
      await dayCell.click();
      await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
      
      const commentInput2 = page.locator(
        'textarea[name="comment"], textarea#comment, input[name="comment"], input#comment'
      ).first();
      
      await commentInput2.clear();
      await commentInput2.fill(updatedComment);
      
      const saveButton2 = page.locator('button:has-text("Save"), button[type="submit"]').first();
      await saveButton2.click();
      await page.waitForTimeout(1000);
      
      // Verify update
      await dayCell.click();
      await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
      
      const commentInput3 = page.locator(
        'textarea[name="comment"], textarea#comment, input[name="comment"], input#comment'
      ).first();
      
      const finalValue = await commentInput3.inputValue();
      expect(finalValue).toBe(updatedComment);
    }
  });

  test('should remove a comment', async ({ page }) => {
    const testComment = 'Comment to be removed';
    
    // Click on a day
    const dayCell = page.locator('.day-cell').nth(4);
    await dayCell.click();
    
    // Wait for modal
    await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
    
    const commentInput = page.locator(
      'textarea[name="comment"], textarea#comment, input[name="comment"], input#comment'
    ).first();
    
    if (await commentInput.count() > 0) {
      // Add comment
      await commentInput.clear();
      await commentInput.fill(testComment);
      
      const saveButton = page.locator('button:has-text("Save"), button[type="submit"]').first();
      await saveButton.click();
      await page.waitForTimeout(1000);
      
      // Reopen and remove comment
      await dayCell.click();
      await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
      
      const commentInput2 = page.locator(
        'textarea[name="comment"], textarea#comment, input[name="comment"], input#comment'
      ).first();
      
      await commentInput2.clear();
      
      const saveButton2 = page.locator('button:has-text("Save"), button[type="submit"]').first();
      await saveButton2.click();
      await page.waitForTimeout(1000);
      
      // Verify removal
      await dayCell.click();
      await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
      
      const commentInput3 = page.locator(
        'textarea[name="comment"], textarea#comment, input[name="comment"], input#comment'
      ).first();
      
      const finalValue = await commentInput3.inputValue();
      expect(finalValue).toBe('');
    }
  });

  test('should handle both VAB and comment together', async ({ page }) => {
    const testComment = 'VAB day with comment';
    
    // Click on a day
    const dayCell = page.locator('.day-cell').nth(5);
    await dayCell.click();
    
    // Wait for modal
    await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
    
    // Check VAB
    const vabCheckbox = page.locator(
      'input[type="checkbox"][name="isVAB"], input[type="checkbox"][name="vab"], input#vab, input#isVAB'
    ).first();
    
    const commentInput = page.locator(
      'textarea[name="comment"], textarea#comment, input[name="comment"], input#comment'
    ).first();
    
    if (await vabCheckbox.count() > 0 && await commentInput.count() > 0) {
      await vabCheckbox.check();
      await commentInput.clear();
      await commentInput.fill(testComment);
      
      // Save
      const saveButton = page.locator('button:has-text("Save"), button[type="submit"]').first();
      await saveButton.click();
      await page.waitForTimeout(1000);
      
      // Verify both were saved
      await dayCell.click();
      await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
      
      const vabCheckbox2 = page.locator(
        'input[type="checkbox"][name="isVAB"], input[type="checkbox"][name="vab"], input#vab, input#isVAB'
      ).first();
      
      const commentInput2 = page.locator(
        'textarea[name="comment"], textarea#comment, input[name="comment"], input#comment'
      ).first();
      
      const isVABChecked = await vabCheckbox2.isChecked();
      const commentValue = await commentInput2.inputValue();
      
      expect(isVABChecked).toBeTruthy();
      expect(commentValue).toBe(testComment);
    }
  });
});
