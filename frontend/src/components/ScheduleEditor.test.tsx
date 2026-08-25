import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type { CalendarDayDto, FamilyDto } from "../api/v2";
import ScheduleEditor from "./ScheduleEditor";

const mocks = vi.hoisted(() => ({ preview: vi.fn(), create: vi.fn() }));
vi.mock("../api/v2", async (importOriginal) => {
  const actual = await importOriginal<typeof import("../api/v2")>();
  return { ...actual, scheduleApi: { ...actual.scheduleApi, preview: mocks.preview, create: mocks.create } };
});

const family: FamilyDto = {
  id: "family",
  name: "Selma",
  timeZoneId: "Europe/Stockholm",
  sideALabel: "Hos Tomas",
  sideBLabel: "Hos Anna",
  exchangeDetailLevel: "Day",
  status: "Active",
  myMemberId: "member",
  myPermission: "Owner",
  mySide: "A",
  calendars: [{ id: "calendar", name: "Boendeschema", isActive: true, childCount: 1 }],
  activeChildren: 1,
  hasActiveSchedule: false,
};

function days(): CalendarDayDto[] {
  return Array.from({ length: 42 }, (_, index) => {
    const date = new Date(2026, 8, 1 + index);
    return {
      date: `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`,
      side: index < 7 ? "A" : "B",
      isOverride: false,
      isVab: false,
      specialStatus: null,
      changeoverTime: null,
      changeoverPlace: null,
      hasRecurringVisit: false,
      comments: [],
    };
  });
}

describe("ScheduleEditor", () => {
  beforeEach(() => {
    mocks.preview.mockReset();
    mocks.create.mockReset();
    mocks.preview.mockResolvedValue({ days: days(), validUntil: "2099-09-01T12:00:00Z" });
    mocks.create.mockResolvedValue({ id: "version" });
  });

  it("keeps the simple start date as anchor and invalidates a stale preview after changes", async () => {
    render(<ScheduleEditor family={family} calendarId="calendar" showHistory={false} />);

    expect(screen.getByLabelText("Mönstret räknas från")).not.toBeVisible();
    fireEvent.change(screen.getByLabelText("Schemat börjar"), { target: { value: "2026-09-01" } });
    fireEvent.click(screen.getByRole("button", { name: "Förhandsvisa sex veckor" }));

    await waitFor(() => expect(mocks.preview).toHaveBeenCalled());
    expect(mocks.preview.mock.calls[0][2]).toMatchObject({ effectiveFrom: "2026-09-01", anchorDate: "2026-09-01" });
    expect(await screen.findByTestId("schedule-preview")).toBeInTheDocument();
    expect(screen.getAllByRole("gridcell")).toHaveLength(42);
    expect(screen.getByRole("button", { name: "Aktivera schema" })).toBeEnabled();

    fireEvent.change(screen.getByLabelText("Schemat börjar"), { target: { value: "2026-09-02" } });
    expect(screen.queryByTestId("schedule-preview")).not.toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Aktivera schema" })).toBeDisabled();
  });

  it("offers a separately explained anchor only in the advanced section", () => {
    render(<ScheduleEditor family={family} calendarId="calendar" showHistory={false} />);
    fireEvent.click(screen.getByText("Avancerat: anpassa mönstrets ankare"));
    expect(screen.getByLabelText("Mönstret räknas från")).toBeInTheDocument();
    expect(screen.getByText(/Selma räknar sedan mönstret framåt/)).toBeInTheDocument();
  });

  it("requires a new preview when the previous preview has expired", async () => {
    mocks.preview.mockResolvedValue({ days: days(), validUntil: "2000-09-01T12:00:00Z" });
    render(<ScheduleEditor family={family} calendarId="calendar" showHistory={false} />);
    fireEvent.click(screen.getByRole("button", { name: "Förhandsvisa sex veckor" }));
    expect(await screen.findByTestId("schedule-preview")).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Aktivera schema" }));
    expect(await screen.findByText(/Förhandsvisningen har gått ut/)).toBeInTheDocument();
    expect(mocks.create).not.toHaveBeenCalled();
  });

  it("does not suggest a duplicate activation when only the following reload fails", async () => {
    const onActivated = vi.fn().mockRejectedValue(new Error("reload failed"));
    render(<ScheduleEditor family={family} calendarId="calendar" showHistory={false} onActivated={onActivated} />);
    fireEvent.click(screen.getByRole("button", { name: "Förhandsvisa sex veckor" }));
    expect(await screen.findByTestId("schedule-preview")).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Aktivera schema" }));
    expect(await screen.findByText(/Schemat aktiverades, men sidan kunde inte uppdateras/)).toBeInTheDocument();
    expect(mocks.create).toHaveBeenCalledTimes(1);
    expect(screen.queryByTestId("schedule-preview")).not.toBeInTheDocument();
  });
});
