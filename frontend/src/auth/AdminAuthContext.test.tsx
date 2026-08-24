import { render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AdminAuthProvider, useAdminAuth } from "./AdminAuthContext";

function Consumer() {
  const { admin, isLoading } = useAdminAuth();
  return <div>{isLoading ? "laddar" : admin?.email ?? "ingen admin"}</div>;
}

describe("AdminAuthProvider", () => {
  beforeEach(() => vi.restoreAllMocks());

  it("bootstraps only from the separate admin session endpoint", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ email: "admin@example.test", displayName: "Admin", authenticationMethod: "oidc", expiresAt: "2026-08-20T22:00:00Z" }),
    }));
    render(<AdminAuthProvider><Consumer /></AdminAuthProvider>);
    await waitFor(() => expect(screen.getByText("admin@example.test")).toBeInTheDocument());
    expect(fetch).toHaveBeenCalledWith(expect.stringContaining("/api/admin/auth/me"), { credentials: "include" });
    expect(fetch).not.toHaveBeenCalledWith(expect.stringContaining("/api/auth/me"), expect.anything());
  });
});
