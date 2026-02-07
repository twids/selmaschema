import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { ThemeProvider } from "@mui/material/styles";
import { theme } from "../theme";
import Statistics from "../components/Statistics";
import type { StatisticsDto } from "../api/types";

/* ---------- mocks ---------- */

const mockUseCalendar = vi.fn();
const mockUseConfig = vi.fn();
const mockUseAuth = vi.fn();

vi.mock("../context/CalendarContext", () => ({
  useCalendar: () => mockUseCalendar(),
}));

vi.mock("../context/ConfigContext", () => ({
  useConfig: () => mockUseConfig(),
}));

vi.mock("../auth/AuthContext", () => ({
  useAuth: () => mockUseAuth(),
}));

/* ---------- helpers ---------- */

function renderWithTheme(ui: React.ReactElement) {
  return render(<ThemeProvider theme={theme}>{ui}</ThemeProvider>);
}

const statsResponse: StatisticsDto = {
  parentADays: 120,
  parentBDays: 110,
  vabDays: 15,
  unassignedDays: 120,
  daysWithComments: 25,
};

function setupDefaults() {
  mockUseCalendar.mockReturnValue({ currentYear: 2026 });
  mockUseConfig.mockReturnValue({
    parentNames: { parentAName: "Alice", parentBName: "Bob" },
    loading: false,
    updateParentNames: vi.fn(),
  });
  mockUseAuth.mockReturnValue({
    authHeader: () => ({ Authorization: "Bearer test-token" }),
    token: "test-token",
    user: null,
    isAuthenticated: true,
    loginAdmin: vi.fn(),
    exchangeMagicToken: vi.fn(),
    logout: vi.fn(),
  });
}

/* ---------- tests ---------- */

describe("Statistics", () => {
  beforeEach(() => {
    vi.stubGlobal("fetch", vi.fn() as unknown as typeof globalThis.fetch);
    setupDefaults();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("renders loading skeletons initially", () => {
    // Never resolve fetch to keep loading
    vi.mocked(globalThis.fetch).mockReturnValue(new Promise(() => {}));

    renderWithTheme(<Statistics />);

    expect(screen.getByTestId("statistics")).toBeInTheDocument();
    // Skeletons are visible during loading
    const skeletons = document.querySelectorAll(".MuiSkeleton-root");
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("renders stat cards with correct values after loading", async () => {
    vi.mocked(globalThis.fetch).mockResolvedValueOnce(
      new Response(JSON.stringify(statsResponse), { status: 200 }),
    );

    renderWithTheme(<Statistics />);

    await waitFor(() => {
      // parentADays=120 and unassignedDays=120 both render
      expect(screen.getAllByText("120")).toHaveLength(2);
    });

    // Parent A days
    expect(screen.getByText("Alice")).toBeInTheDocument();
    // Parent B days
    expect(screen.getByText("Bob")).toBeInTheDocument();
    // VAB days
    expect(screen.getByText("15")).toBeInTheDocument();
    expect(screen.getByText(/VAB/i)).toBeInTheDocument();
    // Days with comments
    expect(screen.getByText("25")).toBeInTheDocument();
  });

  it("uses parent names from ConfigContext", async () => {
    mockUseConfig.mockReturnValue({
      parentNames: { parentAName: "Tomas", parentBName: "Caroline" },
      loading: false,
      updateParentNames: vi.fn(),
    });

    vi.mocked(globalThis.fetch).mockResolvedValueOnce(
      new Response(JSON.stringify(statsResponse), { status: 200 }),
    );

    renderWithTheme(<Statistics />);

    await waitFor(() => {
      expect(screen.getByText("Tomas")).toBeInTheDocument();
    });
    expect(screen.getByText("Caroline")).toBeInTheDocument();
  });

  it("shows error alert when fetch fails", async () => {
    vi.mocked(globalThis.fetch).mockResolvedValueOnce(
      new Response("Server Error", { status: 500, statusText: "Internal Server Error" }),
    );

    renderWithTheme(<Statistics />);

    await waitFor(() => {
      expect(screen.getByRole("alert")).toBeInTheDocument();
    });
  });

  it("fetches statistics for the current year", async () => {
    mockUseCalendar.mockReturnValue({ currentYear: 2025 });

    vi.mocked(globalThis.fetch).mockResolvedValueOnce(
      new Response(JSON.stringify(statsResponse), { status: 200 }),
    );

    renderWithTheme(<Statistics />);

    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        expect.stringContaining("/api/statistics/2025"),
        expect.any(Object),
      );
    });
  });

  it("re-fetches when currentYear changes", async () => {
    vi.mocked(globalThis.fetch).mockResolvedValue(
      new Response(JSON.stringify(statsResponse), { status: 200 }),
    );

    mockUseCalendar.mockReturnValue({ currentYear: 2025 });
    const { rerender } = renderWithTheme(<Statistics />);

    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        expect.stringContaining("/api/statistics/2025"),
        expect.any(Object),
      );
    });

    mockUseCalendar.mockReturnValue({ currentYear: 2026 });
    rerender(
      <ThemeProvider theme={theme}>
        <Statistics />
      </ThemeProvider>,
    );

    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        expect.stringContaining("/api/statistics/2026"),
        expect.any(Object),
      );
    });
  });

  it("has data-testid on the container", () => {
    vi.mocked(globalThis.fetch).mockReturnValue(new Promise(() => {}));

    renderWithTheme(<Statistics />);

    expect(screen.getByTestId("statistics")).toBeInTheDocument();
  });

  it("displays each stat with Typography h3 for the number", async () => {
    vi.mocked(globalThis.fetch).mockResolvedValueOnce(
      new Response(JSON.stringify(statsResponse), { status: 200 }),
    );

    renderWithTheme(<Statistics />);

    await waitFor(() => {
      expect(screen.getAllByText("120")).toHaveLength(2);
    });

    // All stat values should be rendered as h3 elements
    const h3Elements = document.querySelectorAll("h3");
    expect(h3Elements.length).toBe(5);
  });
});
