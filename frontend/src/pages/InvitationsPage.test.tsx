import { cleanup, fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AuthContext } from "../auth/AuthContext";
import InvitationConfirmPage from "./InvitationConfirmPage";
import InvitationsPage from "./InvitationsPage";

const createInvitation = vi.fn();
const getInvitations = vi.fn();
const getPendingInvitation = vi.fn();
const cancelPendingInvitation = vi.fn();
vi.mock("../api/invitations", () => ({
  createInvitation: (...args: unknown[]) => createInvitation(...args),
  getInvitations: () => getInvitations(),
  getPendingInvitation: () => getPendingInvitation(),
  cancelPendingInvitation: () => cancelPendingInvitation(),
}));

const completeInvitation = vi.fn();

function authValue(role: "Admin" | "ParentA" = "ParentA") {
  return {
    user: { id: 1, email: "user@test.se", role, displayName: "User" },
    isAuthenticated: true,
    isLoading: false,
    loginAdmin: vi.fn(),
    startOidcLogin: vi.fn(),
    completeInvitation,
    logout: vi.fn(),
    refreshUser: vi.fn(),
  };
}

describe("invitations UI", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    getInvitations.mockResolvedValue([]);
    Object.assign(navigator, { clipboard: { writeText: vi.fn().mockResolvedValue(undefined) } });
  });

  it("allows only admins to select the Admin invitation role", async () => {
    render(
      <AuthContext.Provider value={authValue("ParentA")}><InvitationsPage /></AuthContext.Provider>,
    );
    fireEvent.mouseDown(screen.getByLabelText("Roll"));
    expect(screen.queryByText("Admin")).not.toBeInTheDocument();

    cleanup();
    render(<AuthContext.Provider value={authValue("Admin")}><InvitationsPage /></AuthContext.Provider>);
    fireEvent.mouseDown(screen.getByLabelText("Roll"));
    expect(await screen.findByText("Admin")).toBeInTheDocument();
  });

  it("creates and copies the one-time invitation URL", async () => {
    createInvitation.mockResolvedValue({
      invitation: { id: 1, role: "ParentA" },
      invitationUrl: "https://selma.test/api/auth/invitations/raw",
    });
    render(<AuthContext.Provider value={authValue()}><InvitationsPage /></AuthContext.Provider>);
    fireEvent.change(screen.getByLabelText("E-postledtråd (valfritt)"), { target: { value: "hint@test.se" } });
    fireEvent.click(screen.getByRole("button", { name: "Skapa inbjudan" }));
    await waitFor(() => expect(createInvitation).toHaveBeenCalledWith("ParentA", "hint@test.se"));
    fireEvent.click(await screen.findByLabelText("Kopiera inbjudningslänk"));
    expect(navigator.clipboard.writeText).toHaveBeenCalledWith("https://selma.test/api/auth/invitations/raw");
  });

  it("shows both addresses and requires explicit confirmation", async () => {
    getPendingInvitation.mockResolvedValue({
      invitationId: 1,
      emailHint: "hint@test.se",
      verifiedEmail: "actual@test.se",
      displayName: "Actual",
      role: "ParentB",
      expiresAt: new Date().toISOString(),
    });
    completeInvitation.mockResolvedValue(false);
    render(
      <MemoryRouter initialEntries={["/auth/invitation"]}>
        <AuthContext.Provider value={authValue()}><InvitationConfirmPage /></AuthContext.Provider>
      </MemoryRouter>,
    );

    expect(await screen.findByText(/hint@test.se/)).toBeInTheDocument();
    expect(screen.getByText(/actual@test.se/)).toBeInTheDocument();
    expect(screen.getByText(/Adresserna skiljer sig/)).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Bekräfta och fortsätt" }));
    await waitFor(() => expect(completeInvitation).toHaveBeenCalled());
  });
});
