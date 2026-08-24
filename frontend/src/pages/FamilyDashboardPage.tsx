import { Alert, Box, Button, Card, CardActionArea, CardContent, Chip, CircularProgress, Stack, Typography } from "@mui/material";
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
    { done: family.mySide !== null, label: "Välj vilken sida du tillhör" },
    { done: family.activeChildren > 0, label: "Lägg till barn" },
    { done: family.hasActiveSchedule, label: "Förhandsvisa och aktivera schema" },
  ];

  return (
    <Stack spacing={4}>
      <Box>
        <Typography variant="h3" fontWeight={700}>{family.name}</Typography>
        <Typography color="text.secondary">{family.sideALabel} · {family.sideBLabel} · {family.timeZoneId}</Typography>
      </Box>
      {family.myPermission === "Owner" && setup.some((step) => !step.done) && (
        <Alert severity="info" action={<Button component={Link} to={`/families/${familyId}/settings`}>Fortsätt konfigurera</Button>}>
          <Typography fontWeight={600}>Kom igång</Typography>
          {setup.map((step) => <Chip key={step.label} size="small" color={step.done ? "success" : "default"} label={`${step.done ? "✓" : "○"} ${step.label}`} sx={{ mr: 1, mt: 1 }} />)}
        </Alert>
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
