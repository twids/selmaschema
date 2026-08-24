import { Box, CircularProgress } from "@mui/material";
import { Navigate, Outlet } from "react-router-dom";
import { useAdminAuth } from "../auth/AdminAuthContext";

export default function AdminProtectedRoute() {
  const { admin, isLoading } = useAdminAuth();
  if (isLoading) return <Box sx={{ minHeight: "100vh", display: "grid", placeItems: "center" }}><CircularProgress /></Box>;
  return admin ? <Outlet /> : <Navigate to="/admin/login" replace />;
}
