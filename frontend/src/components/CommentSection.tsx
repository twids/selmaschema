import { useState } from "react";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  List,
  ListItem,
  ListItemText,
  TextField,
  Typography,
} from "@mui/material";
import { useAuth } from "../auth/AuthContext";
import { addComment } from "../api/comments";
import { sv } from "../i18n/sv";
import { formatSwedishTime } from "../i18n/sv";
import type { CommentDto } from "../api/types";

interface CommentSectionProps {
  parent: "A" | "B";
  parentName: string;
  comments: CommentDto[];
  dayAssignmentId: number | undefined;
  onCommentAdded: (comment: CommentDto) => void;
}

export default function CommentSection({
  parent,
  parentName,
  comments,
  dayAssignmentId,
  onCommentAdded,
}: CommentSectionProps) {
  const { authHeader } = useAuth();

  const [text, setText] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const disabled = dayAssignmentId === undefined;

  const handleSubmit = async () => {
    const trimmed = text.trim();
    if (!trimmed || disabled || dayAssignmentId === undefined) return;

    setSubmitting(true);
    setError(null);

    try {
      const created = await addComment(
        dayAssignmentId,
        { parent, commentText: trimmed },
        authHeader,
      );
      onCommentAdded(created);
      setText("");
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setSubmitting(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && e.ctrlKey) {
      void handleSubmit();
    }
  };

  return (
    <Box>
      <Typography variant="subtitle2" sx={{ mb: 1 }}>
        Kommentarer – {parentName}
      </Typography>

      {comments.length === 0 ? (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
          {sv.comments.noComments}
        </Typography>
      ) : (
        <List dense disablePadding>
          {comments.map((c) => (
            <ListItem key={c.id} disableGutters>
              <ListItemText
                primary={c.commentText}
                secondary={formatSwedishTime(c.createdAt)}
              />
            </ListItem>
          ))}
        </List>
      )}

      <Box sx={{ display: "flex", gap: 1, mt: 1 }}>
        <TextField
          size="small"
          fullWidth
          placeholder={sv.comments.commentPlaceholder}
          value={text}
          onChange={(e) => setText(e.target.value)}
          onKeyDown={handleKeyDown}
          disabled={disabled || submitting}
        />
        <Button
          variant="outlined"
          size="small"
          onClick={handleSubmit}
          disabled={disabled || submitting}
          startIcon={submitting ? <CircularProgress size={16} /> : undefined}
        >
          {sv.comments.addComment}
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mt: 1 }}>
          {error}
        </Alert>
      )}
    </Box>
  );
}
