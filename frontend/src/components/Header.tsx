import { AppBar, Toolbar, Typography, Button, IconButton, Badge, Box } from '@mui/material';
import MailIcon from '@mui/icons-material/Mail';
import LogoutIcon from '@mui/icons-material/Logout';
import { UserInfo } from '../types';

interface HeaderProps {
  user: UserInfo;
  onLogout: () => void;
  onShowInvitations: () => void;
}

export default function Header({ user, onLogout, onShowInvitations }: HeaderProps) {
  return (
    <AppBar position="static">
      <Toolbar>
        <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
          Co-Parenting Calendar
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Typography variant="body2">
            {user.name} {user.isDemo && '(Demo)'}
          </Typography>
          <IconButton
            color="inherit"
            onClick={onShowInvitations}
            aria-label="invitations"
          >
            <Badge color="error" variant="dot">
              <MailIcon />
            </Badge>
          </IconButton>
          <Button
            color="inherit"
            startIcon={<LogoutIcon />}
            onClick={onLogout}
          >
            Logout
          </Button>
        </Box>
      </Toolbar>
    </AppBar>
  );
}

