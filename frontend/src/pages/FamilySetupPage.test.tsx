import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { FamilyDto } from "../api/v2";
import { AuthContext } from "../auth/AuthContext";
import FamilySetupPage from "./FamilySetupPage";

const mocks = vi.hoisted(() => ({
  get: vi.fn(), children: vi.fn(), calendars: vi.fn(), update: vi.fn(), updateMember: vi.fn(),
  createChild: vi.fn(), createInvitation: vi.fn(), refresh: vi.fn(),
}));

vi.mock("../api/v2", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../api/v2")>();
  return { ...actual, familyApi: { ...actual.familyApi, ...mocks } };
});

vi.mock("../components/ScheduleEditor", () => ({
  default: ({ onActivated }: { onActivated?: () => void | Promise<void> }) => (
    <button onClick={() => void onActivated?.()}>Aktivera testschema</button>
  ),
}));

const initialFamily: FamilyDto = {
  id: "family",
  name: "Familjen",
  timeZoneId: "Europe/Stockholm",
  sideALabel: "Hem A",
  sideBLabel: "Hem B",
  exchangeDetailLevel: "Day",
  status: "Active",
  myMemberId: "member",
  myPermission: "Owner",
  mySide: null,
  calendars: [{ id: "calendar", name: "Boendeschema", isActive: true, childCount: 0 }],
  activeChildren: 0,
  hasActiveSchedule: false,
};

describe("FamilySetupPage", () => {
  beforeEach(() => {
    Object.values(mocks).forEach((mock) => mock.mockReset());
    mocks.get
      .mockResolvedValueOnce(initialFamily)
      .mockResolvedValueOnce({ ...initialFamily, mySide: "A", activeChildren: 1 })
      .mockResolvedValue({ ...initialFamily, mySide: "A", activeChildren: 1, hasActiveSchedule: true });
    mocks.children.mockResolvedValueOnce([]).mockResolvedValue([{ id: "child", displayName: "Selma", calendarId: "calendar", isActive: true }]);
    mocks.calendars.mockResolvedValue([{ id: "calendar", name: "Boendeschema", isActive: true, children: [] }]);
    mocks.update.mockResolvedValue({ ...initialFamily, mySide: null });
    mocks.updateMember.mockResolvedValue({});
    mocks.createChild.mockResolvedValue({});
    mocks.createInvitation.mockResolvedValue({ invitation: { id: "invitation" }, link: "https://selma.test/join/token", code: "ABCD-EFGH" });
    mocks.refresh.mockResolvedValue(undefined);
    Object.assign(navigator, { clipboard: { writeText: vi.fn().mockResolvedValue(undefined) } });
  });

  it("saves each required step and can skip the optional invitation", async () => {
    render(
      <MemoryRouter initialEntries={["/families/family/setup"]}>
        <AuthContext.Provider value={{
          account: { id: "account", email: "owner@example.test", displayName: "Owner" },
          memberships: [{ familyId: "family", familyName: "Familjen", permission: "Owner", side: null, status: "Active" }],
          isAuthenticated: true, isLoading: false, startOidcLogin: () => undefined,
          logout: async () => undefined, refresh: mocks.refresh,
        }}>
          <Routes><Route path="/families/:familyId/setup" element={<FamilySetupPage />} /></Routes>
        </AuthContext.Provider>
      </MemoryRouter>,
    );

    expect(await screen.findByRole("heading", { name: "1. Familjen och hemmen" })).toBeInTheDocument();
    fireEvent.click(screen.getByRole("radio", { name: "Hem A" }));
    fireEvent.click(screen.getByRole("button", { name: "Spara och fortsätt" }));
    await waitFor(() => expect(mocks.updateMember).toHaveBeenCalledWith("family", "member", "Owner", "A", "Initial familjekonfiguration"));

    expect(await screen.findByRole("heading", { name: "2. Barn och kalender" })).toBeInTheDocument();
    fireEvent.change(screen.getByLabelText("Barnets visningsnamn"), { target: { value: "Selma" } });
    fireEvent.click(screen.getByRole("button", { name: "Lägg till barn" }));
    expect(await screen.findByText(/Selma är tillagd/)).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Nästa: boendeschema" }));

    expect(await screen.findByRole("heading", { name: "3. Boendeschema" })).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Aktivera testschema" }));
    expect(await screen.findByRole("heading", { name: "4. Bjud in en annan vuxen" })).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Hoppa över" }));
    expect(await screen.findByText("Familjen är redo att användas")).toBeInTheDocument();
  });

  it("offers an optional invitation with the opposite home and copyable one-time values", async () => {
    const configuredFamily = { ...initialFamily, mySide: "A" as const, activeChildren: 1 };
    mocks.get.mockReset().mockResolvedValueOnce(configuredFamily).mockResolvedValue({ ...configuredFamily, hasActiveSchedule: true });
    mocks.children.mockReset().mockResolvedValue([{ id: "child", displayName: "Selma", calendarId: "calendar", isActive: true }]);

    render(
      <MemoryRouter initialEntries={["/families/family/setup"]}>
        <AuthContext.Provider value={{
          account: { id: "account", email: "owner@example.test", displayName: "Owner" },
          memberships: [{ familyId: "family", familyName: "Familjen", permission: "Owner", side: "A", status: "Active" }],
          isAuthenticated: true, isLoading: false, startOidcLogin: () => undefined,
          logout: async () => undefined, refresh: mocks.refresh,
        }}>
          <Routes><Route path="/families/:familyId/setup" element={<FamilySetupPage />} /></Routes>
        </AuthContext.Provider>
      </MemoryRouter>,
    );

    expect(await screen.findByRole("heading", { name: "3. Boendeschema" })).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Aktivera testschema" }));
    expect(await screen.findByRole("heading", { name: "4. Bjud in en annan vuxen" })).toBeInTheDocument();
    expect(screen.getByLabelText("Tillhör hem")).toHaveTextContent("Hem B");
    fireEvent.click(screen.getByRole("button", { name: "Skapa inbjudan" }));
    expect(await screen.findByText("ABCD-EFGH")).toBeInTheDocument();
    expect(mocks.createInvitation).toHaveBeenCalledWith("family", "Editor", "B", "");
    fireEvent.click(screen.getByRole("button", { name: "Kopiera kod" }));
    expect(await screen.findByRole("button", { name: "Kod kopierad" })).toBeInTheDocument();
    expect(navigator.clipboard.writeText).toHaveBeenCalledWith("ABCD-EFGH");
  });
});
