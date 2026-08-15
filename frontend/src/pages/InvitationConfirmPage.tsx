import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { Alert, Box, Button, CircularProgress, Paper, Typography } from "@mui/material";
import { useAuth } from "../auth/AuthContext";
import {
  cancelPendingInvitation,
  getPendingInvitation,
  type PendingInvitationDto,
} from "../api/invitations";

export default function InvitationConfirmPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { completeInvitation } = useAuth();
  const [pending, setPending] = useState<PendingInvitationDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(
    searchParams.get("error") ? "Inbjudningslänken är ogiltig, använd eller utgången." : null,
  );

  useEffect(() => {
    if (searchParams.get("error")) {
      setLoading(false);
      return;
    }
    getPendingInvitation()
      .then(setPending)
      .catch(() => setError("Ingen giltig väntande inbjudan hittades."))
      .finally(() => setLoading(false));
  }, [searchParams]);

  const confirm = async () => {
    setSubmitting(true);
    if (await completeInvitation()) {
      navigate("/", { replace: true });
    } else {
      setError("Inbjudningen kunde inte lösas in. Den kan redan ha använts.");
      setSubmitting(false);
    }
  };

  const cancel = async () => {
    await cancelPendingInvitation().catch(() => undefined);
    navigate("/login", { replace: true });
  };

  if (loading) {
    return <Box sx={{ minHeight: "100vh", display: "grid", placeItems: "center" }}><CircularProgress /></Box>;
  }

  return (
    <Box sx={{ minHeight: "100vh", display: "grid", placeItems: "center", p: 2 }}>
      <Paper sx={{ p: 4, maxWidth: 560, width: "100%" }}>
        <Typography variant="h4" gutterBottom>Bekräfta inbjudan</Typography>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        {pending && (
          <>
            <Typography><strong>Avsedd e-post:</strong> {pending.emailHint || "Ingen angiven"}</Typography>
            <Typography><strong>Verifierad Widsell ID-adress:</strong> {pending.verifiedEmail}</Typography>
            <Typography><strong>Roll:</strong> {pending.role}</Typography>
            {pending.emailHint && pending.emailHint !== pending.verifiedEmail && (
              <Alert severity="warning" sx={{ mt: 2 }}>
                Adresserna skiljer sig. Bekräfta bara om denna inbjudan är avsedd för dig.
              </Alert>
            )}
            <Box sx={{ display: "flex", justifyContent: "flex-end", gap: 2, mt: 3 }}>
              <Button onClick={cancel} disabled={submitting}>Avbryt</Button>
              <Button variant="contained" onClick={confirm} disabled={submitting}>
                {submitting ? "Bekräftar…" : "Bekräfta och fortsätt"}
              </Button>
            </Box>
          </>
        )}
      </Paper>
    </Box>
  );
}
