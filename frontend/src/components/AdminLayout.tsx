import { AppBar, Box, Button, Container, Toolbar, Typography } from "@mui/material";
import { Outlet } from "react-router-dom";
import { useAdminAuth } from "../auth/AdminAuthContext";

export default function AdminLayout() {
  const { admin, logout } = useAdminAuth();
  return <Box sx={{ minHeight: "100vh", bgcolor: "grey.100" }}>
    <AppBar position="static" color="default" elevation={1}><Toolbar><Typography variant="h6" fontWeight={700}>Selma plattformsadministration</Typography><Box sx={{ flex: 1 }} /><Typography variant="body2" sx={{ mr: 2 }}>{admin?.displayName} · {admin?.authenticationMethod}</Typography><Button onClick={() => void logout()}>Logga ut</Button></Toolbar></AppBar>
    <Container maxWidth="xl" sx={{ py: 4 }}><Outlet /></Container>
  </Box>;
}
