import { Alert, Box, Button, Card, CardContent, CircularProgress, Stack, TextField, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import { joinApi, type JoinPreviewDto } from "../api/v2";
import { useAuth } from "../auth/AuthContext";

const apiBase = import.meta.env.VITE_API_URL || "";

export default function JoinPage() {
  const { token } = useParams();
  const { account, isAuthenticated, isLoading, refresh, startOidcLogin } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [preview, setPreview] = useState<JoinPreviewDto | null>(null);
  const initialCode = (location.state as { invitationCode?: string } | null)?.invitationCode ?? "";
  const [code, setCode] = useState(initialCode);
  const [pendingCode, setPendingCode] = useState("");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (token && isAuthenticated) {
      void joinApi.preview(token).then(setPreview).catch(() => setError("Inbjudan är ogiltig, använd eller har gått ut."));
    }
  }, [token, isAuthenticated]);

  useEffect(() => {
    if (!token && initialCode && isAuthenticated) {
      void joinApi.previewCode(initialCode)
        .then((value) => { setPreview(value); setPendingCode(initialCode); })
        .catch(() => setError("Koden är ogiltig, använd eller har gått ut."));
    }
  }, [token, initialCode, isAuthenticated]);

  if (isLoading) return <Box sx={{ display: "grid", placeItems: "center", minHeight: 300 }}><CircularProgress /></Box>;
  if (!isAuthenticated) {
    return (
      <Card sx={{ maxWidth: 560, mx: "auto" }}><CardContent sx={{ p: 4 }}>
        <Typography variant="h4" gutterBottom>Gå med i familj</Typography>
        <Typography sx={{ mb: 3 }}>Bekräfta först din identitet med Widsell ID.</Typography>
        <Button variant="contained" onClick={() => {
          if (token) window.location.assign(`${apiBase}/api/join/link/${encodeURIComponent(token)}`);
          else startOidcLogin("/join");
        }}>Logga in och fortsätt</Button>
      </CardContent></Card>
    );
  }

  const completeInvitation = async () => {
    try {
      const result = token ? await joinApi.complete(token) : await joinApi.code(pendingCode);
      if (!result.joined) throw new Error("already member");
      await refresh(); navigate(`/families/${result.familyId}`);
    } catch { setError("Inbjudan kunde inte lösas in."); }
  };
  const previewCode = async () => {
    try {
      const value = await joinApi.previewCode(code);
      setPreview(value); setPendingCode(code); setError(null);
    } catch { setError("Koden är ogiltig, använd eller har gått ut."); }
  };

  return (
    <Card sx={{ maxWidth: 620, mx: "auto" }}><CardContent sx={{ p: 4 }}>
      <Typography variant="h4" gutterBottom>Gå med i familj</Typography>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      {token && !preview && !error && <CircularProgress />}
      {preview && (
        <Stack spacing={2}>
          <Typography>Familj: <strong>{preview.familyName}</strong></Typography>
          <Typography>Din verifierade adress: <strong>{account?.email}</strong></Typography>
          {preview.emailHint && <Typography>Inbjudan var avsedd för: <strong>{preview.emailHint}</strong></Typography>}
          {preview.emailHint && preview.emailHint.toLowerCase() !== account?.email.toLowerCase() && (
            <Alert severity="warning">Adresserna skiljer sig. Du kan fortsätta, men behöver bekräfta det uttryckligen.</Alert>
          )}
          <Typography>Behörighet: {preview.permission}, sida: {preview.side ? `Hem ${preview.side}` : "ingen sida"}</Typography>
          {preview.alreadyMember ? <Alert severity="info">Du är redan medlem. Inbjudan ändrar inte din behörighet.</Alert> :
            <Button variant="contained" onClick={() => void completeInvitation()}>Bekräfta och gå med</Button>}
        </Stack>
      )}
      {!token && !preview && (
        <Stack spacing={2}>
          <TextField label="Inbjudningskod" value={code} onChange={(e) => setCode(e.target.value.toUpperCase())} />
          <Button variant="contained" disabled={!code.trim()} onClick={() => void previewCode()}>Gå med</Button>
        </Stack>
      )}
    </CardContent></Card>
  );
}
