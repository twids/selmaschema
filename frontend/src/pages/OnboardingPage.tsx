import { Alert, Box, Button, Card, CardContent, Divider, Stack, TextField, Typography } from "@mui/material";
import { useState } from "react";
import { Navigate, useNavigate } from "react-router-dom";
import { familyApi } from "../api/v2";
import { useAuth } from "../auth/AuthContext";

export default function OnboardingPage() {
  const { memberships, refresh } = useAuth();
  const navigate = useNavigate();
  const [familyName, setFamilyName] = useState("");
  const [code, setCode] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [createdFamilyId, setCreatedFamilyId] = useState<string | null>(null);

  const activeMembership = memberships.find((membership) => membership.status === "Active");
  if (activeMembership && !createdFamilyId) return <Navigate to={`/families/${activeMembership.familyId}`} replace />;

  const createFamily = async () => {
    setBusy(true); setError(null);
    let family: Awaited<ReturnType<typeof familyApi.create>>;
    try {
      family = await familyApi.create(familyName);
    } catch {
      setError("Familjen kunde inte skapas. Kontrollera namnet och försök igen.");
      setBusy(false);
      return;
    }
    setCreatedFamilyId(family.id);
    try {
      await refresh();
      navigate(`/families/${family.id}/setup`);
    } catch {
      setError("Familjen skapades, men sidan kunde inte uppdateras. Ladda om sidan för att fortsätta konfigurationen.");
    } finally { setBusy(false); }
  };

  const joinFamily = async () => {
    setError(null);
    navigate("/join", { state: { invitationCode: code } });
  };

  return (
    <Box sx={{ maxWidth: 900, mx: "auto" }}>
      <Typography variant="h3" fontWeight={700} gutterBottom>Välkommen till Selma</Typography>
      <Typography color="text.secondary" sx={{ mb: 4 }}>Börja med en ny familj eller anslut till en du blivit inbjuden till.</Typography>
      {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
      <Stack direction={{ xs: "column", md: "row" }} spacing={3} alignItems="stretch">
        <Card sx={{ flex: 1 }}><CardContent sx={{ p: 4 }}>
          <Typography variant="h5" gutterBottom>Skapa familj</Typography>
          <Typography color="text.secondary" sx={{ mb: 3 }}>Du blir familjens ägare. Barn och schema läggs till i nästa steg.</Typography>
          <TextField fullWidth label="Familjens namn" value={familyName} onChange={(e) => setFamilyName(e.target.value)} sx={{ mb: 2 }} />
          <Button variant="contained" disabled={busy || !familyName.trim()} onClick={() => void createFamily()}>Skapa familj</Button>
        </CardContent></Card>
        <Divider orientation="vertical" flexItem sx={{ display: { xs: "none", md: "block" } }} />
        <Card sx={{ flex: 1 }}><CardContent sx={{ p: 4 }}>
          <Typography variant="h5" gutterBottom>Gå med med kod</Typography>
          <Typography color="text.secondary" sx={{ mb: 3 }}>Skriv engångskoden du fått av familjens ägare.</Typography>
          <TextField fullWidth label="Inbjudningskod" value={code} onChange={(e) => setCode(e.target.value.toUpperCase())} sx={{ mb: 2 }} />
          <Button variant="outlined" disabled={busy || !code.trim()} onClick={() => void joinFamily()}>Fortsätt</Button>
        </CardContent></Card>
      </Stack>
    </Box>
  );
}
