import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { ThemeProvider } from "@mui/material/styles";
import { theme } from "../theme";
import MagicLinkForm from "../components/MagicLinkForm";
import type { MagicLinkResponse } from "../api/admin";

/* ---------- mocks ---------- */

const mockCreateMagicLink = vi.fn();

vi.mock("../api/admin", () => ({
  createMagicLink: (...args: unknown[]) => mockCreateMagicLink(...args),
}));

vi.mock("../auth/AuthContext", () => ({
  useAuth: () => ({
    authHeader: () => ({ Authorization: "Bearer test-token" }),
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

const successResponse: MagicLinkResponse = {
  token: "abc123",
  magicLink: "https://app.test/auth/magic?token=abc123",
  email: "parent@example.com",
  role: "ParentA",
  displayName: "Alice",
  expiresAt: "2026-02-14T12:00:00Z",
  createdAt: "2026-02-07T12:00:00Z",
};

/* ---------- tests ---------- */

describe("MagicLinkForm", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    Object.assign(navigator, {
      clipboard: { writeText: vi.fn().mockResolvedValue(undefined) },
    });
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("renders form with all fields", () => {
    renderWithTheme(<MagicLinkForm onCreated={vi.fn()} />);

    expect(screen.getByTestId("magic-link-form")).toBeInTheDocument();
    expect(screen.getByLabelText(/e-post/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/roll/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/visningsnamn/i)).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /skapa magisk länk/i }),
    ).toBeInTheDocument();
  });

  it("has ParentA selected as default role", () => {
    renderWithTheme(<MagicLinkForm onCreated={vi.fn()} />);

    // MUI Select renders the selected value as text
    expect(screen.getByText("Förälder A")).toBeInTheDocument();
  });

  it("disables submit button when email is empty", () => {
    renderWithTheme(<MagicLinkForm onCreated={vi.fn()} />);

    const btn = screen.getByRole("button", { name: /skapa magisk länk/i });
    expect(btn).toBeDisabled();
  });

  it("enables submit button when email is filled", () => {
    renderWithTheme(<MagicLinkForm onCreated={vi.fn()} />);

    fireEvent.change(screen.getByLabelText(/e-post/i), {
      target: { value: "parent@example.com" },
    });

    const btn = screen.getByRole("button", { name: /skapa magisk länk/i });
    expect(btn).toBeEnabled();
  });

  it("submits the form and shows success with magic link", async () => {
    mockCreateMagicLink.mockResolvedValueOnce(successResponse);
    const onCreated = vi.fn();
    renderWithTheme(<MagicLinkForm onCreated={onCreated} />);

    fireEvent.change(screen.getByLabelText(/e-post/i), {
      target: { value: "parent@example.com" },
    });
    fireEvent.change(screen.getByLabelText(/visningsnamn/i), {
      target: { value: "Alice" },
    });

    fireEvent.click(
      screen.getByRole("button", { name: /skapa magisk länk/i }),
    );

    await waitFor(() => {
      expect(mockCreateMagicLink).toHaveBeenCalledWith(
        { email: "parent@example.com", role: "ParentA", displayName: "Alice" },
        expect.any(Function),
      );
    });

    await waitFor(() => {
      expect(screen.getByText(/länk skapad/i)).toBeInTheDocument();
    });

    expect(onCreated).toHaveBeenCalled();
  });

  it("shows loading state during creation", async () => {
    let resolvePromise!: (value: MagicLinkResponse) => void;
    mockCreateMagicLink.mockReturnValueOnce(
      new Promise<MagicLinkResponse>((resolve) => {
        resolvePromise = resolve;
      }),
    );

    renderWithTheme(<MagicLinkForm onCreated={vi.fn()} />);

    fireEvent.change(screen.getByLabelText(/e-post/i), {
      target: { value: "parent@example.com" },
    });

    fireEvent.click(
      screen.getByRole("button", { name: /skapa magisk länk/i }),
    );

    await waitFor(() => {
      expect(
        screen.getByRole("button", { name: /skapar/i }),
      ).toBeInTheDocument();
    });

    resolvePromise(successResponse);

    await waitFor(() => {
      expect(screen.getByText(/länk skapad/i)).toBeInTheDocument();
    });
  });

  it("shows error alert on failure", async () => {
    mockCreateMagicLink.mockRejectedValueOnce(new Error("API 400 Bad Request"));
    renderWithTheme(<MagicLinkForm onCreated={vi.fn()} />);

    fireEvent.change(screen.getByLabelText(/e-post/i), {
      target: { value: "bad@example.com" },
    });

    fireEvent.click(
      screen.getByRole("button", { name: /skapa magisk länk/i }),
    );

    await waitFor(() => {
      expect(screen.getByRole("alert")).toBeInTheDocument();
      expect(
        screen.getByText(/kunde inte skapa magisk länk/i),
      ).toBeInTheDocument();
    });
  });

  it("has a copy button in success alert", async () => {
    mockCreateMagicLink.mockResolvedValueOnce(successResponse);
    renderWithTheme(<MagicLinkForm onCreated={vi.fn()} />);

    fireEvent.change(screen.getByLabelText(/e-post/i), {
      target: { value: "parent@example.com" },
    });

    fireEvent.click(
      screen.getByRole("button", { name: /skapa magisk länk/i }),
    );

    await waitFor(() => {
      expect(screen.getByText(/länk skapad/i)).toBeInTheDocument();
    });

    const copyBtn = screen.getByRole("button", { name: /kopiera/i });
    expect(copyBtn).toBeInTheDocument();
  });
});
