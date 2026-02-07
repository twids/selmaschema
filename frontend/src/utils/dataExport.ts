/**
 * Export utilities for calendar data.
 *
 * Packages calendar state into a portable JSON format and triggers
 * a browser download.
 */

import type { CalendarData, ParentNamesDto } from "../api/types";

/** Shape of exported calendar data with metadata. */
export interface ExportData {
  calendarData: CalendarData;
  parentNames: ParentNamesDto;
  exportDate: string;
  year: number;
  month: number;
}

/** Package calendar data with metadata for export. */
export function createExportData(
  calendarData: CalendarData,
  parentNames: ParentNamesDto,
  year: number,
  month: number,
): ExportData {
  return {
    calendarData,
    parentNames,
    exportDate: new Date().toISOString(),
    year,
    month,
  };
}

/**
 * Trigger a JSON file download in the browser.
 *
 * @param data     The export payload to serialise.
 * @param filename Optional filename override. Defaults to
 *                 `calendar-export-{year}-{month}.json`.
 */
export function downloadAsJson(data: ExportData, filename?: string): void {
  const json = JSON.stringify(data, null, 2);
  const blob = new Blob([json], { type: "application/json" });
  const url = URL.createObjectURL(blob);

  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = filename ?? `calendar-export-${data.year}-${data.month}.json`;
  anchor.click();

  URL.revokeObjectURL(url);
}
