import { render, screen, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { AuthProvider, useAuth } from "./AuthContext";

function Consumer() {
  const auth = useAuth();
  return <div>{auth.isLoading ? "loading" : auth.user?.email || "anonymous"}</div>;
}

describe("AuthProvider", () => {
  afterEach(() => vi.restoreAllMocks());

  it("bootstraps from /api/auth/me with cookies and exposes loading state", async () => {
    let resolve!: (response: Response) => void;
    const fetchMock = vi.spyOn(globalThis, "fetch").mockReturnValue(
      new Promise<Response>((done) => { resolve = done; }),
    );
    const storageSpy = vi.spyOn(Storage.prototype, "getItem");

    render(<AuthProvider><Consumer /></AuthProvider>);
    expect(screen.getByText("loading")).toBeInTheDocument();

    resolve(new Response(JSON.stringify({
      id: 1,
      email: "user@test.se",
      role: "ParentA",
      displayName: "User",
    }), { status: 200 }));

    expect(await screen.findByText("user@test.se")).toBeInTheDocument();
    expect(fetchMock).toHaveBeenCalledWith("/api/auth/me", { credentials: "include" });
    expect(storageSpy).not.toHaveBeenCalled();
  });

  it("becomes anonymous when /me returns 401", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(new Response(null, { status: 401 }));
    render(<AuthProvider><Consumer /></AuthProvider>);
    await waitFor(() => expect(screen.getByText("anonymous")).toBeInTheDocument());
  });
});
