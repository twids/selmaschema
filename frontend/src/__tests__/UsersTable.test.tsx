import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { ThemeProvider } from "@mui/material/styles";
import { theme } from "../theme";
import UsersTable from "../components/UsersTable";
import type { UserDto } from "../api/admin";

/* ---------- mocks ---------- */

const mockGetUsers = vi.fn();

vi.mock("../api/admin", () => ({
  getUsers: (...args: unknown[]) => mockGetUsers(...args),
}));

/* ---------- helpers ---------- */

function renderWithTheme(ui: React.ReactElement) {
  return render(<ThemeProvider theme={theme}>{ui}</ThemeProvider>);
}

const sampleUsers: UserDto[] = [
  {
    id: 1,
    email: "admin@example.com",
    role: "Admin",
    displayName: "Administratör",
    lastLoginAt: "2026-02-07T10:30:00Z",
  },
  {
    id: 2,
    email: "parent@example.com",
    role: "ParentA",
    displayName: "Alice",
    lastLoginAt: null,
  },
];

/* ---------- tests ---------- */

describe("UsersTable", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("renders table with data-testid", async () => {
    mockGetUsers.mockResolvedValueOnce(sampleUsers);
    renderWithTheme(<UsersTable refreshKey={0} />);

    await waitFor(() => {
      expect(screen.getByTestId("users-table")).toBeInTheDocument();
    });
  });

  it("shows loading skeleton initially", () => {
    mockGetUsers.mockReturnValueOnce(new Promise(() => {})); // never resolves
    renderWithTheme(<UsersTable refreshKey={0} />);

    const skeletons = screen.getAllByTestId("skeleton-row");
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("renders user rows after loading", async () => {
    mockGetUsers.mockResolvedValueOnce(sampleUsers);
    renderWithTheme(<UsersTable refreshKey={0} />);

    await waitFor(() => {
      expect(screen.getByText("admin@example.com")).toBeInTheDocument();
      expect(screen.getByText("parent@example.com")).toBeInTheDocument();
    });

    expect(screen.getByText("Administratör")).toBeInTheDocument();
    expect(screen.getByText("Alice")).toBeInTheDocument();
  });

  it("shows column headers", async () => {
    mockGetUsers.mockResolvedValueOnce(sampleUsers);
    renderWithTheme(<UsersTable refreshKey={0} />);

    await waitFor(() => {
      expect(screen.getByText("Visningsnamn")).toBeInTheDocument();
      expect(screen.getByText("E-post")).toBeInTheDocument();
      expect(screen.getByText("Roll")).toBeInTheDocument();
      expect(screen.getByText("Senaste inloggning")).toBeInTheDocument();
    });
  });

  it("shows 'Aldrig' for null lastLoginAt", async () => {
    mockGetUsers.mockResolvedValueOnce(sampleUsers);
    renderWithTheme(<UsersTable refreshKey={0} />);

    await waitFor(() => {
      expect(screen.getByText("Aldrig")).toBeInTheDocument();
    });
  });

  it("shows empty state when no users", async () => {
    mockGetUsers.mockResolvedValueOnce([]);
    renderWithTheme(<UsersTable refreshKey={0} />);

    await waitFor(() => {
      expect(
        screen.getByText(/inga användare registrerade/i),
      ).toBeInTheDocument();
    });
  });

  it("refetches when refreshKey changes", async () => {
    mockGetUsers.mockResolvedValue(sampleUsers);
    const { rerender } = renderWithTheme(<UsersTable refreshKey={0} />);

    await waitFor(() => {
      expect(mockGetUsers).toHaveBeenCalledTimes(1);
    });

    rerender(
      <ThemeProvider theme={theme}>
        <UsersTable refreshKey={1} />
      </ThemeProvider>,
    );

    await waitFor(() => {
      expect(mockGetUsers).toHaveBeenCalledTimes(2);
    });
  });
});
