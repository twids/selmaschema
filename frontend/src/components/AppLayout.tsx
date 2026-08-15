import { AppBar, Toolbar, Typography, Button, Container, Box } from '@mui/material';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

interface AppLayoutProps {
  children: React.ReactNode;
}

export default function AppLayout({ children }: AppLayoutProps) {
  const { isAuthenticated, user, logout } = useAuth();
  const location = useLocation();

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            Co-Parenting Kalender
          </Typography>
          {isAuthenticated && user && (
            <>
              <Button
                component={Link}
                to="/"
                color="inherit"
                sx={{
                  mr: 1,
                  borderBottom: location.pathname === '/' ? 2 : 0,
                  borderRadius: 0,
                }}
                data-testid="nav-calendar"
              >
                Kalender
              </Button>
              <Button
                component={Link}
                to="/change-requests"
                color="inherit"
                sx={{
                  mr: 2,
                  borderBottom: location.pathname === '/change-requests' ? 2 : 0,
                  borderRadius: 0,
                }}
                data-testid="nav-change-requests"
              >
                Byten
              </Button>
              <Button
                component={Link}
                to="/invitations"
                color="inherit"
                sx={{
                  mr: 2,
                  borderBottom: location.pathname === '/invitations' ? 2 : 0,
                  borderRadius: 0,
                }}
                data-testid="nav-invitations"
              >
                Inbjudningar
              </Button>
              {user.role === 'Admin' && (
                <Button
                  component={Link}
                  to="/admin"
                  color="inherit"
                  sx={{
                    mr: 2,
                    borderBottom: location.pathname === '/admin' ? 2 : 0,
                    borderRadius: 0,
                  }}
                  data-testid="nav-admin"
                >
                  Admin
                </Button>
              )}
              <Typography variant="body1" sx={{ mr: 2 }}>
                {user.displayName || user.email}
              </Typography>
              <Button color="inherit" onClick={() => void logout()}>
                Logga ut
              </Button>
            </>
          )}
        </Toolbar>
      </AppBar>
      <Container component="main" sx={{ mt: 4, mb: 4, flexGrow: 1 }}>
        {children}
      </Container>
    </Box>
  );
}
