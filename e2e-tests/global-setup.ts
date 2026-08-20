import { mkdir } from 'node:fs/promises';
import { dirname } from 'node:path';
import { request } from '@playwright/test';

const authFile = 'test-results/.auth/admin.json';

export default async function globalSetup(): Promise<void> {
  const apiURL = process.env.API_URL || 'http://localhost:8080';
  const password = process.env.E2E_ADMIN_PASSWORD;
  if (!password) {
    throw new Error('E2E_ADMIN_PASSWORD must be configured');
  }

  await mkdir(dirname(authFile), { recursive: true });
  const api = await request.newContext({ baseURL: apiURL });
  try {
    const response = await api.post('/api/auth/admin/login', {
      data: { password },
    });
    if (!response.ok()) {
      throw new Error(`Local admin bootstrap failed with HTTP ${response.status()}`);
    }

    await api.storageState({ path: authFile });
  } finally {
    await api.dispose();
  }
}
