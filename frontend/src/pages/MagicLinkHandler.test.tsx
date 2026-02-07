import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import MagicLinkHandler from './MagicLinkHandler';
import { AuthContext } from '../auth/AuthContext';

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

describe('MagicLinkHandler', () => {
  const mockExchangeMagicToken = vi.fn();

  const mockAuthContext = {
    token: null,
    user: null,
    isAuthenticated: false,
    loginAdmin: vi.fn(),
    exchangeMagicToken: mockExchangeMagicToken,
    logout: vi.fn(),
    authHeader: vi.fn(() => ({})),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  const renderWithToken = (token: string | null) => {
    const searchParams = token ? `?token=${token}` : '';
    return render(
      <MemoryRouter initialEntries={[`/auth/magic${searchParams}`]}>
        <AuthContext.Provider value={mockAuthContext}>
          <MagicLinkHandler />
        </AuthContext.Provider>
      </MemoryRouter>
    );
  };

  it('should show loading spinner initially', () => {
    mockExchangeMagicToken.mockImplementation(() => new Promise(() => {})); // Never resolves
    renderWithToken('validtoken123');
    
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
    expect(screen.getByText(/loggar.*in/i)).toBeInTheDocument();
  });

  it('should extract token from URL query parameter and call exchangeMagicToken', async () => {
    mockExchangeMagicToken.mockResolvedValue(true);
    renderWithToken('abc123token');
    
    await waitFor(() => {
      expect(mockExchangeMagicToken).toHaveBeenCalledWith('abc123token');
    });
  });

  it('should redirect to home page after successful token exchange', async () => {
    mockExchangeMagicToken.mockResolvedValue(true);
    renderWithToken('validtoken');
    
    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/', { replace: true });
    });
  });

  it('should show error message when token is missing from URL', async () => {
    renderWithToken(null);
    
    await waitFor(() => {
      expect(screen.getByText(/ogiltig.*utgången/i)).toBeInTheDocument();
    });
    
    expect(mockExchangeMagicToken).not.toHaveBeenCalled();
  });

  it('should show error message when token exchange fails', async () => {
    mockExchangeMagicToken.mockResolvedValue(false);
    renderWithToken('invalidtoken');
    
    await waitFor(() => {
      expect(screen.getByText(/ogiltig.*utgången/i)).toBeInTheDocument();
    });
  });

  it('should display user-friendly error message with guidance', async () => {
    renderWithToken(null);
    
    await waitFor(() => {
      expect(screen.getByText(/begär.*ny.*länk/i)).toBeInTheDocument();
    });
  });

  it('should only call exchangeMagicToken once on mount', async () => {
    mockExchangeMagicToken.mockResolvedValue(true);
    const { rerender } = renderWithToken('token123');
    
    await waitFor(() => {
      expect(mockExchangeMagicToken).toHaveBeenCalledTimes(1);
    });
    
    // Rerender should not call again
    rerender(
      <MemoryRouter initialEntries={['/auth/magic?token=token123']}>
        <AuthContext.Provider value={mockAuthContext}>
          <MagicLinkHandler />
        </AuthContext.Provider>
      </MemoryRouter>
    );
    
    // Still only called once
    expect(mockExchangeMagicToken).toHaveBeenCalledTimes(1);
  });

  it('should handle network errors gracefully', async () => {
    mockExchangeMagicToken.mockRejectedValue(new Error('Network error'));
    renderWithToken('token123');
    
    await waitFor(() => {
      expect(screen.getByText(/ogiltig.*utgången/i)).toBeInTheDocument();
    });
  });

  it('should use Material UI components (CircularProgress, Typography)', () => {
    mockExchangeMagicToken.mockImplementation(() => new Promise(() => {}));
    renderWithToken('token123');
    
    // Check for MUI CircularProgress
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('should show error state with Material UI Typography', async () => {
    renderWithToken(null);
    
    await waitFor(() => {
      const errorText = screen.getByText(/ogiltig.*utgången/i);
      expect(errorText).toBeInTheDocument();
    });
  });
});
