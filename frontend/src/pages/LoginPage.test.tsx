import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AuthContext } from "../auth/AuthContext";
import LoginPage from "./LoginPage";

const loginAdmin = vi.fn();
const startOidcLogin = vi.fn();

function renderPage() {
  return render(
    <MemoryRouter>
      <AuthContext.Provider value={{
        user: null,
        isAuthenticated: false,
        isLoading: false,
        loginAdmin,
        startOidcLogin,
        completeInvitation: vi.fn(),
        logout: vi.fn(),
        refreshUser: vi.fn(),
      }}>
        <LoginPage />
      </AuthContext.Provider>
    </MemoryRouter>,
  );
}

describe("LoginPage", () => {
  beforeEach(() => vi.clearAllMocks());

  it("starts OIDC from the primary Widsell ID button", () => {
    renderPage();
    fireEvent.click(screen.getByRole("button", { name: "Logga in med Widsell ID" }));
    expect(startOidcLogin).toHaveBeenCalledWith("/");
  });

  it("keeps local admin as a secondary reserve form", async () => {
    loginAdmin.mockResolvedValue(false);
    renderPage();
    fireEvent.change(screen.getByLabelText("Administratörslösenord"), { target: { value: "wrong" } });
    fireEvent.click(screen.getByRole("button", { name: "Logga in som reservadmin" }));
    await waitFor(() => expect(loginAdmin).toHaveBeenCalledWith("wrong"));
    expect(await screen.findByText(/Ogiltigt administratörslösenord/)).toBeInTheDocument();
  });
});
