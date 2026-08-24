import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { AuthContext } from "../auth/AuthContext";
import LoginPage from "./LoginPage";

const auth = {
  account: null,
  memberships: [],
  isAuthenticated: false,
  isLoading: false,
  startOidcLogin: vi.fn(),
  logout: async () => undefined,
  refresh: async () => undefined,
};

function renderLogin(path = "/login", from = "/") {
  auth.startOidcLogin.mockClear();
  render(
    <MemoryRouter initialEntries={[{ pathname: path.split("?")[0], search: path.includes("?") ? `?${path.split("?")[1]}` : "", state: { from: { pathname: from } } }]}>
      <AuthContext.Provider value={auth}>
        <Routes><Route path="/login" element={<LoginPage />} /></Routes>
      </AuthContext.Provider>
    </MemoryRouter>,
  );
}

describe("LoginPage", () => {
  it("starts OIDC automatically after auth bootstrap", async () => {
    renderLogin("/login", "/families/family-1");

    expect(screen.getByRole("progressbar")).toBeInTheDocument();
    await waitFor(() => expect(auth.startOidcLogin).toHaveBeenCalledOnce());
    expect(auth.startOidcLogin).toHaveBeenCalledWith("/families/family-1");
  });

  it("shows a retry action instead of creating a redirect loop after an error", () => {
    renderLogin("/login?error=oidc");

    expect(auth.startOidcLogin).not.toHaveBeenCalled();
    fireEvent.click(screen.getByRole("button", { name: "Försök igen" }));
    expect(auth.startOidcLogin).toHaveBeenCalledWith("/");
  });
});
