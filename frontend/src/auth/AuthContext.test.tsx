import { render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AuthProvider, useAuth } from "./AuthContext";

function Consumer() {
  const value = useAuth();
  return <div>{value.isLoading ? "laddar" : `${value.account?.email}:${value.memberships.length}`}</div>;
}

describe("AuthProvider v2", () => {
  beforeEach(() => vi.restoreAllMocks());

  it("bootstraps account and memberships from the cookie-based /me endpoint", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        account: { id: "a", email: "person@example.test", displayName: "Person" },
        memberships: [{ familyId: "f", familyName: "Familjen", permission: "Owner", side: null, status: "Active" }],
      }),
    }));
    const storageSpy = vi.spyOn(Storage.prototype, "getItem");
    render(<AuthProvider><Consumer /></AuthProvider>);
    await waitFor(() => expect(screen.getByText("person@example.test:1")).toBeInTheDocument());
    expect(fetch).toHaveBeenCalledWith(expect.stringContaining("/api/auth/me"), { credentials: "include" });
    expect(storageSpy).not.toHaveBeenCalled();
  });

  it("finishes loading unauthenticated when /me returns 401", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue({ ok: false }));
    render(<AuthProvider><Consumer /></AuthProvider>);
    await waitFor(() => expect(screen.getByText("undefined:0")).toBeInTheDocument());
  });
});
