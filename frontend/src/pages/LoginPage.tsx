import { Alert, Box, Button, Card, CardContent, Container, Typography } from "@mui/material";
import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export default function LoginPage() {
  const { isAuthenticated, isLoading, startOidcLogin } = useAuth();
  const location = useLocation();
  const query = new URLSearchParams(location.search);
  const from = (location.state as { from?: { pathname?: string } } | null)?.from?.pathname ?? "/";

  if (!isLoading && isAuthenticated) return <Navigate to={from} replace />;

  return (
    <Container maxWidth="sm" sx={{ minHeight: "100vh", display: "grid", placeItems: "center" }}>
      <Card sx={{ width: "100%" }}>
        <CardContent sx={{ p: 5 }}>
          <Typography variant="h3" fontWeight={700} gutterBottom>Selma</Typography>
          <Typography color="text.secondary" sx={{ mb: 4 }}>
            Boendescheman som fungerar för hela familjen.
          </Typography>
          {query.get("error") && <Alert severity="error" sx={{ mb: 2 }}>Inloggningen kunde inte slutföras.</Alert>}
          <Button fullWidth size="large" variant="contained" onClick={() => startOidcLogin(from)}>
            Logga in med Widsell ID
          </Button>
          <Box sx={{ mt: 3 }}>
            <Typography variant="body2" color="text.secondary">
              Första gången skapas ett konto automatiskt. Därefter väljer du att skapa en familj eller gå med i en befintlig.
            </Typography>
          </Box>
        </CardContent>
      </Card>
    </Container>
  );
}
