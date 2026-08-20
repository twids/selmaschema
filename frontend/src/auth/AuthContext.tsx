/* eslint-disable react-refresh/only-export-components */
import { createContext, useCallback, useContext, useEffect, useState } from "react";
import { apiPost } from "../api/client";

export type FamilyPermission = "Owner" | "Editor" | "Viewer";
export type ScheduleSide = "A" | "B";

export interface AccountDto {
  id: string;
  email: string;
  displayName: string;
}

export interface MembershipSummaryDto {
  familyId: string;
  familyName: string;
  permission: FamilyPermission;
  side: ScheduleSide | null;
  status: "Active" | "Suspended";
}

export interface AuthMeDto {
  account: AccountDto;
  memberships: MembershipSummaryDto[];
}

interface AuthContextValue {
  account: AccountDto | null;
  memberships: MembershipSummaryDto[];
  isAuthenticated: boolean;
  isLoading: boolean;
  startOidcLogin: (returnUrl?: string) => void;
  logout: () => Promise<void>;
  refresh: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);
const apiBase = import.meta.env.VITE_API_URL || "";

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [me, setMe] = useState<AuthMeDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refresh = useCallback(async () => {
    try {
      const response = await fetch(`${apiBase}/api/auth/me`, { credentials: "include" });
      setMe(response.ok ? ((await response.json()) as AuthMeDto) : null);
    } catch {
      setMe(null);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const startOidcLogin = useCallback((returnUrl = "/") => {
    window.location.assign(`${apiBase}/api/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`);
  }, []);

  const logout = useCallback(async () => {
    try {
      await apiPost<void>("/api/auth/logout");
    } finally {
      setMe(null);
    }
  }, []);

  return (
    <AuthContext.Provider
      value={{
        account: me?.account ?? null,
        memberships: me?.memberships ?? [],
        isAuthenticated: me !== null,
        isLoading,
        startOidcLogin,
        logout,
        refresh,
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
