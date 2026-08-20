import { Alert, Button, Card, CardContent, Collapse, Container, Stack, TextField, Typography } from "@mui/material";
import { useState } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAdminAuth } from "../auth/AdminAuthContext";

export default function AdminLoginPage() {
  const { admin, isLoading, startLogin, breakGlassLogin } = useAdminAuth();
  const [showBreakGlass, setShowBreakGlass] = useState(false);
  const [password, setPassword] = useState("");
  const [error, setError] = useState(false);
  const location = useLocation();
  if (!isLoading && admin) return <Navigate to="/admin" replace />;
  const queryError = new URLSearchParams(location.search).get("error");
  return <Container maxWidth="sm" sx={{ minHeight: "100vh", display: "grid", placeItems: "center" }}><Card sx={{ width: "100%" }}><CardContent sx={{ p: 5 }}><Stack spacing={3}>
    <Typography variant="h4" fontWeight={700}>Plattformsadministration</Typography>
    <Typography color="text.secondary">Den här ytan är separerad från vanliga Selma och kräver medlemskap i Authentik-gruppen för plattformsadministratörer.</Typography>
    {(queryError || error) && <Alert severity="error">Administratörsinloggningen nekades.</Alert>}
    <Button size="large" variant="contained" onClick={startLogin}>Logga in med administratörs-ID</Button>
    <Button size="small" color="inherit" onClick={() => setShowBreakGlass((value) => !value)}>Reservåtkomst</Button>
    <Collapse in={showBreakGlass}><Stack spacing={2}><Alert severity="warning">Break-glass är endast för nödläge och loggas alltid.</Alert><TextField type="password" label="Reservlösenord" value={password} onChange={(e) => setPassword(e.target.value)} /><Button variant="outlined" onClick={() => void breakGlassLogin(password).then((ok) => setError(!ok))}>Logga in med reservåtkomst</Button></Stack></Collapse>
  </Stack></CardContent></Card></Container>;
}
