import React, { useState } from 'react';
import { Box, Button, Typography } from '@mui/material';
import { Add as AddIcon } from '@mui/icons-material';
import { sv } from '../i18n/sv';
import { ChangeRequestModal } from '../components/ChangeRequestModal';
import { ChangeRequestsPanel } from '../components/ChangeRequestsPanel';

export const ChangeRequestsPage: React.FC = () => {

  const [modalOpen, setModalOpen] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);

  const handleSuccess = () => {
    setRefreshKey((prev) => prev + 1);
  };

  return (
    <Box sx={{ p: 3 }}>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          mb: 3,
        }}
      >
        <Typography variant="h4" component="h1" data-testid="page-title">
          {sv.changeRequest.title}
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => setModalOpen(true)}
          data-testid="create-request-button"
        >
          {sv.changeRequest.createNew}
        </Button>
      </Box>

      <ChangeRequestsPanel key={refreshKey} onRequestsChanged={handleSuccess} />

      <ChangeRequestModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        onSuccess={handleSuccess}
      />
    </Box>
  );
};
