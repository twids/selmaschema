import { test, expect, type Page } from '@playwright/test';
import { readFile } from 'node:fs/promises';
import {
  config,
  waitForFrontend,
  waitForAPI,
  cleanupTestData,
  getDateKey,
  getCurrentYearMonth,
} from './helpers/test-utils';

async function waitForCalendar(page: Page) {
  await expect(page.getByRole('heading', { name: 'Kalender' })).toBeVisible();
  await expect(page.locator('[data-testid^="day-cell-"]').first()).toBeVisible();
}

function importPayload() {
  const { year, month } = getCurrentYearMonth();
  const dateKey = getDateKey(year, month, 1);
  return {
    dateKey,
    data: {
      calendarData: {
        [dateKey]: {
          id: 0,
          date: dateKey,
          parent: 'A',
          isVAB: false,
          specialStatus: null,
          parentAComments: [],
          parentBComments: [],
        },
      },
      parentNames: { parentAName: 'Parent A', parentBName: 'Parent B' },
      exportDate: new Date().toISOString(),
      year,
      month,
    },
  };
}

test.describe('Import/Export Functionality', () => {
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

  test('should show import and export controls', async ({ page }) => {
    const controls = page.getByTestId('import-export');
    await expect(controls.getByRole('button', { name: 'Export' })).toBeVisible();
    await expect(controls.getByRole('button', { name: 'Import' })).toBeVisible();
  });

  test('should export the current calendar as JSON', async ({ page }) => {
    const downloadPromise = page.waitForEvent('download');
    await page.getByRole('button', { name: 'Export' }).click();
    const download = await downloadPromise;

    expect(download.suggestedFilename()).toMatch(/^calendar-export-\d{4}-\d{1,2}\.json$/);
    const downloadPath = await download.path();
    expect(downloadPath).not.toBeNull();
    const exported = JSON.parse(await readFile(downloadPath!, 'utf8'));
    expect(exported).toEqual(expect.objectContaining({
      calendarData: expect.any(Object),
      parentNames: expect.objectContaining({
        parentAName: expect.any(String),
        parentBName: expect.any(String),
      }),
      exportDate: expect.any(String),
      year: expect.any(Number),
      month: expect.any(Number),
    }));
  });

  test('should validate an import before applying it', async ({ page }) => {
    const { data } = importPayload();
    await page.locator('input[type="file"]').setInputFiles({
      name: 'calendar.json',
      mimeType: 'application/json',
      buffer: Buffer.from(JSON.stringify(data)),
    });

    await expect(page.getByRole('heading', { name: 'Bekräfta import' })).toBeVisible();
    await expect(page.getByText('1 dag(ar) kommer att uppdateras.')).toBeVisible();
    await page.getByRole('button', { name: 'Avbryt' }).click();
  });

  test('should reject an invalid import file', async ({ page }) => {
    await page.locator('input[type="file"]').setInputFiles({
      name: 'invalid.json',
      mimeType: 'application/json',
      buffer: Buffer.from('{"invalid":true}'),
    });

    await expect(page.getByRole('alert')).toContainText('Invalid file structure');
  });

  test('should import a valid day assignment', async ({ page }) => {
    const { data, dateKey } = importPayload();
    await page.locator('input[type="file"]').setInputFiles({
      name: 'calendar.json',
      mimeType: 'application/json',
      buffer: Buffer.from(JSON.stringify(data)),
    });
    await page.getByRole('button', { name: 'Bekräfta' }).click();

    await expect(page.getByText('Import lyckades!')).toBeVisible();
    await expect(page.getByTestId(`day-cell-${dateKey}`)).toContainText('Parent A');
  });
});
