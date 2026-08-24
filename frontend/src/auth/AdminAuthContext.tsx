/* eslint-disable react-refresh/only-export-components */
import { createContext, useCallback, useContext, useEffect, useState } from "react";
import { apiPost } from "../api/client";

export interface AdminMeDto {
  email: string;
  displayName: string;
  authenticationMethod: "oidc" | "break-glass";
  expiresAt: string;
}

interface AdminAuthContextValue {
  admin: AdminMeDto | null;
  isLoading: boolean;
  startLogin: () => void;
  breakGlassLogin: (password: string) => Promise<boolean>;
  logout: () => Promise<void>;
}

const AdminAuthContext = createContext<AdminAuthContextValue | undefined>(undefined);
const apiBase = import.meta.env.VITE_API_URL || "";

export function AdminAuthProvider({ children }: { children: React.ReactNode }) {
  const [admin, setAdmin] = useState<AdminMeDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    void fetch(`${apiBase}/api/admin/auth/me`, { credentials: "include" })
      .then(async (response) => setAdmin(response.ok ? ((await response.json()) as AdminMeDto) : null))
      .catch(() => setAdmin(null))
      .finally(() => setIsLoading(false));
  }, []);

  const startLogin = useCallback(() => {
    window.location.assign(`${apiBase}/api/admin/auth/login`);
  }, []);

  const breakGlassLogin = useCallback(async (password: string) => {
    try {
      const result = await apiPost<AdminMeDto>("/api/admin/auth/break-glass", { password });
      setAdmin(result);
      return true;
    } catch {
      return false;
    }
  }, []);

  const logout = useCallback(async () => {
    try {
      await apiPost<void>("/api/admin/auth/logout");
    } finally {
      setAdmin(null);
    }
  }, []);

  return (
    <AdminAuthContext.Provider value={{ admin, isLoading, startLogin, breakGlassLogin, logout }}>
      {children}
    </AdminAuthContext.Provider>
  );
}

export function useAdminAuth(): AdminAuthContextValue {
  const value = useContext(AdminAuthContext);
  if (!value) throw new Error("useAdminAuth must be used within AdminAuthProvider");
  return value;
}
