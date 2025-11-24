import { Box, Button, Container, Typography, Paper, Stack } from '@mui/material';
import GoogleIcon from '@mui/icons-material/Google';
import PreviewIcon from '@mui/icons-material/Preview';

interface LoginScreenProps {
  onLogin: (isDemo: boolean) => void;
}

export default function LoginScreen({ onLogin }: LoginScreenProps) {
  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        bgcolor: 'background.default',
      }}
    >
      <Container maxWidth="sm">
        <Paper elevation={3} sx={{ p: 4 }}>
          <Stack spacing={3} alignItems="center">
            <Typography variant="h3" component="h1" textAlign="center">
              Co-Parenting Calendar
            </Typography>
            <Typography variant="body1" textAlign="center" color="text.secondary">
              Manage your co-parenting schedule with ease
            </Typography>
            <Box sx={{ width: '100%', mt: 2 }}>
              <Stack spacing={2}>
                <Button
                  variant="contained"
                  size="large"
                  fullWidth
                  startIcon={<GoogleIcon />}
                  onClick={() => onLogin(false)}
                  sx={{ py: 1.5 }}
                >
                  Sign in with Google
                </Button>
                <Button
                  variant="outlined"
                  size="large"
                  fullWidth
                  startIcon={<PreviewIcon />}
                  onClick={() => onLogin(true)}
                  sx={{ py: 1.5 }}
                >
                  Try Demo Mode
                </Button>
              </Stack>
            </Box>
            <Typography variant="caption" textAlign="center" color="text.secondary" sx={{ mt: 2 }}>
              Demo mode allows you to explore the app without signing in.
              Your demo data will be temporary.
            </Typography>
          </Stack>
        </Paper>
      </Container>
    </Box>
  );
}
