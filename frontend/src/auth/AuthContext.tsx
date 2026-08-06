/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useEffect, useMemo, useState } from "react";

type Role = "Admin" | "ParentA" | "ParentB" | "";

interface UserDto {
  id: number;
  email: string;
  role: Role;
  displayName?: string | null;
  lastLoginAt?: string | null;
}

interface AuthResponse {
  token: string;
  user: UserDto;
  expiresAt: string;
}

interface AuthContextValue {
  token: string | null;
  user: UserDto | null;
  isAuthenticated: boolean;
  loginAdmin: (password: string) => Promise<boolean>;
  exchangeMagicToken: (magicToken: string) => Promise<boolean>;
  logout: () => void;
  authHeader: () => Record<string, string>;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const API_BASE_URL = import.meta.env.VITE_API_URL || "";

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(() =>
    localStorage.getItem("auth_token")
  );
  const [user, setUser] = useState<UserDto | null>(() => {
    const raw = localStorage.getItem("auth_user");
    return raw ? JSON.parse(raw) : null;
  });

  useEffect(() => {
    if (token) localStorage.setItem("auth_token", token);
    else localStorage.removeItem("auth_token");
  }, [token]);

  useEffect(() => {
    if (user) localStorage.setItem("auth_user", JSON.stringify(user));
    else localStorage.removeItem("auth_user");
  }, [user]);

  const isAuthenticated = !!token;

  const loginAdmin = async (password: string) => {
    try {
      const res = await fetch(`${API_BASE_URL}/api/auth/admin/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ password }),
      });
      if (!res.ok) return false;
      const data: AuthResponse = await res.json();
      setToken(data.token);
      setUser(data.user);
      return true;
    } catch {
      return false;
    }
  };

  const exchangeMagicToken = async (magicToken: string) => {
    try {
      const res = await fetch(`${API_BASE_URL}/api/auth/magic`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ token: magicToken }),
      });
      if (!res.ok) return false;
      const data: AuthResponse = await res.json();
      setToken(data.token);
      setUser(data.user);
      return true;
    } catch {
      return false;
    }
  };

  const logout = () => {
    setToken(null);
    setUser(null);
  };

  const authHeader = useMemo<() => Record<string, string>>(() => {
    return () => (token ? { Authorization: `Bearer ${token}` } : {} as Record<string, string>);
  }, [token]);

  const value: AuthContextValue = {
    token,
    user,
    isAuthenticated,
    loginAdmin,
    exchangeMagicToken,
    logout,
    authHeader,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
