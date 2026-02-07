import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { ThemeProvider } from "@mui/material/styles";
import { theme } from "../theme";
import ParentNameEditor from "../components/ParentNameEditor";

/* ---------- mocks ---------- */

const mockUpdateParentNames = vi.fn();
const mockUseConfig = vi.fn();

vi.mock("../context/ConfigContext", () => ({
  useConfig: () => mockUseConfig(),
}));

/* ---------- helpers ---------- */

function renderWithTheme(ui: React.ReactElement) {
  return render(<ThemeProvider theme={theme}>{ui}</ThemeProvider>);
}

function setupDefaults() {
  mockUseConfig.mockReturnValue({
    parentNames: { parentAName: "Alice", parentBName: "Bob" },
    loading: false,
    updateParentNames: mockUpdateParentNames,
  });
  mockUpdateParentNames.mockReset();
}

/* ---------- tests ---------- */

describe("ParentNameEditor", () => {
  beforeEach(() => {
    setupDefaults();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("renders the editor with data-testid", () => {
    renderWithTheme(<ParentNameEditor />);

    expect(screen.getByTestId("parent-name-editor")).toBeInTheDocument();
  });

  it("renders two text fields pre-filled with parent names", () => {
    renderWithTheme(<ParentNameEditor />);

    const inputs = screen.getAllByRole("textbox");
    expect(inputs).toHaveLength(2);
    expect(inputs[0]).toHaveValue("Alice");
    expect(inputs[1]).toHaveValue("Bob");
  });

  it("renders a save button", () => {
    renderWithTheme(<ParentNameEditor />);

    expect(screen.getByRole("button", { name: /spara/i })).toBeInTheDocument();
  });

  it("calls updateParentNames on save", async () => {
    mockUpdateParentNames.mockResolvedValueOnce(undefined);

    renderWithTheme(<ParentNameEditor />);

    const inputs = screen.getAllByRole("textbox");
    fireEvent.change(inputs[0], { target: { value: "Tomas" } });
    fireEvent.change(inputs[1], { target: { value: "Caroline" } });

    fireEvent.click(screen.getByRole("button", { name: /spara/i }));

    await waitFor(() => {
      expect(mockUpdateParentNames).toHaveBeenCalledWith("Tomas", "Caroline");
    });
  });

  it("shows success snackbar after save", async () => {
    mockUpdateParentNames.mockResolvedValueOnce(undefined);

    renderWithTheme(<ParentNameEditor />);

    fireEvent.click(screen.getByRole("button", { name: /spara/i }));

    await waitFor(() => {
      expect(screen.getByRole("alert")).toBeInTheDocument();
    });
  });

  it("shows error alert on save failure", async () => {
    mockUpdateParentNames.mockRejectedValueOnce(new Error("Server Error"));

    renderWithTheme(<ParentNameEditor />);

    fireEvent.click(screen.getByRole("button", { name: /spara/i }));

    await waitFor(() => {
      expect(screen.getByRole("alert")).toBeInTheDocument();
    });
  });

  it("shows loading state during save", async () => {
    // Create a promise that doesn't resolve immediately
    let resolveUpdate!: () => void;
    mockUpdateParentNames.mockReturnValue(
      new Promise<void>((resolve) => {
        resolveUpdate = resolve;
      }),
    );

    renderWithTheme(<ParentNameEditor />);

    fireEvent.click(screen.getByRole("button", { name: /spara/i }));

    // Button should show loading indicator
    await waitFor(() => {
      const saveButton = screen.getByRole("button", { name: /spara/i });
      expect(saveButton).toBeDisabled();
    });

    // Resolve the update
    resolveUpdate();

    await waitFor(() => {
      const saveButton = screen.getByRole("button", { name: /spara/i });
      expect(saveButton).not.toBeDisabled();
    });
  });

  it("updates text fields when parent names change in config", () => {
    const { rerender } = renderWithTheme(<ParentNameEditor />);

    const inputs = screen.getAllByRole("textbox");
    expect(inputs[0]).toHaveValue("Alice");

    mockUseConfig.mockReturnValue({
      parentNames: { parentAName: "Tomas", parentBName: "Caroline" },
      loading: false,
      updateParentNames: mockUpdateParentNames,
    });

    rerender(
      <ThemeProvider theme={theme}>
        <ParentNameEditor />
      </ThemeProvider>,
    );

    const updatedInputs = screen.getAllByRole("textbox");
    expect(updatedInputs[0]).toHaveValue("Tomas");
    expect(updatedInputs[1]).toHaveValue("Caroline");
  });
});
