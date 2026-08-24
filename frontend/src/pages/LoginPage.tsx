import { Alert, Box, Button, CircularProgress, Container, Typography } from "@mui/material";
import { useEffect, useRef } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export default function LoginPage() {
  const { isAuthenticated, isLoading, startOidcLogin } = useAuth();
  const location = useLocation();
  const query = new URLSearchParams(location.search);
  const from = (location.state as { from?: { pathname?: string } } | null)?.from?.pathname ?? "/";
  const hasError = query.has("error");
  const redirectStarted = useRef(false);

  useEffect(() => {
    if (isLoading || isAuthenticated || hasError || redirectStarted.current) return;
    redirectStarted.current = true;
    startOidcLogin(from);
  }, [from, hasError, isAuthenticated, isLoading, startOidcLogin]);

  if (!isLoading && isAuthenticated) return <Navigate to={from} replace />;

  return (
    <Container maxWidth="sm" sx={{ minHeight: "100vh", display: "grid", placeItems: "center" }}>
      {hasError ? (
        <Box sx={{ width: "100%" }}>
          <Alert severity="error" sx={{ mb: 2 }}>Inloggningen kunde inte slutföras.</Alert>
          <Button fullWidth size="large" variant="contained" onClick={() => startOidcLogin(from)}>
            Försök igen
          </Button>
        </Box>
      ) : (
        <Box sx={{ display: "grid", justifyItems: "center", gap: 2 }}>
          <CircularProgress />
          <Typography color="text.secondary">Skickar dig till Widsell ID…</Typography>
        </Box>
      )}
    </Container>
  );
}
