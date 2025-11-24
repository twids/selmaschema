import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  List,
  ListItem,
  ListItemText,
  Typography,
  CircularProgress,
  Box,
  Chip,
  Stack,
} from '@mui/material';
import { Invitation } from '../types';

const API_BASE_URL = import.meta.env.VITE_API_URL || '';

interface InvitationsDialogProps {
  open: boolean;
  onClose: () => void;
}

export default function InvitationsDialog({ open, onClose }: InvitationsDialogProps) {
  const [invitations, setInvitations] = useState<Invitation[]>([]);
  const [loading, setLoading] = useState(false);

  const loadInvitations = async () => {
    setLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/api/invitations/pending`, {
        credentials: 'include',
      });
      if (response.ok) {
        const data = await response.json();
        setInvitations(data);
      }
    } catch (error) {
      console.error('Error loading invitations:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (open) {
      loadInvitations();
    }
  }, [open]);

  const handleAccept = async (invitationId: number) => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/invitations/${invitationId}/accept`, {
        method: 'POST',
        credentials: 'include',
      });
      if (response.ok) {
        await loadInvitations();
      }
    } catch (error) {
      console.error('Error accepting invitation:', error);
    }
  };

  const handleDecline = async (invitationId: number) => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/invitations/${invitationId}/decline`, {
        method: 'POST',
        credentials: 'include',
      });
      if (response.ok) {
        await loadInvitations();
      }
    } catch (error) {
      console.error('Error declining invitation:', error);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Pending Invitations</DialogTitle>
      <DialogContent>
        {loading ? (
          <Box display="flex" justifyContent="center" p={3}>
            <CircularProgress />
          </Box>
        ) : invitations.length === 0 ? (
          <Typography color="text.secondary" textAlign="center" py={3}>
            No pending invitations
          </Typography>
        ) : (
          <List>
            {invitations.map((invitation) => (
              <ListItem
                key={invitation.id}
                sx={{
                  border: 1,
                  borderColor: 'divider',
                  borderRadius: 1,
                  mb: 1,
                }}
              >
                <ListItemText
                  primary={
                    <Stack direction="row" spacing={1} alignItems="center">
                      <Typography variant="subtitle1">{invitation.childName}</Typography>
                      <Chip label="New" size="small" color="primary" />
                    </Stack>
                  }
                  secondary={
                    <>
                      <Typography component="span" variant="body2">
                        Invited by {invitation.inviterName}
                      </Typography>
                      <br />
                      <Typography component="span" variant="caption" color="text.secondary">
                        {new Date(invitation.createdAt).toLocaleDateString()}
                      </Typography>
                    </>
                  }
                />
                <Stack direction="row" spacing={1}>
                  <Button
                    variant="contained"
                    size="small"
                    onClick={() => handleAccept(invitation.id)}
                  >
                    Accept
                  </Button>
                  <Button
                    variant="outlined"
                    size="small"
                    color="error"
                    onClick={() => handleDecline(invitation.id)}
                  >
                    Decline
                  </Button>
                </Stack>
              </ListItem>
            ))}
          </List>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
}
