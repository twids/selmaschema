import { describe, it, expect } from "vitest";
import { parseImportFile, validateImportData } from "../utils/dataImport";

/* ---------- helpers ---------- */

function createFile(content: string, name = "test.json"): File {
  return new File([content], name, { type: "application/json" });
}

const validExport = {
  calendarData: {
    "2026-02-09": {
      id: 1,
      date: "2026-02-09",
      parent: "parentA",
      isVAB: false,
      specialStatus: null,
      parentAComments: [],
      parentBComments: [],
    },
  },
  parentNames: { parentAName: "Alice", parentBName: "Bob" },
  exportDate: "2026-02-07T12:00:00.000Z",
  year: 2026,
  month: 2,
};

/* ---------- validateImportData ---------- */

describe("validateImportData", () => {
  it("returns true for valid export data", () => {
    expect(validateImportData(validExport)).toBe(true);
  });

  it("returns false for null", () => {
    expect(validateImportData(null)).toBe(false);
  });

  it("returns false for non-object", () => {
    expect(validateImportData("string")).toBe(false);
  });

  it("returns false when calendarData is missing", () => {
    const { calendarData: _, ...rest } = validExport;
    expect(validateImportData(rest)).toBe(false);
  });

  it("returns false when parentNames is missing", () => {
    const { parentNames: _, ...rest } = validExport;
    expect(validateImportData(rest)).toBe(false);
  });

  it("returns false when year is missing", () => {
    const { year: _, ...rest } = validExport;
    expect(validateImportData(rest)).toBe(false);
  });

  it("returns false when month is missing", () => {
    const { month: _, ...rest } = validExport;
    expect(validateImportData(rest)).toBe(false);
  });

  it("returns false when exportDate is missing", () => {
    const { exportDate: _, ...rest } = validExport;
    expect(validateImportData(rest)).toBe(false);
  });

  it("returns false when year is not a number", () => {
    expect(validateImportData({ ...validExport, year: "2026" })).toBe(false);
  });

  it("returns false when month is not a number", () => {
    expect(validateImportData({ ...validExport, month: "2" })).toBe(false);
  });

  it("returns false when calendarData is not an object", () => {
    expect(validateImportData({ ...validExport, calendarData: [] })).toBe(false);
  });

  it("returns false when parentNames lacks parentAName", () => {
    expect(
      validateImportData({
        ...validExport,
        parentNames: { parentBName: "Bob" },
      }),
    ).toBe(false);
  });

  it("returns false when parentNames lacks parentBName", () => {
    expect(
      validateImportData({
        ...validExport,
        parentNames: { parentAName: "Alice" },
      }),
    ).toBe(false);
  });
});

/* ---------- parseImportFile ---------- */

describe("parseImportFile", () => {
  it("parses a valid JSON file", async () => {
    const file = createFile(JSON.stringify(validExport));
    const result = await parseImportFile(file);

    expect(result.success).toBe(true);
    expect(result.data).toBeDefined();
    expect(result.data!.year).toBe(2026);
    expect(result.data!.month).toBe(2);
  });

  it("returns error for invalid JSON", async () => {
    const file = createFile("not json {{{");
    const result = await parseImportFile(file);

    expect(result.success).toBe(false);
    expect(result.error).toBeDefined();
    expect(result.error).toContain("parse");
  });

  it("returns error for valid JSON with wrong structure", async () => {
    const file = createFile(JSON.stringify({ foo: "bar" }));
    const result = await parseImportFile(file);

    expect(result.success).toBe(false);
    expect(result.error).toBeDefined();
    expect(result.error).toContain("structure");
  });

  it("returns the parsed data on success", async () => {
    const file = createFile(JSON.stringify(validExport));
    const result = await parseImportFile(file);

    expect(result.success).toBe(true);
    expect(result.data?.calendarData["2026-02-09"].parent).toBe("parentA");
  });
});
