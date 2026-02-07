import { useCallback, useState } from "react";
import { Container, Paper, Typography } from "@mui/material";
import MagicLinkForm from "./MagicLinkForm";
import MagicLinksTable from "./MagicLinksTable";
import UsersTable from "./UsersTable";

/** Admin dashboard with magic link generation and user management. */
export default function AdminDashboard() {
  const [refreshKey, setRefreshKey] = useState(0);

  const handleLinkCreated = useCallback(() => {
    setRefreshKey((k) => k + 1);
  }, []);

  return (
    <Container maxWidth="lg" data-testid="admin-dashboard">
      <Typography variant="h4" sx={{ my: 3 }}>
        Administration
      </Typography>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>
          Skapa magisk länk
        </Typography>
        <MagicLinkForm onCreated={handleLinkCreated} />
      </Paper>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>
          Aktiva magiska länkar
        </Typography>
        <MagicLinksTable refreshKey={refreshKey} />
      </Paper>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>
          Användare
        </Typography>
        <UsersTable refreshKey={refreshKey} />
      </Paper>
    </Container>
  );
}
