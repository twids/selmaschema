import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import InvitationsPage from "./InvitationsPage";

const mocks = vi.hoisted(() => ({ invitations: vi.fn(), createInvitation: vi.fn(), revokeInvitation: vi.fn() }));
vi.mock("../api/v2", () => ({ familyApi: { invitations: mocks.invitations, createInvitation: mocks.createInvitation, revokeInvitation: mocks.revokeInvitation } }));

describe("InvitationsPage v2", () => {
  beforeEach(() => {
    mocks.invitations.mockResolvedValue([]);
    mocks.createInvitation.mockResolvedValue({ invitation: { id: "i" }, link: "https://selma.test/join/token", code: "ABCD-EFGH" });
    Object.assign(navigator, { clipboard: { writeText: vi.fn().mockResolvedValue(undefined) } });
  });
  it("shows both one-time link and short code and copies them", async () => {
    render(<MemoryRouter initialEntries={["/families/f/invitations"]}><Routes><Route path="/families/:familyId/invitations" element={<InvitationsPage />} /></Routes></MemoryRouter>);
    await waitFor(() => expect(mocks.invitations).toHaveBeenCalledWith("f"));
    fireEvent.click(screen.getByRole("button", { name: "Skapa inbjudan" }));
    await screen.findByText("ABCD-EFGH");
    fireEvent.click(screen.getByRole("button", { name: "Kopiera kod" }));
    expect(navigator.clipboard.writeText).toHaveBeenCalledWith("ABCD-EFGH");
  });
});
