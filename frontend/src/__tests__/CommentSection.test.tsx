import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ThemeProvider } from "@mui/material/styles";
import { theme } from "../theme";
import CommentSection from "../components/CommentSection";
import type { CommentDto } from "../api/types";

/* ---------- mock addComment API ---------- */

const mockAddComment = vi.fn();

vi.mock("../api/comments", () => ({
  addComment: (...args: unknown[]) => mockAddComment(...args),
}));

/* ---------- mock auth ---------- */

const mockAuthHeader = () => ({ Authorization: "Bearer test-token" });

vi.mock("../auth/AuthContext", () => ({
  useAuth: () => ({
    authHeader: mockAuthHeader,
    token: "test-token",
    user: null,
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

const sampleComments: CommentDto[] = [
  {
    id: 1,
    dayAssignmentId: 10,
    parent: "A",
    commentText: "First comment",
    createdAt: "2026-02-07T10:30:00Z",
    modifiedAt: null,
  },
  {
    id: 2,
    dayAssignmentId: 10,
    parent: "A",
    commentText: "Second comment",
    createdAt: "2026-02-07T14:15:00Z",
    modifiedAt: null,
  },
];

/* ---------- tests ---------- */

describe("CommentSection", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockAddComment.mockResolvedValue({
      id: 99,
      dayAssignmentId: 10,
      parent: "A",
      commentText: "New comment",
      createdAt: "2026-02-07T16:00:00Z",
      modifiedAt: null,
    } satisfies CommentDto);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe("rendering", () => {
    it("shows header with parent name", () => {
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );
      expect(screen.getByText(/Tomas/)).toBeInTheDocument();
    });

    it("renders existing comments", () => {
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={sampleComments}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );
      expect(screen.getByText("First comment")).toBeInTheDocument();
      expect(screen.getByText("Second comment")).toBeInTheDocument();
    });

    it("shows formatted Swedish timestamps for comments", () => {
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={sampleComments}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );
      const expectedTime = new Date(sampleComments[0].createdAt).toLocaleTimeString(
        "sv-SE",
        { hour: "2-digit", minute: "2-digit" },
      );
      expect(screen.getByText(expectedTime)).toBeInTheDocument();
    });

    it("shows empty state when no comments", () => {
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );
      expect(screen.getByText("Inga kommentarer")).toBeInTheDocument();
    });

    it("shows text field for new comment", () => {
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );
      expect(
        screen.getByPlaceholderText("Lägg till kommentar..."),
      ).toBeInTheDocument();
    });

    it("shows add button", () => {
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );
      expect(
        screen.getByRole("button", { name: /Lägg till/i }),
      ).toBeInTheDocument();
    });
  });

  describe("disabled state", () => {
    it("disables input and button when dayAssignmentId is undefined", () => {
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={undefined}
          onCommentAdded={vi.fn()}
        />,
      );
      expect(
        screen.getByPlaceholderText("Lägg till kommentar..."),
      ).toBeDisabled();
      expect(
        screen.getByRole("button", { name: /Lägg till/i }),
      ).toBeDisabled();
    });
  });

  describe("adding comments", () => {
    it("calls addComment API on button click with correct params", async () => {
      const onCommentAdded = vi.fn();
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={onCommentAdded}
        />,
      );

      const input = screen.getByPlaceholderText("Lägg till kommentar...");
      await userEvent.type(input, "New comment");
      fireEvent.click(screen.getByRole("button", { name: /Lägg till/i }));

      await waitFor(() => {
        expect(mockAddComment).toHaveBeenCalledWith(
          10,
          { parent: "A", commentText: "New comment" },
          mockAuthHeader,
        );
      });
    });

    it("calls onCommentAdded callback with returned comment", async () => {
      const onCommentAdded = vi.fn();
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={onCommentAdded}
        />,
      );

      const input = screen.getByPlaceholderText("Lägg till kommentar...");
      await userEvent.type(input, "New comment");
      fireEvent.click(screen.getByRole("button", { name: /Lägg till/i }));

      await waitFor(() => {
        expect(onCommentAdded).toHaveBeenCalledWith(
          expect.objectContaining({
            id: 99,
            commentText: "New comment",
            parent: "A",
          }),
        );
      });
    });

    it("clears input after successful submission", async () => {
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );

      const input = screen.getByPlaceholderText("Lägg till kommentar...");
      await userEvent.type(input, "New comment");
      fireEvent.click(screen.getByRole("button", { name: /Lägg till/i }));

      await waitFor(() => {
        expect(input).toHaveValue("");
      });
    });

    it("does not submit when input is empty", async () => {
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );

      fireEvent.click(screen.getByRole("button", { name: /Lägg till/i }));

      expect(mockAddComment).not.toHaveBeenCalled();
    });

    it("does not submit when input is only whitespace", async () => {
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );

      const input = screen.getByPlaceholderText("Lägg till kommentar...");
      await userEvent.type(input, "   ");
      fireEvent.click(screen.getByRole("button", { name: /Lägg till/i }));

      expect(mockAddComment).not.toHaveBeenCalled();
    });
  });

  describe("Ctrl+Enter shortcut", () => {
    it("submits comment on Ctrl+Enter", async () => {
      const onCommentAdded = vi.fn();
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={onCommentAdded}
        />,
      );

      const input = screen.getByPlaceholderText("Lägg till kommentar...");
      await userEvent.type(input, "Ctrl enter comment");
      fireEvent.keyDown(input, { key: "Enter", ctrlKey: true });

      await waitFor(() => {
        expect(mockAddComment).toHaveBeenCalledWith(
          10,
          { parent: "A", commentText: "Ctrl enter comment" },
          mockAuthHeader,
        );
      });
    });

    it("does not submit on plain Enter", async () => {
      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );

      const input = screen.getByPlaceholderText("Lägg till kommentar...");
      await userEvent.type(input, "Just enter");
      fireEvent.keyDown(input, { key: "Enter" });

      expect(mockAddComment).not.toHaveBeenCalled();
    });
  });

  describe("loading state", () => {
    it("disables button during submission", async () => {
      mockAddComment.mockImplementation(() => new Promise(() => {})); // never resolves

      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );

      const input = screen.getByPlaceholderText("Lägg till kommentar...");
      await userEvent.type(input, "Loading test");
      fireEvent.click(screen.getByRole("button", { name: /Lägg till/i }));

      await waitFor(() => {
        expect(
          screen.getByRole("button", { name: /Lägg till/i }),
        ).toBeDisabled();
      });
    });

    it("disables input during submission", async () => {
      mockAddComment.mockImplementation(() => new Promise(() => {}));

      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );

      const input = screen.getByPlaceholderText("Lägg till kommentar...");
      await userEvent.type(input, "Loading test");
      fireEvent.click(screen.getByRole("button", { name: /Lägg till/i }));

      await waitFor(() => {
        expect(input).toBeDisabled();
      });
    });
  });

  describe("error handling", () => {
    it("shows error alert when submission fails", async () => {
      mockAddComment.mockRejectedValue(new Error("Network failure"));

      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );

      const input = screen.getByPlaceholderText("Lägg till kommentar...");
      await userEvent.type(input, "Error test");
      fireEvent.click(screen.getByRole("button", { name: /Lägg till/i }));

      await waitFor(() => {
        expect(screen.getByRole("alert")).toBeInTheDocument();
        expect(screen.getByText(/Network failure/)).toBeInTheDocument();
      });
    });

    it("re-enables input and button after error", async () => {
      mockAddComment.mockRejectedValue(new Error("Oops"));

      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );

      const input = screen.getByPlaceholderText("Lägg till kommentar...");
      await userEvent.type(input, "Error test");
      fireEvent.click(screen.getByRole("button", { name: /Lägg till/i }));

      await waitFor(() => {
        expect(input).toBeEnabled();
        expect(
          screen.getByRole("button", { name: /Lägg till/i }),
        ).toBeEnabled();
      });
    });

    it("preserves input text after error", async () => {
      mockAddComment.mockRejectedValue(new Error("Oops"));

      renderWithTheme(
        <CommentSection
          parent="A"
          parentName="Tomas"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );

      const input = screen.getByPlaceholderText("Lägg till kommentar...");
      await userEvent.type(input, "Preserved text");
      fireEvent.click(screen.getByRole("button", { name: /Lägg till/i }));

      await waitFor(() => {
        expect(input).toHaveValue("Preserved text");
      });
    });
  });

  describe("parent B", () => {
    it("renders with parent B name", () => {
      renderWithTheme(
        <CommentSection
          parent="B"
          parentName="Carro"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );
      expect(screen.getByText(/Carro/)).toBeInTheDocument();
    });

    it("sends parent B when adding comment", async () => {
      renderWithTheme(
        <CommentSection
          parent="B"
          parentName="Carro"
          comments={[]}
          dayAssignmentId={10}
          onCommentAdded={vi.fn()}
        />,
      );

      const input = screen.getByPlaceholderText("Lägg till kommentar...");
      await userEvent.type(input, "B comment");
      fireEvent.click(screen.getByRole("button", { name: /Lägg till/i }));

      await waitFor(() => {
        expect(mockAddComment).toHaveBeenCalledWith(
          10,
          { parent: "B", commentText: "B comment" },
          mockAuthHeader,
        );
      });
    });
  });
});
