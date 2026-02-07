/**
 * Import utilities for calendar data.
 *
 * Reads an exported JSON file, validates its structure, and returns
 * a typed result with clear error messages.
 */

import type { ExportData } from "./dataExport";

/** Result of attempting to parse an import file. */
export interface ImportResult {
  success: boolean;
  data?: ExportData;
  error?: string;
}

/**
 * Type-guard that checks whether `data` conforms to the ExportData shape.
 */
export function validateImportData(data: unknown): data is ExportData {
  if (data === null || typeof data !== "object") return false;

  const obj = data as Record<string, unknown>;

  if (typeof obj.year !== "number") return false;
  if (typeof obj.month !== "number") return false;
  if (typeof obj.exportDate !== "string") return false;

  // calendarData must be a plain object (not array)
  if (
    typeof obj.calendarData !== "object" ||
    obj.calendarData === null ||
    Array.isArray(obj.calendarData)
  ) {
    return false;
  }

  // parentNames must have both fields
  if (typeof obj.parentNames !== "object" || obj.parentNames === null) {
    return false;
  }
  const names = obj.parentNames as Record<string, unknown>;
  if (typeof names.parentAName !== "string") return false;
  if (typeof names.parentBName !== "string") return false;

  return true;
}

/**
 * Read and validate a JSON file selected by the user.
 */
export async function parseImportFile(file: File): Promise<ImportResult> {
  try {
    const text = await new Promise<string>((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = () => reject(new Error("Failed to read file"));
      reader.readAsText(file);
    });

    let parsed: unknown;
    try {
      parsed = JSON.parse(text);
    } catch {
      return { success: false, error: "Could not parse JSON — check file format" };
    }

    if (!validateImportData(parsed)) {
      return {
        success: false,
        error: "Invalid file structure — missing required fields",
      };
    }

    return { success: true, data: parsed };
  } catch (err) {
    return {
      success: false,
      error: err instanceof Error ? err.message : String(err),
    };
  }
}
