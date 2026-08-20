import { Container, Paper, Typography } from "@mui/material";
import UsersTable from "./UsersTable";

export default function AdminDashboard() {
  return (
    <Container maxWidth="lg" data-testid="admin-dashboard">
      <Typography variant="h4" sx={{ my: 3 }}>Administration</Typography>
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>Användare</Typography>
        <UsersTable refreshKey={0} />
      </Paper>
    </Container>
  );
}
