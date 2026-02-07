import { useEffect, useState } from "react";
import {
  Alert,
  Button,
  Snackbar,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useConfig } from "../context/ConfigContext";

/** Editor for changing the display names of Parent A and Parent B. */
export default function ParentNameEditor() {
  const { parentNames, updateParentNames } = useConfig();

  const [parentA, setParentA] = useState(parentNames.parentAName);
  const [parentB, setParentB] = useState(parentNames.parentBName);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  // Sync local state when config changes externally
  useEffect(() => {
    setParentA(parentNames.parentAName);
    setParentB(parentNames.parentBName);
  }, [parentNames.parentAName, parentNames.parentBName]);

  async function handleSave() {
    setSaving(true);
    setError(null);
    setSuccess(false);
    try {
      await updateParentNames(parentA, parentB);
      setSuccess(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <Stack
      data-testid="parent-name-editor"
      spacing={2}
      sx={{ mt: 3, maxWidth: 400 }}
    >
      <Typography variant="h6">Föräldranamn</Typography>

      <TextField
        label="Förälder A"
        value={parentA}
        onChange={(e) => setParentA(e.target.value)}
        size="small"
        fullWidth
      />

      <TextField
        label="Förälder B"
        value={parentB}
        onChange={(e) => setParentB(e.target.value)}
        size="small"
        fullWidth
      />

      {error && (
        <Alert severity="error">{error}</Alert>
      )}

      <Button
        variant="contained"
        onClick={handleSave}
        disabled={saving}
      >
        Spara
      </Button>

      <Snackbar
        open={success}
        autoHideDuration={3000}
        onClose={() => setSuccess(false)}
      >
        <Alert severity="success" onClose={() => setSuccess(false)}>
          Namn uppdaterade
        </Alert>
      </Snackbar>
    </Stack>
  );
}
