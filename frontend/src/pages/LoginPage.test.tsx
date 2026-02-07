import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import LoginPage from './LoginPage';
import { AuthContext } from '../auth/AuthContext';

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

describe('LoginPage', () => {
  const mockLoginAdmin = vi.fn();

  const mockAuthContext = {
    token: null,
    user: null,
    isAuthenticated: false,
    loginAdmin: mockLoginAdmin,
    exchangeMagicToken: vi.fn(),
    logout: vi.fn(),
    authHeader: vi.fn(() => ({})),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  const renderLoginPage = () => {
    return render(
      <BrowserRouter>
        <AuthContext.Provider value={mockAuthContext}>
          <LoginPage />
        </AuthContext.Provider>
      </BrowserRouter>
    );
  };

  it('should render login form with password field and submit button', () => {
    renderLoginPage();
    
    expect(screen.getByLabelText(/lösenord/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /logga in/i })).toBeInTheDocument();
  });

  it('should render with Material UI components (Card, TextField, Button)', () => {
    renderLoginPage();
    
    // Check for MUI TextField (has MuiFormControl-root class)
    const passwordInput = screen.getByLabelText(/lösenord/i);
    expect(passwordInput.closest('.MuiFormControl-root')).toBeInTheDocument();
    
    // Check for MUI Button
    const submitButton = screen.getByRole('button', { name: /logga in/i });
    expect(submitButton.closest('.MuiButton-root')).toBeInTheDocument();
  });

  it('should show validation error when submitting empty password', async () => {
    renderLoginPage();
    
    const submitButton = screen.getByRole('button', { name: /logga in/i });
    fireEvent.click(submitButton);
    
    // Should not call loginAdmin if password is empty
    expect(mockLoginAdmin).not.toHaveBeenCalled();
  });

  it('should call loginAdmin with password on form submission', async () => {
    mockLoginAdmin.mockResolvedValue(true);
    renderLoginPage();
    
    const passwordInput = screen.getByLabelText(/lösenord/i);
    const submitButton = screen.getByRole('button', { name: /logga in/i });
    
    fireEvent.change(passwordInput, { target: { value: 'admin123' } });
    fireEvent.click(submitButton);
    
    await waitFor(() => {
      expect(mockLoginAdmin).toHaveBeenCalledWith('admin123');
    });
  });

  it('should show loading state during submission', async () => {
    mockLoginAdmin.mockImplementation(() => new Promise(resolve => setTimeout(() => resolve(true), 100)));
    renderLoginPage();
    
    const passwordInput = screen.getByLabelText(/lösenord/i);
    const submitButton = screen.getByRole('button', { name: /logga in/i });
    
    fireEvent.change(passwordInput, { target: { value: 'admin123' } });
    fireEvent.click(submitButton);
    
    // Button should be disabled during loading
    expect(submitButton).toBeDisabled();
    
    await waitFor(() => {
      expect(submitButton).not.toBeDisabled();
    });
  });

  it('should show error alert when login fails', async () => {
    mockLoginAdmin.mockResolvedValue(false);
    renderLoginPage();
    
    const passwordInput = screen.getByLabelText(/lösenord/i);
    const submitButton = screen.getByRole('button', { name: /logga in/i });
    
    fireEvent.change(passwordInput, { target: { value: 'wrongpassword' } });
    fireEvent.click(submitButton);
    
    await waitFor(() => {
      // Should show MUI Alert with error message
      expect(screen.getByRole('alert')).toBeInTheDocument();
      expect(screen.getByText(/ogiltigt.*lösenord/i)).toBeInTheDocument();
    });
  });

  it('should redirect to home page after successful login', async () => {
    mockLoginAdmin.mockResolvedValue(true);
    renderLoginPage();
    
    const passwordInput = screen.getByLabelText(/lösenord/i);
    const submitButton = screen.getByRole('button', { name: /logga in/i });
    
    fireEvent.change(passwordInput, { target: { value: 'admin123' } });
    fireEvent.click(submitButton);
    
    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/');
    });
  });

  it('should clear error message when user starts typing again', async () => {
    mockLoginAdmin.mockResolvedValue(false);
    renderLoginPage();
    
    const passwordInput = screen.getByLabelText(/lösenord/i);
    const submitButton = screen.getByRole('button', { name: /logga in/i });
    
    // First submission with wrong password
    fireEvent.change(passwordInput, { target: { value: 'wrong' } });
    fireEvent.click(submitButton);
    
    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeInTheDocument();
    });
    
    // Start typing again - error should clear
    fireEvent.change(passwordInput, { target: { value: 'wrong2' } });
    
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });

  it('should have accessible form with proper labels', () => {
    renderLoginPage();
    
    const passwordInput = screen.getByLabelText(/lösenord/i);
    expect(passwordInput).toHaveAttribute('type', 'password');
    expect(passwordInput).toHaveAttribute('id');
  });

  it('should show info text for parent users', () => {
    renderLoginPage();
    
    expect(screen.getByText(/föräldrar.*magiska länk/i)).toBeInTheDocument();
  });
});
