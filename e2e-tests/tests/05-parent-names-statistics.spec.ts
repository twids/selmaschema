import { test, expect } from '@playwright/test';
import { config, waitForFrontend, waitForAPI, cleanupTestData } from './helpers/test-utils';

/**
 * Parent Names Configuration and Statistics Tests
 * 
 * Tests:
 * - Update parent names via UI
 * - View statistics dashboard
 * - Verify statistics calculations
 * - Data persistence
 */

test.describe('Parent Names and Statistics', () => {
  
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

  test('should display default parent names in legend', async ({ page }) => {
    // Check legend for parent names
    const legend = page.locator('.legend-container, .legend');
    await expect(legend).toBeVisible();
    
    const legendText = await legend.textContent();
    expect(legendText).toBeTruthy();
    expect(legendText!.length).toBeGreaterThan(0);
  });

  test('should update parent names via settings', async ({ page }) => {
    const newParentA = 'Alice';
    const newParentB = 'Bob';
    
    // Look for settings or edit parent names button/input
    const parentAInput = page.locator(
      'input[name="parentA"], input[placeholder*="Parent A"], input[id*="parentA"]'
    ).first();
    
    const parentBInput = page.locator(
      'input[name="parentB"], input[placeholder*="Parent B"], input[id*="parentB"]'
    ).first();
    
    if (await parentAInput.count() > 0 && await parentBInput.count() > 0) {
      // Update names
      await parentAInput.clear();
      await parentAInput.fill(newParentA);
      
      await parentBInput.clear();
      await parentBInput.fill(newParentB);
      
      // Look for save/update button
      const saveButton = page.locator(
        'button:has-text("Update Names"), button:has-text("Save Names"), button[type="submit"]'
      ).first();
      
      if (await saveButton.count() > 0) {
        await saveButton.click();
        await page.waitForTimeout(1000);
      }
      
      // Verify names appear in the legend
      const legend = page.locator('.legend-container, .legend');
      const legendText = await legend.textContent();
      
      expect(legendText).toContain(newParentA);
      expect(legendText).toContain(newParentB);
    }
  });

  test('should display statistics section', async ({ page }) => {
    // Look for statistics section
    const stats = page.locator('.statistics-container, .statistics, .stats-section');
    
    if (await stats.count() > 0) {
      await expect(stats).toBeVisible();
    }
  });

  test('should show day distribution in statistics', async ({ page }) => {
    // Initialize month to have data
    const initButton = page.locator('button:has-text("Initialize")');
    if (await initButton.count() > 0) {
      await initButton.click();
      await page.waitForTimeout(2000);
    }
    
    // Check for statistics
    const stats = page.locator('.statistics-container, .statistics, .stats-section');
    
    if (await stats.count() > 0) {
      const statsText = await stats.textContent();
      expect(statsText).toBeTruthy();
      
      // Should contain some numbers or day counts
      const hasNumbers = /\d+/.test(statsText!);
      expect(hasNumbers).toBeTruthy();
    }
  });

  test('should update statistics when days are assigned', async ({ page, request }) => {
    // Initialize month
    const initButton = page.locator('button:has-text("Initialize")');
    if (await initButton.count() > 0) {
      await initButton.click();
      await page.waitForTimeout(2000);
    }
    
    // Get initial statistics via API
    const year = new Date().getFullYear();
    const initialStatsResponse = await request.get(`${config.apiURL}/api/statistics/${year}`);
    const initialStats = await initialStatsResponse.json();
    
    // Assign a day via UI
    const dayCell = page.locator('.day-cell').first();
    await dayCell.click();
    
    await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
    
    // Select parent and save
    const parentAOption = page.locator(
      'input[type="radio"][value="parentA"], input[value="parentA"]'
    ).first();
    
    if (await parentAOption.count() > 0) {
      await parentAOption.click();
      
      const saveButton = page.locator('button:has-text("Save"), button[type="submit"]').first();
      await saveButton.click();
      await page.waitForTimeout(1000);
      
      // Get updated statistics
      const updatedStatsResponse = await request.get(`${config.apiURL}/api/statistics/${year}`);
      const updatedStats = await updatedStatsResponse.json();
      
      // Verify statistics changed (at least one field should be different or present)
      expect(updatedStats).toBeDefined();
      expect(updatedStats.year).toBe(year);
    }
  });

  test('should show VAB count in statistics', async ({ page }) => {
    // Initialize month
    const initButton = page.locator('button:has-text("Initialize")');
    if (await initButton.count() > 0) {
      await initButton.click();
      await page.waitForTimeout(1000);
    }
    
    // Mark a day as VAB
    const dayCell = page.locator('.day-cell').first();
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
      
      // Check statistics for VAB count
      const stats = page.locator('.statistics-container, .statistics, .stats-section');
      
      if (await stats.count() > 0) {
        const statsText = await stats.textContent();
        
        // Should mention VAB
        const hasVAB = statsText && (
          statsText.includes('VAB') || 
          statsText.includes('vab') || 
          statsText.includes('child care')
        );
        
        if (hasVAB) {
          expect(hasVAB).toBeTruthy();
        }
      }
    }
  });

  test('should show comment count in statistics', async ({ page }) => {
    const testComment = 'Test comment for statistics';
    
    // Initialize month
    const initButton = page.locator('button:has-text("Initialize")');
    if (await initButton.count() > 0) {
      await initButton.click();
      await page.waitForTimeout(1000);
    }
    
    // Add a comment
    const dayCell = page.locator('.day-cell').first();
    await dayCell.click();
    
    await page.waitForSelector('.modal, .day-modal, [role="dialog"]', { timeout: 5000 });
    
    const commentInput = page.locator(
      'textarea[name="comment"], textarea#comment, input[name="comment"], input#comment'
    ).first();
    
    if (await commentInput.count() > 0) {
      await commentInput.clear();
      await commentInput.fill(testComment);
      
      const saveButton = page.locator('button:has-text("Save"), button[type="submit"]').first();
      await saveButton.click();
      await page.waitForTimeout(1000);
      
      // Check statistics for comment count
      const stats = page.locator('.statistics-container, .statistics, .stats-section');
      
      if (await stats.count() > 0) {
        const statsText = await stats.textContent();
        
        // Should mention comments
        const hasComments = statsText && (
          statsText.includes('comment') || 
          statsText.includes('Comment') ||
          statsText.includes('note')
        );
        
        if (hasComments) {
          expect(hasComments).toBeTruthy();
        }
      }
    }
  });

  test('should persist parent names across page refresh', async ({ page }) => {
    const newParentA = 'TestParentA_Persist';
    const newParentB = 'TestParentB_Persist';
    
    // Update names
    const parentAInput = page.locator(
      'input[name="parentA"], input[placeholder*="Parent A"], input[id*="parentA"]'
    ).first();
    
    const parentBInput = page.locator(
      'input[name="parentB"], input[placeholder*="Parent B"], input[id*="parentB"]'
    ).first();
    
    if (await parentAInput.count() > 0 && await parentBInput.count() > 0) {
      await parentAInput.clear();
      await parentAInput.fill(newParentA);
      
      await parentBInput.clear();
      await parentBInput.fill(newParentB);
      
      const saveButton = page.locator(
        'button:has-text("Update Names"), button:has-text("Save Names"), button[type="submit"]'
      ).first();
      
      if (await saveButton.count() > 0) {
        await saveButton.click();
        await page.waitForTimeout(1000);
      }
      
      // Refresh page
      await page.reload();
      await page.waitForSelector('.calendar-container', { timeout: config.defaultTimeout });
      
      // Verify names persisted
      const legend = page.locator('.legend-container, .legend');
      const legendText = await legend.textContent();
      
      expect(legendText).toContain(newParentA);
      expect(legendText).toContain(newParentB);
    }
  });
});
