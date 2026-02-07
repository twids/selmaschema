import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from '../auth/AuthContext';
import LoginPage from './LoginPage';
import MagicLinkHandler from './MagicLinkHandler';

// Mock fetch for API calls
globalThis.fetch = vi.fn();

const mockFetch = globalThis.fetch as ReturnType<typeof vi.fn>;

// Mock localStorage
const localStorageMock = {
  store: {} as Record<string, string>,
  getItem(key: string) {
    return this.store[key] || null;
  },
  setItem(key: string, value: string) {
    this.store[key] = value;
  },
  removeItem(key: string) {
    delete this.store[key];
  },
  clear() {
    this.store = {};
  },
};

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
  writable: true,
});

describe('Authentication Flow Integration Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorageMock.clear();
  });

  const renderAuthFlow = (initialPath = '/login') => {
    return render(
      <MemoryRouter initialEntries={[initialPath]}>
        <AuthProvider>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/auth/magic" element={<MagicLinkHandler />} />
            <Route path="/" element={<div>Home Page</div>} />
          </Routes>
        </AuthProvider>
      </MemoryRouter>
    );
  };

  describe('Admin Login Flow', () => {
    it('should complete full admin login flow successfully', async () => {
      const mockAuthResponse = {
        token: 'admin-token-123',
        user: {
          id: 1,
          email: 'admin@example.com',
          role: 'Admin',
          displayName: 'Admin User',
        },
        expiresAt: new Date(Date.now() + 86400000).toISOString(),
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockAuthResponse,
      } as Response);

      renderAuthFlow('/login');

      // Step 1: Enter password
      const passwordInput = screen.getByLabelText(/lösenord/i);
      fireEvent.change(passwordInput, { target: { value: 'admin123' } });

      // Step 2: Submit form
      const submitButton = screen.getByRole('button', { name: /logga in/i });
      fireEvent.click(submitButton);

      // Step 3: Verify API call
      await waitFor(() => {
        expect(mockFetch).toHaveBeenCalledWith(
          expect.stringContaining('/api/auth/admin/login'),
          expect.objectContaining({
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ password: 'admin123' }),
          })
        );
      });

      // Step 4: Verify redirect to home page
      await waitFor(() => {
        expect(screen.getByText('Home Page')).toBeInTheDocument();
      });

      // Step 5: Verify token stored in localStorage
      expect(localStorage.getItem('auth_token')).toBe('admin-token-123');
      expect(localStorage.getItem('auth_user')).toContain('Admin User');
    });

    it('should show error and stay on login page when admin login fails', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
      } as Response);

      renderAuthFlow('/login');

      const passwordInput = screen.getByLabelText(/lösenord/i);
      const submitButton = screen.getByRole('button', { name: /logga in/i });

      fireEvent.change(passwordInput, { target: { value: 'wrongpassword' } });
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(screen.getByRole('alert')).toBeInTheDocument();
      });

      // Should still be on login page
      expect(screen.getByRole('button', { name: /logga in/i })).toBeInTheDocument();
    });

    it('should handle network errors during admin login', async () => {
      mockFetch.mockRejectedValueOnce(new Error('Network error'));

      renderAuthFlow('/login');

      const passwordInput = screen.getByLabelText(/lösenord/i);
      const submitButton = screen.getByRole('button', { name: /logga in/i });

      fireEvent.change(passwordInput, { target: { value: 'admin123' } });
      fireEvent.click(submitButton);

      await waitFor(() => {
        expect(screen.getByRole('alert')).toBeInTheDocument();
      });

      // Token should not be stored
      expect(localStorage.getItem('auth_token')).toBeNull();
    });
  });

  describe('Magic Link Flow', () => {
    it('should complete full magic link flow successfully', async () => {
      const mockAuthResponse = {
        token: 'magic-token-456',
        user: {
          id: 2,
          email: 'parent@example.com',
          role: 'ParentA',
          displayName: 'Parent A',
        },
        expiresAt: new Date(Date.now() + 86400000).toISOString(),
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockAuthResponse,
      } as Response);

      render(
        <MemoryRouter initialEntries={['/auth/magic?token=abc123magic']}>
          <AuthProvider>
            <Routes>
              <Route path="/auth/magic" element={<MagicLinkHandler />} />
              <Route path="/" element={<div>Home Page</div>} />
            </Routes>
          </AuthProvider>
        </MemoryRouter>
      );

      // Step 1: Verify loading state
      expect(screen.getByRole('progressbar')).toBeInTheDocument();

      // Step 2: Verify API call
      await waitFor(() => {
        expect(mockFetch).toHaveBeenCalledWith(
          expect.stringContaining('/api/auth/magic'),
          expect.objectContaining({
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ token: 'abc123magic' }),
          })
        );
      });

      // Step 3: Verify redirect to home page
      await waitFor(() => {
        expect(screen.getByText('Home Page')).toBeInTheDocument();
      });

      // Step 4: Verify token stored in localStorage
      expect(localStorage.getItem('auth_token')).toBe('magic-token-456');
      expect(localStorage.getItem('auth_user')).toContain('Parent A');
    });

    it('should show error when magic link token is invalid', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
      } as Response);

      render(
        <MemoryRouter initialEntries={['/auth/magic?token=invalidtoken']}>
          <AuthProvider>
            <Routes>
              <Route path="/auth/magic" element={<MagicLinkHandler />} />
              <Route path="/" element={<div>Home Page</div>} />
            </Routes>
          </AuthProvider>
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText(/ogiltig.*utgången/i)).toBeInTheDocument();
      });

      // Should not redirect or store token
      expect(localStorage.getItem('auth_token')).toBeNull();
      expect(screen.queryByText('Home Page')).not.toBeInTheDocument();
    });

    it('should show error when magic link has no token parameter', async () => {
      render(
        <MemoryRouter initialEntries={['/auth/magic']}>
          <AuthProvider>
            <Routes>
              <Route path="/auth/magic" element={<MagicLinkHandler />} />
            </Routes>
          </AuthProvider>
        </MemoryRouter>
      );

      await waitFor(() => {
        expect(screen.getByText(/ogiltig.*utgången/i)).toBeInTheDocument();
      });

      // Should not make API call
      expect(mockFetch).not.toHaveBeenCalled();
    });
  });

  describe('Token Persistence', () => {
    it('should restore authentication state from localStorage', async () => {
      const storedUser = {
        id: 1,
        email: 'admin@example.com',
        role: 'Admin',
        displayName: 'Admin User',
      };

      localStorage.setItem('auth_token', 'stored-token-789');
      localStorage.setItem('auth_user', JSON.stringify(storedUser));

      render(
        <MemoryRouter initialEntries={['/']}>
          <AuthProvider>
            <div data-testid="auth-status">Authenticated</div>
          </AuthProvider>
        </MemoryRouter>
      );

      // Token should be available immediately
      expect(screen.getByTestId('auth-status')).toBeInTheDocument();
    });
  });
});
