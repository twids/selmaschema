import { test, expect, type Page } from '@playwright/test';
import { config, waitForFrontend, waitForAPI, cleanupTestData } from './helpers/test-utils';

async function waitForCalendar(page: Page) {
  await expect(page.getByRole('heading', { name: 'Kalender' })).toBeVisible();
  await expect(page.locator('[data-testid^="day-cell-"]').first()).toBeVisible();
}

test.describe('Parent Names and Statistics', () => {
  test.beforeAll(async () => {
    await waitForAPI();
    await waitForFrontend();
  });

  test.beforeEach(async ({ page }) => {
    await page.goto(config.baseURL);
    await waitForCalendar(page);
  });

  test.afterAll(async () => {
    await cleanupTestData();
  });

  test('should display parent names in the legend and editor', async ({ page }) => {
    await expect(page.getByText('Parent A', { exact: true }).first()).toBeVisible();
    await expect(page.getByText('Parent B', { exact: true }).first()).toBeVisible();
    await expect(page.getByRole('textbox', { name: 'Förälder A' })).toHaveValue('Parent A');
    await expect(page.getByRole('textbox', { name: 'Förälder B' })).toHaveValue('Parent B');
  });

  test('should update and persist parent names', async ({ page }) => {
    const editor = page.getByTestId('parent-name-editor');
    await editor.getByRole('textbox', { name: 'Förälder A' }).fill('Alice');
    await editor.getByRole('textbox', { name: 'Förälder B' }).fill('Bob');
    await editor.getByRole('button', { name: 'Spara' }).click();
    await expect(page.getByText('Namn uppdaterade')).toBeVisible();
    await expect(page.getByText('Alice', { exact: true }).first()).toBeVisible();
    await expect(page.getByText('Bob', { exact: true }).first()).toBeVisible();

    await page.reload();
    await waitForCalendar(page);
    await expect(page.getByRole('textbox', { name: 'Förälder A' })).toHaveValue('Alice');
    await expect(page.getByRole('textbox', { name: 'Förälder B' })).toHaveValue('Bob');
  });

  test('should display all yearly statistics', async ({ page }) => {
    const statistics = page.getByTestId('statistics');
    await expect(statistics).toBeVisible();
    await expect(statistics.getByText('VAB', { exact: true })).toBeVisible();
    await expect(statistics.getByText('Kommentarer', { exact: true })).toBeVisible();
    await expect(statistics.getByText('Ej tilldelade', { exact: true })).toBeVisible();
    await expect(statistics.getByRole('heading')).toHaveCount(5);
  });
});
