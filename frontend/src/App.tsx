import { Box } from "@mui/material";
import { Navigate, Route, Routes } from "react-router-dom";
import { AdminAuthProvider } from "./auth/AdminAuthContext";
import { useAuth } from "./auth/AuthContext";
import AdminLayout from "./components/AdminLayout";
import AdminProtectedRoute from "./components/AdminProtectedRoute";
import AppLayout from "./components/AppLayout";
import ProtectedRoute from "./components/ProtectedRoute";
import AdminLoginPage from "./pages/AdminLoginPage";
import FamilyCalendarPage from "./pages/FamilyCalendarPage";
import FamilyDashboardPage from "./pages/FamilyDashboardPage";
import FamilySettingsPage from "./pages/FamilySettingsPage";
import InvitationsPage from "./pages/InvitationsPage";
import JoinPage from "./pages/JoinPage";
import LoginPage from "./pages/LoginPage";
import OnboardingPage from "./pages/OnboardingPage";
import PlatformAdminPage from "./pages/PlatformAdminPage";
import { ChangeRequestsPage } from "./pages/ChangeRequestsPage";

function HomeRedirect() {
  const { memberships } = useAuth();
  const active = memberships.find((membership) => membership.status === "Active");
  return !active
    ? <Navigate to="/onboarding" replace />
    : <Navigate to={`/families/${active.familyId}`} replace />;
}

function AdminApp() {
  return <AdminAuthProvider><Routes>
    <Route path="login" element={<AdminLoginPage />} />
    <Route element={<AdminProtectedRoute />}>
      <Route element={<AdminLayout />}><Route index element={<PlatformAdminPage />} /></Route>
    </Route>
    <Route path="*" element={<Navigate to="/admin" replace />} />
  </Routes></AdminAuthProvider>;
}

export default function App() {
  return <Routes>
    <Route path="/login" element={<LoginPage />} />
    <Route path="/join" element={<Box sx={{ p: 4 }}><JoinPage /></Box>} />
    <Route path="/join/:token" element={<Box sx={{ p: 4 }}><JoinPage /></Box>} />
    <Route path="/admin/*" element={<AdminApp />} />
    <Route element={<ProtectedRoute />}>
      <Route path="/" element={<HomeRedirect />} />
      <Route path="/onboarding" element={<Box sx={{ p: 4 }}><OnboardingPage /></Box>} />
      <Route path="/families/:familyId" element={<AppLayout />}>
        <Route index element={<FamilyDashboardPage />} />
        <Route path="calendars/:calendarId" element={<FamilyCalendarPage />} />
        <Route path="invitations" element={<InvitationsPage />} />
        <Route path="change-requests" element={<ChangeRequestsPage />} />
        <Route path="settings" element={<FamilySettingsPage />} />
      </Route>
    </Route>
    <Route path="*" element={<Navigate to="/" replace />} />
  </Routes>;
}
