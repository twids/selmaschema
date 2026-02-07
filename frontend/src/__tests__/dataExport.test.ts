import { describe, it, expect, vi, beforeEach } from "vitest";
import { createExportData, downloadAsJson } from "../utils/dataExport";
import type { CalendarData, DayAssignmentDto, ParentNamesDto } from "../api/types";

/* ---------- fixtures ---------- */

const sampleDay: DayAssignmentDto = {
  id: 1,
  date: "2026-02-09",
  parent: "parentA",
  isVAB: false,
  specialStatus: null,
  parentAComments: [],
  parentBComments: [],
};

const calendarData: CalendarData = {
  "2026-02-09": sampleDay,
};

const parentNames: ParentNamesDto = {
  parentAName: "Alice",
  parentBName: "Bob",
};

/* ---------- tests ---------- */

describe("createExportData", () => {
  it("packages calendar data with metadata", () => {
    const result = createExportData(calendarData, parentNames, 2026, 2);

    expect(result.calendarData).toBe(calendarData);
    expect(result.parentNames).toBe(parentNames);
    expect(result.year).toBe(2026);
    expect(result.month).toBe(2);
    expect(result.exportDate).toBeDefined();
    expect(typeof result.exportDate).toBe("string");
  });

  it("sets exportDate to a valid ISO string", () => {
    const result = createExportData(calendarData, parentNames, 2026, 2);
    const parsed = new Date(result.exportDate);
    expect(parsed.getTime()).not.toBeNaN();
  });
});

describe("downloadAsJson", () => {
  let createObjectURLSpy: ReturnType<typeof vi.fn>;
  let revokeObjectURLSpy: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    createObjectURLSpy = vi.fn().mockReturnValue("blob:test-url");
    revokeObjectURLSpy = vi.fn();
    globalThis.URL.createObjectURL = createObjectURLSpy as unknown as typeof URL.createObjectURL;
    globalThis.URL.revokeObjectURL = revokeObjectURLSpy as unknown as typeof URL.revokeObjectURL;
  });

  it("creates a download link and triggers click", () => {
    const clickSpy = vi.fn();
    vi.spyOn(document, "createElement").mockReturnValue({
      href: "",
      download: "",
      click: clickSpy,
    } as unknown as HTMLAnchorElement);

    const data = createExportData(calendarData, parentNames, 2026, 2);
    downloadAsJson(data);

    expect(createObjectURLSpy).toHaveBeenCalledOnce();
    expect(clickSpy).toHaveBeenCalledOnce();
    expect(revokeObjectURLSpy).toHaveBeenCalledOnce();
  });

  it("uses default filename based on year and month", () => {
    const anchor = { href: "", download: "", click: vi.fn() };
    vi.spyOn(document, "createElement").mockReturnValue(
      anchor as unknown as HTMLAnchorElement,
    );

    const data = createExportData(calendarData, parentNames, 2026, 2);
    downloadAsJson(data);

    expect(anchor.download).toBe("calendar-export-2026-2.json");
  });

  it("uses a custom filename when provided", () => {
    const anchor = { href: "", download: "", click: vi.fn() };
    vi.spyOn(document, "createElement").mockReturnValue(
      anchor as unknown as HTMLAnchorElement,
    );

    const data = createExportData(calendarData, parentNames, 2026, 2);
    downloadAsJson(data, "my-file.json");

    expect(anchor.download).toBe("my-file.json");
  });
});
