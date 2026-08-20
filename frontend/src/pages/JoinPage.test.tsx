import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import JoinPage from "./JoinPage";

const mocks = vi.hoisted(() => ({ previewCode: vi.fn(), code: vi.fn(), refresh: vi.fn() }));
vi.mock("../api/v2", () => ({
  joinApi: { previewCode: mocks.previewCode, code: mocks.code, preview: vi.fn(), complete: vi.fn() },
}));
vi.mock("../auth/AuthContext", () => ({
  useAuth: () => ({
    account: { id: "account", email: "actual@example.test", displayName: "Person" },
    isAuthenticated: true,
    isLoading: false,
    refresh: mocks.refresh,
    startOidcLogin: vi.fn(),
  }),
}));

describe("JoinPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mocks.previewCode.mockResolvedValue({
      invitationId: "invite", familyId: "family", familyName: "Familjen",
      emailHint: "hint@example.test", permission: "Editor", side: "B",
      expiresAt: "2026-08-27T00:00:00Z", alreadyMember: false,
    });
    mocks.code.mockResolvedValue({ familyId: "family", familyName: "Familjen", joined: true });
    mocks.refresh.mockResolvedValue(undefined);
  });

  it("previews a code and requires explicit confirmation when email differs", async () => {
    render(<MemoryRouter initialEntries={[{ pathname: "/join", state: { invitationCode: "ABCD-EFGH" } }]}><Routes>
      <Route path="/join" element={<JoinPage />} />
      <Route path="/families/:familyId" element={<div>familjevy</div>} />
    </Routes></MemoryRouter>);
    await screen.findByText(/Adresserna skiljer sig/);
    expect(mocks.previewCode).toHaveBeenCalledWith("ABCD-EFGH");
    expect(mocks.code).not.toHaveBeenCalled();
    fireEvent.click(screen.getByRole("button", { name: "Bekräfta och gå med" }));
    await waitFor(() => expect(mocks.code).toHaveBeenCalledWith("ABCD-EFGH"));
    await screen.findByText("familjevy");
  });
});
