import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { ThemeProvider } from "@mui/material/styles";
import { theme } from "../theme";
import ImportExport from "../components/ImportExport";
import type { CalendarData, DayAssignmentDto } from "../api/types";

/* ---------- mocks ---------- */

const mockUseCalendar = vi.fn();
const mockUseConfig = vi.fn();

vi.mock("../context/CalendarContext", () => ({
  useCalendar: () => mockUseCalendar(),
}));

vi.mock("../context/ConfigContext", () => ({
  useConfig: () => mockUseConfig(),
}));

vi.mock("../utils/dataExport", () => ({
  createExportData: vi.fn().mockReturnValue({
    calendarData: {},
    parentNames: { parentAName: "Alice", parentBName: "Bob" },
    exportDate: "2026-02-07T12:00:00.000Z",
    year: 2026,
    month: 2,
  }),
  downloadAsJson: vi.fn(),
}));

vi.mock("../utils/dataImport", () => ({
  parseImportFile: vi.fn(),
  validateImportData: vi.fn().mockReturnValue(true),
}));

/* ---------- helpers ---------- */

function renderWithTheme(ui: React.ReactElement) {
  return render(<ThemeProvider theme={theme}>{ui}</ThemeProvider>);
}

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

const defaultCalendarCtx = {
  currentYear: 2026,
  currentMonth: 2,
  calendarData,
  loading: false,
  error: null,
  loadMonthData: vi.fn(),
  updateDay: vi.fn().mockResolvedValue(undefined),
  initializeMonth: vi.fn(),
  setMonth: vi.fn(),
  setYear: vi.fn(),
  navigateMonth: vi.fn(),
};

const defaultConfigCtx = {
  parentNames: { parentAName: "Alice", parentBName: "Bob" },
  loading: false,
  updateParentNames: vi.fn(),
};

/* ---------- tests ---------- */

describe("ImportExport", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseCalendar.mockReturnValue(defaultCalendarCtx);
    mockUseConfig.mockReturnValue(defaultConfigCtx);
  });

  it("renders the import/export container", () => {
    renderWithTheme(<ImportExport />);
    expect(screen.getByTestId("import-export")).toBeInTheDocument();
  });

  it("renders an export button", () => {
    renderWithTheme(<ImportExport />);
    expect(
      screen.getByRole("button", { name: /export/i }),
    ).toBeInTheDocument();
  });

  it("renders an import button", () => {
    renderWithTheme(<ImportExport />);
    expect(
      screen.getByRole("button", { name: /import/i }),
    ).toBeInTheDocument();
  });

  it("calls createExportData and downloadAsJson when export is clicked", async () => {
    const { createExportData, downloadAsJson } = await import(
      "../utils/dataExport"
    );

    renderWithTheme(<ImportExport />);
    fireEvent.click(screen.getByRole("button", { name: /export/i }));

    expect(createExportData).toHaveBeenCalledWith(
      calendarData,
      defaultConfigCtx.parentNames,
      2026,
      2,
    );
    expect(downloadAsJson).toHaveBeenCalled();
  });

  it("opens file input when import button is clicked", () => {
    renderWithTheme(<ImportExport />);
    const fileInput = document.querySelector(
      'input[type="file"]',
    ) as HTMLInputElement;
    expect(fileInput).toBeTruthy();
    expect(fileInput.style.display).toBe("none");
  });

  it("shows confirmation dialog after selecting a valid file", async () => {
    const { parseImportFile } = await import("../utils/dataImport");
    (parseImportFile as ReturnType<typeof vi.fn>).mockResolvedValue({
      success: true,
      data: {
        calendarData: { "2026-02-09": sampleDay },
        parentNames: { parentAName: "Alice", parentBName: "Bob" },
        exportDate: "2026-02-07T12:00:00.000Z",
        year: 2026,
        month: 2,
      },
    });

    renderWithTheme(<ImportExport />);

    const fileInput = document.querySelector(
      'input[type="file"]',
    ) as HTMLInputElement;
    const file = new File(
      [JSON.stringify({ test: true })],
      "test.json",
      { type: "application/json" },
    );

    fireEvent.change(fileInput, { target: { files: [file] } });

    await waitFor(() => {
      expect(screen.getByText(/bekräfta import/i)).toBeInTheDocument();
    });
  });

  it("shows error snackbar for invalid file", async () => {
    const { parseImportFile } = await import("../utils/dataImport");
    (parseImportFile as ReturnType<typeof vi.fn>).mockResolvedValue({
      success: false,
      error: "Invalid file structure",
    });

    renderWithTheme(<ImportExport />);

    const fileInput = document.querySelector(
      'input[type="file"]',
    ) as HTMLInputElement;
    const file = new File(["bad"], "bad.json", { type: "application/json" });

    fireEvent.change(fileInput, { target: { files: [file] } });

    await waitFor(() => {
      expect(screen.getByText(/invalid file/i)).toBeInTheDocument();
    });
  });

  it("calls updateDay for each day after confirming import", async () => {
    const { parseImportFile } = await import("../utils/dataImport");
    const updateDay = vi.fn().mockResolvedValue(undefined);
    mockUseCalendar.mockReturnValue({ ...defaultCalendarCtx, updateDay });

    (parseImportFile as ReturnType<typeof vi.fn>).mockResolvedValue({
      success: true,
      data: {
        calendarData: { "2026-02-09": sampleDay },
        parentNames: { parentAName: "Alice", parentBName: "Bob" },
        exportDate: "2026-02-07T12:00:00.000Z",
        year: 2026,
        month: 2,
      },
    });

    renderWithTheme(<ImportExport />);

    const fileInput = document.querySelector(
      'input[type="file"]',
    ) as HTMLInputElement;
    const file = new File([JSON.stringify({})], "test.json", {
      type: "application/json",
    });

    fireEvent.change(fileInput, { target: { files: [file] } });

    await waitFor(() => {
      expect(screen.getByText(/bekräfta import/i)).toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole("button", { name: /bekräfta/i }));

    await waitFor(() => {
      expect(updateDay).toHaveBeenCalled();
    });
  });

  it("shows success snackbar after import completes", async () => {
    const { parseImportFile } = await import("../utils/dataImport");
    const updateDay = vi.fn().mockResolvedValue(undefined);
    mockUseCalendar.mockReturnValue({ ...defaultCalendarCtx, updateDay });

    (parseImportFile as ReturnType<typeof vi.fn>).mockResolvedValue({
      success: true,
      data: {
        calendarData: { "2026-02-09": sampleDay },
        parentNames: { parentAName: "Alice", parentBName: "Bob" },
        exportDate: "2026-02-07T12:00:00.000Z",
        year: 2026,
        month: 2,
      },
    });

    renderWithTheme(<ImportExport />);

    const fileInput = document.querySelector(
      'input[type="file"]',
    ) as HTMLInputElement;
    const file = new File([JSON.stringify({})], "test.json", {
      type: "application/json",
    });

    fireEvent.change(fileInput, { target: { files: [file] } });

    await waitFor(() => {
      expect(screen.getByText(/bekräfta import/i)).toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole("button", { name: /bekräfta/i }));

    await waitFor(() => {
      expect(screen.getByText(/import lyckades/i)).toBeInTheDocument();
    });
  });
});
