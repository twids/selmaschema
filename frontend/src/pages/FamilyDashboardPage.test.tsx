import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { FamilyDto } from "../api/v2";
import FamilyDashboardPage from "./FamilyDashboardPage";

const mocks = vi.hoisted(() => ({ get: vi.fn() }));
vi.mock("../api/v2", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../api/v2")>();
  return { ...actual, familyApi: { ...actual.familyApi, get: mocks.get } };
});

const family: FamilyDto = {
  id: "family",
  name: "Familjen",
  timeZoneId: "Europe/Stockholm",
  sideALabel: "Hos Tomas",
  sideBLabel: "Hos Anna",
  exchangeDetailLevel: "Day",
  status: "Active",
  myMemberId: "member",
  myPermission: "Owner",
  mySide: "A",
  calendars: [],
  activeChildren: 0,
  hasActiveSchedule: false,
};

describe("FamilyDashboardPage setup progress", () => {
  beforeEach(() => mocks.get.mockReset());

  it("shows the next missing step and links to the resumable guide", async () => {
    mocks.get.mockResolvedValue(family);
    render(<MemoryRouter initialEntries={["/families/family"]}><Routes><Route path="/families/:familyId" element={<FamilyDashboardPage />} /></Routes></MemoryRouter>);
    expect(await screen.findByText("Nästa steg: Lägg till minst ett barn")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Fortsätt steg för steg" })).toHaveAttribute("href", "/families/family/setup");
  });

  it("hides setup progress after all required setup is complete", async () => {
    mocks.get.mockResolvedValue({ ...family, activeChildren: 1, hasActiveSchedule: true });
    render(<MemoryRouter initialEntries={["/families/family"]}><Routes><Route path="/families/:familyId" element={<FamilyDashboardPage />} /></Routes></MemoryRouter>);
    expect(await screen.findByRole("heading", { name: "Familjen" })).toBeInTheDocument();
    expect(screen.queryByText("Fortsätt konfigurera familjen")).not.toBeInTheDocument();
  });
});
