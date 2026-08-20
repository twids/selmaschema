import { test, expect, type Locator, type Page } from '@playwright/test';
import { config, waitForFrontend, waitForAPI, cleanupTestData } from './helpers/test-utils';

async function waitForCalendar(page: Page) {
  await expect(page.getByRole('heading', { name: 'Kalender' })).toBeVisible();
  await expect(page.locator('[data-testid^="day-cell-"]').first()).toBeVisible();
}

async function openDay(page: Page, index: number): Promise<Locator> {
  const day = page.locator('[data-testid^="day-cell-"]').nth(index);
  await day.click();
  await expect(page.getByTestId('day-modal')).toBeVisible();
  return day;
}

async function saveDay(page: Page) {
  await page.getByTestId('day-modal').getByRole('button', { name: 'Spara' }).click();
  await expect(page.getByTestId('day-modal')).toBeHidden();
}

test.describe('VAB and Day Details', () => {
  test.beforeAll(async () => {
    await waitForAPI();
    await waitForFrontend();
  });

  test.beforeEach(async ({ page }) => {
    await page.goto(config.baseURL);
    await waitForCalendar(page);
    await page.getByRole('button', { name: 'Initiera månad' }).click();
    await expect(
      page.locator('[data-testid^="day-cell-"]').filter({ hasText: /Parent A|Parent B/ }).first(),
    ).toBeVisible();
  });

  test.afterAll(async () => {
    await cleanupTestData();
  });

  test('should mark a day as VAB and persist it', async ({ page }) => {
    const day = await openDay(page, 2);
    const checkbox = page.getByRole('checkbox', { name: 'Markera som VAB' });
    await checkbox.check();
    await saveDay(page);
    await expect(day).toContainText('VAB');

    await day.click();
    await expect(page.getByRole('checkbox', { name: 'Markera som VAB' })).toBeChecked();
  });

  test('should unmark a VAB day', async ({ page }) => {
    const day = await openDay(page, 2);
    const checkbox = page.getByRole('checkbox', { name: 'Markera som VAB' });
    await checkbox.check();
    await saveDay(page);

    await day.click();
    await page.getByRole('checkbox', { name: 'Markera som VAB' }).uncheck();
    await saveDay(page);
    await expect(day).not.toContainText('VAB');
  });

  test('should persist a special status', async ({ page }) => {
    const day = await openDay(page, 3);
    const status = page.getByTestId('day-modal').getByRole('combobox', { name: /Special status/ });
    await status.click();
    await page.getByRole('option', { name: 'Helgdag', exact: true }).click();
    await saveDay(page);

    await day.click();
    await expect(page.getByTestId('day-modal').getByRole('combobox', { name: /Special status/ }))
      .toHaveText('Helgdag');
  });

  test('should show separate comment areas for both parents', async ({ page }) => {
    await openDay(page, 4);
    await expect(page.getByText('Kommentarer – Parent A', { exact: true })).toBeVisible();
    await expect(page.getByText('Kommentarer – Parent B', { exact: true })).toBeVisible();
    await expect(page.getByPlaceholder('Lägg till kommentar...')).toHaveCount(2);
  });
});
