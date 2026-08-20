import { Routes, Route, Navigate } from 'react-router-dom';
import AppLayout from './components/AppLayout';
import ProtectedRoute from './components/ProtectedRoute';
import LoginPage from './pages/LoginPage';
import InvitationConfirmPage from './pages/InvitationConfirmPage';
import InvitationsPage from './pages/InvitationsPage';
import CalendarPage from './components/CalendarPage';
import AdminDashboard from './components/AdminDashboard';
import { ChangeRequestsPage } from './pages/ChangeRequestsPage';

export default function App() {
  return (
    <Routes>
      {/* Public routes */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/auth/invitation" element={<InvitationConfirmPage />} />

      {/* Protected routes with AppLayout */}
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <AppLayout>
              <CalendarPage />
            </AppLayout>
          </ProtectedRoute>
        }
      />

      {/* Change Requests route */}
      <Route
        path="/change-requests"
        element={
          <ProtectedRoute>
            <AppLayout>
              <ChangeRequestsPage />
            </AppLayout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/invitations"
        element={
          <ProtectedRoute>
            <AppLayout><InvitationsPage /></AppLayout>
          </ProtectedRoute>
        }
      />

      {/* Admin-only route */}
      <Route
        path="/admin"
        element={
          <ProtectedRoute requireAdmin>
            <AppLayout>
              <AdminDashboard />
            </AppLayout>
          </ProtectedRoute>
        }
      />

      {/* Catch-all redirect */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

