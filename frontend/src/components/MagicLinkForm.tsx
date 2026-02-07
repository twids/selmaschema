import { useCallback, useState } from "react";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  type SelectChangeEvent,
} from "@mui/material";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import { useAuth } from "../auth/AuthContext";
import { createMagicLink, type MagicLinkResponse } from "../api/admin";

interface MagicLinkFormProps {
  /** Called after a magic link is successfully created. */
  onCreated: () => void;
}

/** Form to generate a new magic-link invitation. */
export default function MagicLinkForm({ onCreated }: MagicLinkFormProps) {
  const { authHeader } = useAuth();

  const [email, setEmail] = useState("");
  const [role, setRole] = useState("ParentA");
  const [displayName, setDisplayName] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [createdLink, setCreatedLink] = useState<MagicLinkResponse | null>(null);

  const canSubmit = email.trim().length > 0 && !loading;

  const handleRoleChange = useCallback((e: SelectChangeEvent) => {
    setRole(e.target.value);
  }, []);

  const handleSubmit = useCallback(async () => {
    setLoading(true);
    setError(null);
    setCreatedLink(null);

    try {
      const result = await createMagicLink(
        { email: email.trim(), role, displayName: displayName.trim() },
        authHeader,
      );
      setCreatedLink(result);
      setEmail("");
      setDisplayName("");
      onCreated();
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : "Okänt fel uppstod";
      setError(message);
    } finally {
      setLoading(false);
    }
  }, [email, role, displayName, authHeader, onCreated]);

  const handleCopy = useCallback(() => {
    if (createdLink) {
      navigator.clipboard.writeText(createdLink.magicLink);
    }
  }, [createdLink]);

  return (
    <Box data-testid="magic-link-form">
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns: { xs: "1fr", sm: "1fr 160px 1fr auto" },
          alignItems: "start",
        }}
      >
        <TextField
          label="E-post"
          type="email"
          size="small"
          required
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />

        <FormControl size="small">
          <InputLabel id="role-label">Roll</InputLabel>
          <Select
            labelId="role-label"
            label="Roll"
            value={role}
            onChange={handleRoleChange}
          >
            <MenuItem value="ParentA">Förälder A</MenuItem>
            <MenuItem value="ParentB">Förälder B</MenuItem>
          </Select>
        </FormControl>

        <TextField
          label="Visningsnamn"
          size="small"
          value={displayName}
          onChange={(e) => setDisplayName(e.target.value)}
        />

        <Button
          variant="contained"
          disabled={!canSubmit}
          onClick={handleSubmit}
          startIcon={loading ? <CircularProgress size={16} /> : undefined}
        >
          {loading ? "Skapar…" : "Skapa magisk länk"}
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mt: 2 }}>
          Kunde inte skapa magisk länk: {error}
        </Alert>
      )}

      {createdLink && (
        <Alert
          severity="success"
          sx={{ mt: 2, wordBreak: "break-all" }}
          action={
            <IconButton
              aria-label="Kopiera"
              size="small"
              onClick={handleCopy}
            >
              <ContentCopyIcon fontSize="small" />
            </IconButton>
          }
        >
          Länk skapad: {createdLink.magicLink}
        </Alert>
      )}
    </Box>
  );
}
