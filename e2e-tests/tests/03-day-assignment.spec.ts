import { test, expect, type Locator, type Page } from '@playwright/test';
import { config, waitForFrontend, waitForAPI, cleanupTestData } from './helpers/test-utils';

async function waitForCalendar(page: Page) {
  await expect(page.getByRole('heading', { name: 'Kalender' })).toBeVisible();
  await expect(page.locator('[data-testid^="day-cell-"]').first()).toBeVisible();
}

async function openDay(page: Page, index = 0): Promise<Locator> {
  const day = page.locator('[data-testid^="day-cell-"]').nth(index);
  await day.click();
  await expect(page.getByTestId('day-modal')).toBeVisible();
  return day;
}

async function selectParent(page: Page, parentName: string) {
  const modal = page.getByTestId('day-modal');
  await modal.getByRole('combobox', { name: /Tilldelad till/ }).click();
  await page.getByRole('option', { name: parentName, exact: true }).click();
}

test.describe('Day Assignment Operations', () => {
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

  test('should initialize the month with the default pattern', async ({ page }) => {
    await page.getByRole('button', { name: 'Initiera månad' }).click();
    await expect(
      page.locator('[data-testid^="day-cell-"]').filter({ hasText: /Parent A|Parent B/ }).first(),
    ).toBeVisible();
  });

  test('should open the day dialog', async ({ page }) => {
    await openDay(page);
    const modal = page.getByTestId('day-modal');
    await expect(modal.getByRole('checkbox', { name: 'Markera som VAB' })).toBeVisible();
    await expect(modal.getByRole('button', { name: 'Spara' })).toBeVisible();
    await expect(modal.getByRole('button', { name: 'Avbryt' })).toBeVisible();
  });

  test('should assign a day to Parent A', async ({ page }) => {
    const day = await openDay(page, 0);
    await selectParent(page, 'Parent A');
    await page.getByTestId('day-modal').getByRole('button', { name: 'Spara' }).click();

    await expect(page.getByTestId('day-modal')).toBeHidden();
    await expect(day).toContainText('Parent A');
  });

  test('should assign a day to Parent B', async ({ page }) => {
    const day = await openDay(page, 1);
    await selectParent(page, 'Parent B');
    await page.getByTestId('day-modal').getByRole('button', { name: 'Spara' }).click();

    await expect(page.getByTestId('day-modal')).toBeHidden();
    await expect(day).toContainText('Parent B');
  });

  test('should close the dialog without saving changes', async ({ page }) => {
    const day = await openDay(page, 0);
    await selectParent(page, 'Parent B');
    await page.getByTestId('day-modal').getByRole('button', { name: 'Avbryt' }).click();
    await expect(page.getByTestId('day-modal')).toBeHidden();

    await day.click();
    await expect(page.getByTestId('day-modal').getByRole('combobox', { name: /Tilldelad till/ }))
      .toHaveText('Parent A');
  });
});
