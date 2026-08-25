import { Alert, Box, Button, Card, CardActionArea, CardContent, Chip, CircularProgress, LinearProgress, Stack, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { familyApi, type FamilyDto } from "../api/v2";

export default function FamilyDashboardPage() {
  const { familyId = "" } = useParams();
  const [family, setFamily] = useState<FamilyDto | null>(null);
  const [error, setError] = useState(false);

  useEffect(() => {
    setFamily(null); setError(false);
    void familyApi.get(familyId).then(setFamily).catch(() => setError(true));
  }, [familyId]);

  if (error) return <Alert severity="error">Familjen kunde inte hittas.</Alert>;
  if (!family) return <CircularProgress />;

  const setup = [
    { done: family.mySide !== null, label: "Namnge hemmen och välj ditt hem" },
    { done: family.activeChildren > 0, label: "Lägg till minst ett barn" },
    { done: family.hasActiveSchedule, label: "Granska och aktivera ett boendeschema" },
  ];
  const completedSetupSteps = setup.filter((step) => step.done).length;
  const nextSetupStep = setup.find((step) => !step.done);

  return (
    <Stack spacing={4}>
      <Box>
        <Typography variant="h3" fontWeight={700}>{family.name}</Typography>
        <Typography color="text.secondary">{family.sideALabel} · {family.sideBLabel} · {family.timeZoneId}</Typography>
      </Box>
      {family.myPermission === "Owner" && setup.some((step) => !step.done) && (
        <Card variant="outlined" sx={{ borderColor: "primary.main" }}>
          <CardContent>
            <Stack spacing={2}>
              <Box>
                <Typography variant="h6">Fortsätt konfigurera familjen</Typography>
                <Typography color="text.secondary">Nästa steg: {nextSetupStep?.label}</Typography>
              </Box>
              <LinearProgress variant="determinate" value={(completedSetupSteps / setup.length) * 100} aria-label={`${completedSetupSteps} av ${setup.length} steg klara`} />
              <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
                {setup.map((step) => <Chip key={step.label} size="small" color={step.done ? "success" : "default"} label={`${step.done ? "✓" : "○"} ${step.label}`} />)}
              </Stack>
              <Box><Button variant="contained" component={Link} to={`/families/${familyId}/setup`}>Fortsätt steg för steg</Button></Box>
            </Stack>
          </CardContent>
        </Card>
      )}
      <Box>
        <Typography variant="h5" gutterBottom>Boendekalendrar</Typography>
        <Stack direction={{ xs: "column", sm: "row" }} spacing={2} flexWrap="wrap">
          {family.calendars.filter((calendar) => calendar.isActive).map((calendar) => (
            <Card key={calendar.id} sx={{ minWidth: 260 }}>
              <CardActionArea component={Link} to={`/families/${familyId}/calendars/${calendar.id}`}>
                <CardContent>
                  <Typography variant="h6">{calendar.name}</Typography>
                  <Typography color="text.secondary">{calendar.childCount} barn</Typography>
                </CardContent>
              </CardActionArea>
            </Card>
          ))}
        </Stack>
      </Box>
    </Stack>
  );
}
