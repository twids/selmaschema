import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import type { CalendarDayDto, FamilyDto, ScheduleDraft, SchedulePreviewDto } from "../api/v2";
import SchedulePreview from "./SchedulePreview";

const family: FamilyDto = {
  id: "family",
  name: "Selma",
  timeZoneId: "Europe/Stockholm",
  sideALabel: "Hos Tomas",
  sideBLabel: "Hos Anna",
  exchangeDetailLevel: "DayTimeAndPlace",
  status: "Active",
  myMemberId: "member",
  myPermission: "Owner",
  mySide: "A",
  calendars: [],
  activeChildren: 1,
  hasActiveSchedule: false,
};

const draft: ScheduleDraft = {
  template: "AlternatingWeeks",
  anchorDate: "2026-09-01",
  anchorSide: "A",
  effectiveFrom: "2026-09-01",
  parameters: { weekendStartsOn: 5, weekendLengthDays: 3, recurringWeekday: null, recurringWeekdayOvernight: false },
  changeoverTime: "17:00",
  changeoverPlace: "Skolan",
};

function previewDays(): CalendarDayDto[] {
  return Array.from({ length: 42 }, (_, index) => {
    const date = new Date(2026, 8, 1 + index);
    return {
      date: `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`,
      side: Math.floor(index / 7) % 2 === 0 ? "A" : "B",
      isOverride: false,
      isVab: false,
      specialStatus: null,
      changeoverTime: "17:00",
      changeoverPlace: "Skolan",
      hasRecurringVisit: index === 3,
      comments: [],
    };
  });
}

describe("SchedulePreview", () => {
  it("renders all preview days with real home names, changes, visits and a summary", () => {
    const preview: SchedulePreviewDto = { days: previewDays(), validUntil: "2099-09-01T12:00:00Z" };
    render(<SchedulePreview family={family} draft={draft} preview={preview} />);

    expect(screen.getByText("Så här blir schemat")).toBeInTheDocument();
    expect(screen.getByText("Hos Tomas: 7 dagar")).toBeInTheDocument();
    expect(screen.getByText("Hos Anna: 7 dagar")).toBeInTheDocument();
    expect(screen.getAllByRole("gridcell")).toHaveLength(42);
    expect(screen.getAllByText("↪ Byte").length).toBeGreaterThan(0);
    expect(screen.getAllByText("Besök").length).toBeGreaterThan(0);
    expect(screen.getAllByText("Kl. 17:00").length).toBeGreaterThan(0);
    expect(screen.getAllByText("Skolan").length).toBeGreaterThan(0);
    expect(screen.queryByText(/A · A · A/)).not.toBeInTheDocument();
  });
});
