import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import ProtectedRoute from './ProtectedRoute';
import { AuthContext } from '../auth/AuthContext';

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
  logout: vi.fn(),
  refreshUser: vi.fn(),
});

const renderWithRouter = (
  component: React.ReactElement,
  authContext: MockAuthContext,
  initialRoute = '/protected'
) => {
  return render(
    <MemoryRouter initialEntries={[initialRoute]}>
      <AuthContext.Provider value={authContext}>
        <Routes>
          <Route path="/login" element={<div>Login Page</div>} />
          <Route path="/protected" element={component} />
        </Routes>
      </AuthContext.Provider>
    </MemoryRouter>
  );
};

describe('ProtectedRoute', () => {
  it('shows a loading state while /me bootstrap is pending', () => {
    const authContext = { ...createMockAuthContext(false), isLoading: true };

    renderWithRouter(
      <ProtectedRoute><div>Protected Content</div></ProtectedRoute>,
      authContext,
    );

    expect(screen.getByRole('progressbar')).toBeInTheDocument();
    expect(screen.queryByText('Login Page')).not.toBeInTheDocument();
  });

  it('should render children when user is authenticated', () => {
    const authContext = createMockAuthContext(true, {
      id: 1,
      email: 'test@test.com',
      role: 'ParentA',
      displayName: 'Test User',
    });

    renderWithRouter(
      <ProtectedRoute>
        <div>Protected Content</div>
      </ProtectedRoute>,
      authContext
    );

    expect(screen.getByText('Protected Content')).toBeInTheDocument();
  });

  it('should redirect to /login when user is not authenticated', () => {
    const authContext = createMockAuthContext(false);

    renderWithRouter(
      <ProtectedRoute>
        <div>Protected Content</div>
      </ProtectedRoute>,
      authContext
    );

    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
    expect(screen.getByText('Login Page')).toBeInTheDocument();
  });

  it('should render children when user has Admin role', () => {
    const authContext = createMockAuthContext(true, {
      id: 1,
      email: 'admin@test.com',
      role: 'Admin',
      displayName: 'Admin User',
    });

    renderWithRouter(
      <ProtectedRoute requireAdmin>
        <div>Admin Content</div>
      </ProtectedRoute>,
      authContext
    );

    expect(screen.getByText('Admin Content')).toBeInTheDocument();
  });

  it('should redirect to / when user is not Admin but Admin is required', () => {
    const authContext = createMockAuthContext(true, {
      id: 1,
      email: 'test@test.com',
      role: 'ParentA',
      displayName: 'Test User',
    });

    render(
      <MemoryRouter initialEntries={['/admin']}>
        <AuthContext.Provider value={authContext}>
          <Routes>
            <Route path="/" element={<div>Home Page</div>} />
            <Route
              path="/admin"
              element={
                <ProtectedRoute requireAdmin>
                  <div>Admin Content</div>
                </ProtectedRoute>
              }
            />
          </Routes>
        </AuthContext.Provider>
      </MemoryRouter>
    );

    expect(screen.queryByText('Admin Content')).not.toBeInTheDocument();
    expect(screen.getByText('Home Page')).toBeInTheDocument();
  });
});
