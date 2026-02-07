import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  Box,
  CircularProgress,
  Typography,
  Container,
} from '@mui/material';
import { useAuth } from '../auth/AuthContext';
import { sv } from '../i18n/sv';

export default function MagicLinkHandler() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { exchangeMagicToken } = useAuth();
  const [error, setError] = useState(false);
  const [hasProcessed, setHasProcessed] = useState(false);

  useEffect(() => {
    // Prevent multiple executions
    if (hasProcessed) {
      return;
    }

    const processMagicLink = async () => {
      setHasProcessed(true);
      
      const token = searchParams.get('token');
      
      if (!token) {
        setError(true);
        return;
      }

      try {
        const success = await exchangeMagicToken(token);
        
        if (success) {
          navigate('/', { replace: true });
        } else {
          setError(true);
        }
      } catch (err) {
        setError(true);
      }
    };

    processMagicLink();
  }, [searchParams, exchangeMagicToken, navigate, hasProcessed]);

  if (error) {
    return (
      <Container maxWidth="sm">
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            minHeight: '100vh',
            gap: 2,
            textAlign: 'center',
          }}
        >
          <Typography variant="h5" color="error">
            {sv.auth.invalidMagicLink}
          </Typography>
          <Typography variant="body1" color="text.secondary">
            {sv.auth.requestNewLink}
          </Typography>
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: '100vh',
          gap: 2,
        }}
      >
        <CircularProgress size={48} />
        <Typography variant="h6">{sv.auth.signingIn}</Typography>
      </Box>
    </Container>
  );
}
