import { test, expect } from '@playwright/test';

const baseURL = process.env.BASE_URL || 'http://localhost:3000';
const apiURL = process.env.API_URL || baseURL;

function futureDate(days: number): string {
  const value = new Date();
  value.setDate(value.getDate() + days);
  return value.toISOString().slice(0, 10);
}

test('creates a family, configures schedules, joins a parent, and audits admin support', async ({ page, browser, playwright }, testInfo) => {
  const runId = `${Date.now()}-${testInfo.retry}`;
  const familyName = `Familjen E2E ${runId}`;
  const ownerLogin = await page.context().request.post(`${apiURL}/api/auth/test-login`, {
    data: { email: `owner-${runId}@e2e.invalid`, displayName: 'E2E Owner' },
    headers: { 'X-Selma-E2E-Secret': process.env.E2E_LOGIN_SECRET || 'e2e-login-secret' },
  });
  expect(ownerLogin.ok()).toBeTruthy();

  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'Välkommen till Selma' })).toBeVisible();
  await page.getByLabel('Familjens namn').fill(familyName);
  await page.getByRole('button', { name: 'Skapa familj' }).click();
  await expect(page.getByRole('heading', { name: familyName })).toBeVisible();

  await page.getByRole('link', { name: 'Inställningar' }).click();
  await page.getByLabel('Visningsnamn').fill('Barnet');
  await page.getByRole('button', { name: 'Lägg till barn' }).click();
  await expect(page.getByText(/Barnet · Boendeschema/)).toBeVisible();
  await page.getByLabel('Namn', { exact: true }).fill('Lovschema');
  await page.getByRole('button', { name: 'Ny kalender' }).click();
  await expect(page.getByText(/Lovschema · 0 barn/)).toBeVisible();

  await page.getByRole('link', { name: 'Översikt' }).click();
  await page.getByText('Boendeschema', { exact: true }).click();
  await page.getByRole('button', { name: 'Förhandsvisa 6 veckor' }).click();
  await expect(page.getByText(/Förhandsvisning giltig/)).toBeVisible();
  await page.getByRole('button', { name: 'Aktivera från valt datum' }).click();

  await page.getByLabel('Mall').click();
  await page.getByRole('option', { name: '2-2-3' }).click();
  await page.getByLabel('Gäller från').fill(futureDate(14));
  await page.getByRole('button', { name: 'Förhandsvisa 6 veckor' }).click();
  await page.getByRole('button', { name: 'Aktivera från valt datum' }).click();

  await page.getByRole('link', { name: 'Bjud in' }).click();
  await page.getByLabel('Schemasida').click();
  await page.getByRole('option', { name: 'Hem B' }).click();
  await page.getByLabel('E-postledtråd (valfri)').fill('other@e2e.invalid');
  await page.getByRole('button', { name: 'Skapa inbjudan' }).click();
  const code = await page.getByText(/^[A-Z2-9]{4}-[A-Z2-9]{4}$/).textContent();
  expect(code).toBeTruthy();

  const secondApi = await playwright.request.newContext({ baseURL: apiURL });
  const login = await secondApi.post('/api/auth/test-login', {
    data: { email: `second-${runId}@e2e.invalid`, displayName: 'E2E Parent' },
    headers: { 'X-Selma-E2E-Secret': process.env.E2E_LOGIN_SECRET || 'e2e-login-secret' },
  });
  expect(login.ok()).toBeTruthy();
  const secondState = await secondApi.storageState();
  await secondApi.dispose();
  const secondContext = await browser.newContext({ baseURL, storageState: secondState });
  const secondPage = await secondContext.newPage();
  await secondPage.goto('/join');
  await secondPage.getByLabel('Inbjudningskod').fill(code!);
  await secondPage.getByRole('button', { name: 'Gå med' }).click();
  await expect(secondPage.getByText('Din verifierade adress:')).toBeVisible();
  await secondPage.getByRole('button', { name: 'Bekräfta och gå med' }).click();
  await expect(secondPage.getByRole('heading', { name: familyName })).toBeVisible();
  await secondContext.close();

  await page.goto('/admin/login');
  await page.getByRole('button', { name: 'Reservåtkomst' }).click();
  await page.getByLabel('Reservlösenord').fill(process.env.E2E_ADMIN_PASSWORD || 'e2e-admin-password');
  await page.getByRole('button', { name: 'Logga in med reservåtkomst' }).click();
  await expect(page.getByRole('heading', { name: 'Familjer' })).toBeVisible();
  await page.getByText(familyName, { exact: true }).click();
  await page.getByLabel('Obligatorisk orsak').fill('E2E verifiering av auditerad support');
  await page.getByRole('button', { name: 'Stäng av familj' }).click();
  await expect(page.getByLabel('Status')).toHaveText('Avstängd');
  await expect(page.getByText(/PlatformAdmin.FamilyUpdated/)).toBeVisible();
});
