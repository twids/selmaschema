import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import { theme } from '../theme';
import AppLayout from './AppLayout';
import { AuthContext } from '../auth/AuthContext';

const mockLogout = vi.fn();
type MockAuthContext = NonNullable<React.ContextType<typeof AuthContext>>;

const createMockAuthContext = (
  isAuthenticated: boolean,
  user: MockAuthContext['user'] = null
): MockAuthContext => ({
  user,
  isAuthenticated,
  isLoading: false,
  loginAdmin: vi.fn(),
  startOidcLogin: vi.fn(),
  completeInvitation: vi.fn(),
  logout: mockLogout,
  refreshUser: vi.fn(),
});

const renderWithProviders = (
  component: React.ReactElement,
  authContext: MockAuthContext
) => {
  return render(
    <BrowserRouter>
      <ThemeProvider theme={theme}>
        <AuthContext.Provider value={authContext}>
          {component}
        </AuthContext.Provider>
      </ThemeProvider>
    </BrowserRouter>
  );
};

describe('AppLayout', () => {
  beforeEach(() => {
    mockLogout.mockClear();
  });

  it('should render AppBar with app title', () => {
    const authContext = createMockAuthContext(true, {
      id: 1,
      email: 'test@test.com',
      role: 'ParentA',
      displayName: 'Test User',
    });

    renderWithProviders(<AppLayout><div>Content</div></AppLayout>, authContext);

    expect(screen.getByText(/Co-Parenting/i)).toBeInTheDocument();
  });

  it('should display user displayName when authenticated', () => {
    const authContext = createMockAuthContext(true, {
      id: 1,
      email: 'test@test.com',
      role: 'ParentA',
      displayName: 'John Doe',
    });

    renderWithProviders(<AppLayout><div>Content</div></AppLayout>, authContext);

    expect(screen.getByText('John Doe')).toBeInTheDocument();
  });

  it('should display user email when displayName is not available', () => {
    const authContext = createMockAuthContext(true, {
      id: 1,
      email: 'test@test.com',
      role: 'ParentB',
      displayName: null,
    });

    renderWithProviders(<AppLayout><div>Content</div></AppLayout>, authContext);

    expect(screen.getByText('test@test.com')).toBeInTheDocument();
  });

  it('should render logout button when authenticated', () => {
    const authContext = createMockAuthContext(true, {
      id: 1,
      email: 'test@test.com',
      role: 'Admin',
      displayName: 'Admin User',
    });

    renderWithProviders(<AppLayout><div>Content</div></AppLayout>, authContext);

    expect(screen.getByRole('button', { name: /logga ut/i })).toBeInTheDocument();
  });

  it('should render children content', () => {
    const authContext = createMockAuthContext(true, {
      id: 1,
      email: 'test@test.com',
      role: 'ParentA',
      displayName: 'Test User',
    });

    renderWithProviders(
      <AppLayout>
        <div data-testid="child-content">Test Content</div>
      </AppLayout>,
      authContext
    );

    expect(screen.getByTestId('child-content')).toBeInTheDocument();
    expect(screen.getByText('Test Content')).toBeInTheDocument();
  });

  it('should not display user info when not authenticated', () => {
    const authContext = createMockAuthContext(false);

    renderWithProviders(<AppLayout><div>Content</div></AppLayout>, authContext);

    expect(screen.queryByText('test@test.com')).not.toBeInTheDocument();
  });
});
