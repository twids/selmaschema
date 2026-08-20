import { type FormEvent, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Container,
  Divider,
  TextField,
  Typography,
} from "@mui/material";
import { useAuth } from "../auth/AuthContext";

const oidcErrors: Record<string, string> = {
  invitation_required: "Kontot saknar åtkomst till Selma. Be en användare om en inbjudan.",
  oidc_failed: "Inloggningen med Widsell ID kunde inte slutföras.",
};

export default function LoginPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { loginAdmin, startOidcLogin } = useAuth();
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const oidcError = searchParams.get("error");

  const submitAdmin = async (event: FormEvent) => {
    event.preventDefault();
    if (!password.trim()) return;
    setLoading(true);
    setError(null);
    if (await loginAdmin(password)) {
      navigate("/", { replace: true });
    } else {
      setError("Ogiltigt administratörslösenord eller reservinloggningen är avstängd.");
    }
    setLoading(false);
  };

  return (
    <Container maxWidth="sm">
      <Box sx={{ minHeight: "100vh", display: "grid", placeItems: "center" }}>
        <Card sx={{ width: "100%", maxWidth: 440 }}>
          <CardContent sx={{ p: 4 }}>
            <Typography variant="h4" component="h1" align="center" gutterBottom>Logga in i Selma</Typography>
            {oidcError && <Alert severity="error" sx={{ mb: 2 }}>{oidcErrors[oidcError] || oidcErrors.oidc_failed}</Alert>}
            <Button
              variant="contained"
              size="large"
              fullWidth
              onClick={() => startOidcLogin("/")}
              sx={{ mt: 2, mb: 3 }}
            >
              Logga in med Widsell ID
            </Button>

            <Divider sx={{ mb: 2 }}>Reservväg</Divider>
            <Typography variant="subtitle2" color="text.secondary">Lokal reservadmin</Typography>
            <Box component="form" onSubmit={submitAdmin}>
              <TextField
                label="Administratörslösenord"
                type="password"
                fullWidth
                size="small"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                autoComplete="current-password"
                disabled={loading}
                sx={{ mt: 1 }}
              />
              {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
              <Button
                type="submit"
                variant="outlined"
                size="small"
                fullWidth
                disabled={loading || !password.trim()}
                sx={{ mt: 2 }}
              >
                {loading ? "Loggar in…" : "Logga in som reservadmin"}
              </Button>
            </Box>
          </CardContent>
        </Card>
      </Box>
    </Container>
  );
}
