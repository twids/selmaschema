import { useState, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Alert,
  Typography,
  Container,
} from '@mui/material';
import { useAuth } from '../auth/AuthContext';
import { sv } from '../i18n/sv';

export default function LoginPage() {
  const navigate = useNavigate();
  const { loginAdmin } = useAuth();
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    
    // Clear previous error
    setError(null);
    
    // Validate password is not empty
    if (!password.trim()) {
      return;
    }
    
    setLoading(true);
    
    try {
      const success = await loginAdmin(password);
      
      if (success) {
        navigate('/');
      } else {
        setError(sv.auth.invalidPassword);
      }
    } catch (err) {
      setError(sv.auth.invalidPassword);
    } finally {
      setLoading(false);
    }
  };

  const handlePasswordChange = (newPassword: string) => {
    setPassword(newPassword);
    // Clear error when user starts typing again
    if (error) {
      setError(null);
    }
  };

  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: '100vh',
        }}
      >
        <Card sx={{ width: '100%', maxWidth: 420 }}>
          <CardContent sx={{ p: 4 }}>
            <Typography variant="h4" component="h1" gutterBottom align="center">
              {sv.auth.adminLogin}
            </Typography>

            <Box component="form" onSubmit={handleSubmit} noValidate>
              <TextField
                id="password"
                label={sv.auth.password}
                type="password"
                fullWidth
                required
                value={password}
                onChange={(e) => handlePasswordChange(e.target.value)}
                margin="normal"
                autoFocus
                autoComplete="current-password"
                disabled={loading}
              />

              {error && (
                <Alert severity="error" sx={{ mt: 2 }}>
                  {error}
                </Alert>
              )}

              <Button
                type="submit"
                variant="contained"
                fullWidth
                size="large"
                disabled={loading || !password.trim()}
                sx={{ mt: 3, mb: 2 }}
              >
                {loading ? sv.auth.signingIn : sv.auth.signIn}
              </Button>
            </Box>

            <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
              {sv.auth.parentsInfo}
            </Typography>
          </CardContent>
        </Card>
      </Box>
    </Container>
  );
}
