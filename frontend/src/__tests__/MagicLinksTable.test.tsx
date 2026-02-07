import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { ThemeProvider } from "@mui/material/styles";
import { theme } from "../theme";
import MagicLinksTable from "../components/MagicLinksTable";
import type { MagicLinkResponse } from "../api/admin";

/* ---------- mocks ---------- */

const mockGetMagicLinks = vi.fn();
const mockAuthHeader = vi.fn(() => ({ Authorization: "Bearer test-token" }));

vi.mock("../api/admin", () => ({
  getMagicLinks: (...args: unknown[]) => mockGetMagicLinks(...args),
}));

vi.mock("../auth/AuthContext", () => ({
  useAuth: () => ({
    authHeader: mockAuthHeader,
    token: "test-token",
    user: { id: 1, email: "admin@test.com", role: "Admin" },
    isAuthenticated: true,
    loginAdmin: vi.fn(),
    exchangeMagicToken: vi.fn(),
    logout: vi.fn(),
  }),
}));

/* ---------- helpers ---------- */

function renderWithTheme(ui: React.ReactElement) {
  return render(<ThemeProvider theme={theme}>{ui}</ThemeProvider>);
}

const sampleLinks: MagicLinkResponse[] = [
  {
    token: "abc123",
    magicLink: "https://app.test/auth/magic?token=abc123",
    email: "parent@example.com",
    role: "ParentA",
    displayName: "Alice",
    expiresAt: "2026-02-14T12:00:00Z",
    createdAt: "2026-02-07T12:00:00Z",
  },
  {
    token: "def456",
    magicLink: "https://app.test/auth/magic?token=def456",
    email: "other@example.com",
    role: "ParentB",
    displayName: "Bob",
    expiresAt: "2026-02-15T12:00:00Z",
    createdAt: "2026-02-08T12:00:00Z",
  },
];

/* ---------- tests ---------- */

describe("MagicLinksTable", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    Object.assign(navigator, {
      clipboard: { writeText: vi.fn().mockResolvedValue(undefined) },
    });
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("renders table with data-testid", async () => {
    mockGetMagicLinks.mockResolvedValueOnce(sampleLinks);
    renderWithTheme(<MagicLinksTable refreshKey={0} />);

    await waitFor(() => {
      expect(screen.getByTestId("magic-links-table")).toBeInTheDocument();
    });
  });

  it("shows loading skeleton initially", () => {
    mockGetMagicLinks.mockReturnValueOnce(new Promise(() => {})); // never resolves
    renderWithTheme(<MagicLinksTable refreshKey={0} />);

    // Skeletons are rendered during loading
    const skeletons = screen.getAllByTestId("skeleton-row");
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("renders magic link rows after loading", async () => {
    mockGetMagicLinks.mockResolvedValueOnce(sampleLinks);
    renderWithTheme(<MagicLinksTable refreshKey={0} />);

    await waitFor(() => {
      expect(screen.getByText("parent@example.com")).toBeInTheDocument();
      expect(screen.getByText("other@example.com")).toBeInTheDocument();
    });

    expect(screen.getByText("Alice")).toBeInTheDocument();
    expect(screen.getByText("Bob")).toBeInTheDocument();
  });

  it("shows column headers", async () => {
    mockGetMagicLinks.mockResolvedValueOnce(sampleLinks);
    renderWithTheme(<MagicLinksTable refreshKey={0} />);

    await waitFor(() => {
      expect(screen.getByText("E-post")).toBeInTheDocument();
      expect(screen.getByText("Roll")).toBeInTheDocument();
      expect(screen.getByText("Visningsnamn")).toBeInTheDocument();
      expect(screen.getByText("Magisk länk")).toBeInTheDocument();
      expect(screen.getByText("Utgår")).toBeInTheDocument();
    });
  });

  it("shows empty state when no links", async () => {
    mockGetMagicLinks.mockResolvedValueOnce([]);
    renderWithTheme(<MagicLinksTable refreshKey={0} />);

    await waitFor(() => {
      expect(
        screen.getByText(/inga aktiva magiska länkar/i),
      ).toBeInTheDocument();
    });
  });

  it("refetches when refreshKey changes", async () => {
    mockGetMagicLinks.mockResolvedValue(sampleLinks);
    const { rerender } = renderWithTheme(<MagicLinksTable refreshKey={0} />);

    await waitFor(() => {
      expect(mockGetMagicLinks).toHaveBeenCalledTimes(1);
    });

    rerender(
      <ThemeProvider theme={theme}>
        <MagicLinksTable refreshKey={1} />
      </ThemeProvider>,
    );

    await waitFor(() => {
      expect(mockGetMagicLinks).toHaveBeenCalledTimes(2);
    });
  });
});
