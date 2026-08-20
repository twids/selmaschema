import { mkdir } from 'node:fs/promises';
import { dirname } from 'node:path';
import { request } from '@playwright/test';

const authFile = 'test-results/.auth/account.json';

export default async function globalSetup(): Promise<void> {
  const apiURL = process.env.API_URL || 'http://localhost:3000';
  const loginSecret = process.env.E2E_LOGIN_SECRET || 'e2e-login-secret';
  await mkdir(dirname(authFile), { recursive: true });
  const api = await request.newContext({ baseURL: apiURL });
  try {
    for (let attempt = 0; attempt < 30; attempt += 1) {
      try {
        const health = await api.get('/api/auth/me');
        if (health.status() === 401 || health.ok()) break;
      } catch {
        if (attempt === 29) throw new Error('Selma did not become ready');
      }
      await new Promise((resolve) => setTimeout(resolve, 1000));
    }
    const response = await api.post('/api/auth/test-login', {
      data: { email: 'owner@e2e.invalid', displayName: 'E2E Owner' },
      headers: { 'X-Selma-E2E-Secret': loginSecret },
    });
    if (!response.ok()) throw new Error(`E2E login failed with HTTP ${response.status()}`);
    await api.storageState({ path: authFile });
  } finally {
    await api.dispose();
  }
}
