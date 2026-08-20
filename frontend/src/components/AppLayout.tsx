import { AppBar, Box, Button, Container, FormControl, MenuItem, Select, Toolbar, Typography } from "@mui/material";
import { Link, Outlet, useLocation, useNavigate, useParams } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export default function AppLayout({ children }: { children?: React.ReactNode }) {
  const { account, memberships, logout } = useAuth();
  const { familyId } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const current = memberships.find((membership) => membership.familyId === familyId);

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppBar position="static" elevation={0}>
        <Toolbar sx={{ gap: 2 }}>
          <Typography variant="h6" sx={{ fontWeight: 700 }}>Selma</Typography>
          {memberships.length > 0 && (
            <FormControl size="small" sx={{ minWidth: 190, bgcolor: "background.paper", borderRadius: 1 }}>
              <Select
                value={familyId ?? ""}
                displayEmpty
                aria-label="Välj familj"
                onChange={(event) => navigate(`/families/${event.target.value}`)}
              >
                {memberships.map((membership) => (
                  <MenuItem key={membership.familyId} value={membership.familyId}>{membership.familyName}</MenuItem>
                ))}
              </Select>
            </FormControl>
          )}
          <Box sx={{ flex: 1 }} />
          {familyId && (
            <>
              <Button color="inherit" component={Link} to={`/families/${familyId}`}>Översikt</Button>
              <Button color="inherit" component={Link} to={`/families/${familyId}/invitations`}>Bjud in</Button>
              <Button color="inherit" component={Link} to={`/families/${familyId}/change-requests`}>Byten</Button>
              {current?.permission === "Owner" && (
                <Button color="inherit" component={Link} to={`/families/${familyId}/settings`}>Inställningar</Button>
              )}
            </>
          )}
          <Typography variant="body2">{account?.displayName || account?.email}</Typography>
          <Button color="inherit" onClick={() => void logout()}>Logga ut</Button>
        </Toolbar>
      </AppBar>
      <Container component="main" maxWidth="lg" sx={{ py: 4 }} data-path={location.pathname}>
        {children ?? <Outlet />}
      </Container>
    </Box>
  );
}
