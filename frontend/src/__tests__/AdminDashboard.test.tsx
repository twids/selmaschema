import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { ThemeProvider } from "@mui/material/styles";
import { theme } from "../theme";
import AdminDashboard from "../components/AdminDashboard";

/* ---------- mocks ---------- */

const mockAuthHeader = vi.fn(() => ({ Authorization: "Bearer test-token" }));

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

const mockCreateMagicLink = vi.fn();
const mockGetMagicLinks = vi.fn();
const mockGetUsers = vi.fn();

vi.mock("../api/admin", () => ({
  createMagicLink: (...args: unknown[]) => mockCreateMagicLink(...args),
  getMagicLinks: (...args: unknown[]) => mockGetMagicLinks(...args),
  getUsers: (...args: unknown[]) => mockGetUsers(...args),
}));

/* ---------- helpers ---------- */

function renderWithTheme(ui: React.ReactElement) {
  return render(<ThemeProvider theme={theme}>{ui}</ThemeProvider>);
}

/* ---------- tests ---------- */

describe("AdminDashboard", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockGetMagicLinks.mockResolvedValue([]);
    mockGetUsers.mockResolvedValue([]);
    Object.assign(navigator, {
      clipboard: { writeText: vi.fn().mockResolvedValue(undefined) },
    });
  });

  it("renders with data-testid", async () => {
    renderWithTheme(<AdminDashboard />);

    await waitFor(() => {
      expect(screen.getByTestId("admin-dashboard")).toBeInTheDocument();
    });
  });

  it("displays the Administration title", async () => {
    renderWithTheme(<AdminDashboard />);

    await waitFor(() => {
      expect(screen.getByText("Administration")).toBeInTheDocument();
    });
  });

  it("contains MagicLinkForm section", async () => {
    renderWithTheme(<AdminDashboard />);

    await waitFor(() => {
      expect(screen.getByTestId("magic-link-form")).toBeInTheDocument();
    });
  });

  it("contains MagicLinksTable section", async () => {
    renderWithTheme(<AdminDashboard />);

    await waitFor(() => {
      expect(screen.getByTestId("magic-links-table")).toBeInTheDocument();
    });
  });

  it("contains UsersTable section", async () => {
    renderWithTheme(<AdminDashboard />);

    await waitFor(() => {
      expect(screen.getByTestId("users-table")).toBeInTheDocument();
    });
  });

  it("renders section headers for each table", async () => {
    renderWithTheme(<AdminDashboard />);

    await waitFor(() => {
      // "Skapa magisk länk" appears as both a heading and button text,
      // so use getAllByText and verify at least the heading is present.
      const headings = screen.getAllByText(/skapa magisk länk/i);
      expect(headings.length).toBeGreaterThanOrEqual(1);
      expect(screen.getByText(/aktiva magiska länkar/i)).toBeInTheDocument();
      expect(screen.getByText(/användare/i)).toBeInTheDocument();
    });
  });
});
