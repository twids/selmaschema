/* eslint-disable react-refresh/only-export-components */
import { createContext, useCallback, useContext, useEffect, useState } from "react";

export type Role = "Admin" | "ParentA" | "ParentB";

export interface UserDto {
  id: number;
  email: string;
  role: Role;
  displayName?: string | null;
  lastLoginAt?: string | null;
}

interface AuthResponse {
  user: UserDto;
  expiresAt: string;
}

interface AuthContextValue {
  user: UserDto | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  loginAdmin: (password: string) => Promise<boolean>;
  startOidcLogin: (returnUrl?: string) => void;
  completeInvitation: () => Promise<boolean>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const API_BASE_URL = import.meta.env.VITE_API_URL || "";

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refreshUser = useCallback(async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/me`, {
        credentials: "include",
      });
      setUser(response.ok ? ((await response.json()) as UserDto) : null);
    } catch {
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void refreshUser();
  }, [refreshUser]);

  const loginAdmin = useCallback(async (password: string) => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/admin/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ password }),
        credentials: "include",
      });
      if (!response.ok) return false;
      const data = (await response.json()) as AuthResponse;
      setUser(data.user);
      return true;
    } catch {
      return false;
    }
  }, []);

  const startOidcLogin = useCallback((returnUrl = "/") => {
    const target = `${API_BASE_URL}/api/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`;
    window.location.assign(target);
  }, []);

  const completeInvitation = useCallback(async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/invitations/complete`, {
        method: "POST",
        credentials: "include",
      });
      if (!response.ok) return false;
      const data = (await response.json()) as AuthResponse;
      setUser(data.user);
      return true;
    } catch {
      return false;
    }
  }, []);

  const logout = useCallback(async () => {
    try {
      await fetch(`${API_BASE_URL}/api/auth/logout`, {
        method: "POST",
        credentials: "include",
      });
    } finally {
      setUser(null);
    }
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: user !== null,
        isLoading,
        loginAdmin,
        startOidcLogin,
        completeInvitation,
        logout,
        refreshUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within AuthProvider");
  return context;
}
