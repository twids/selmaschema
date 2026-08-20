import { test, expect, type Page } from '@playwright/test';
import { config, waitForFrontend, waitForAPI, cleanupTestData } from './helpers/test-utils';

const monthNames = [
  'Januari', 'Februari', 'Mars', 'April', 'Maj', 'Juni',
  'Juli', 'Augusti', 'September', 'Oktober', 'November', 'December',
];

async function waitForCalendar(page: Page) {
  await expect(page.getByRole('heading', { name: 'Kalender' })).toBeVisible();
  await expect(page.locator('[data-testid^="day-cell-"]').first()).toBeVisible();
}

test.describe('Calendar Navigation', () => {
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

  test('should load the application with navigation and calendar controls', async ({ page }) => {
    await expect(page.getByRole('link', { name: 'Kalender' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Byten' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Inbjudningar' })).toBeVisible();
    await expect(page.locator('[aria-label="Månad"]')).toBeVisible();
    await expect(page.locator('[aria-label="År"]')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Föregående månad' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Nästa månad' })).toBeVisible();
  });

  test('should display the current month and year by default', async ({ page }) => {
    const now = new Date();
    await expect(page.locator('[aria-label="Månad"]'))
      .toHaveText(monthNames[now.getMonth()]);
    await expect(page.locator('[aria-label="År"]'))
      .toHaveText(String(now.getFullYear()));
  });

  test('should navigate to the next and previous month', async ({ page }) => {
    const monthSelect = page.locator('[aria-label="Månad"]');
    const initialMonth = await monthSelect.textContent();

    await page.getByRole('button', { name: 'Nästa månad' }).click();
    await expect(monthSelect).not.toHaveText(initialMonth ?? '');

    await page.getByRole('button', { name: 'Föregående månad' }).click();
    await expect(monthSelect).toHaveText(initialMonth ?? '');
  });

  test('should display every day in the selected month', async ({ page }) => {
    const now = new Date();
    const expectedDays = new Date(now.getFullYear(), now.getMonth() + 1, 0).getDate();
    await expect(page.locator('[data-testid^="day-cell-"]')).toHaveCount(expectedDays);
  });

  test('should display the legend and month actions', async ({ page }) => {
    await expect(page.getByText('Parent A', { exact: true }).first()).toBeVisible();
    await expect(page.getByText('Parent B', { exact: true }).first()).toBeVisible();
    await expect(page.getByText('Ej tilldelad', { exact: true }).first()).toBeVisible();
    await expect(page.getByRole('button', { name: 'Initiera månad' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Export' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Import' })).toBeVisible();
  });

  test('should change year and month via selectors', async ({ page }) => {
    const now = new Date();
    const targetYear = String(now.getFullYear() + 1);
    const targetMonthIndex = now.getMonth() === 5 ? 6 : 5;
    const targetMonth = monthNames[targetMonthIndex];

    const yearSelect = page.locator('[aria-label="År"]');
    await yearSelect.getByRole('combobox').click();
    await page.getByRole('option', { name: targetYear, exact: true }).click();
    await expect(yearSelect).toHaveText(targetYear);

    const monthSelect = page.locator('[aria-label="Månad"]');
    await monthSelect.getByRole('combobox').click();
    await page.getByRole('option', { name: targetMonth, exact: true }).click();
    await expect(monthSelect).toHaveText(targetMonth);
  });
});
