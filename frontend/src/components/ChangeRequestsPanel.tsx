import React, { useCallback, useState, useEffect } from 'react';
import {
  Box,
  Paper,
  Tabs,
  Tab,
  List,
  ListItem,
  ListItemText,
  Chip,
  Button,
  Typography,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  CircularProgress,
} from '@mui/material';
import { sv } from '../i18n/sv';
import { useAuth } from '../auth/AuthContext';
import {
  type ChangeRequestDto,
  getMyChangeRequests,
  getPendingChangeRequests,
  reviewChangeRequest,
  cancelChangeRequest,
} from '../api/changeRequests';

interface ChangeRequestsPanelProps {
  onRequestsChanged?: () => void;
}

export const ChangeRequestsPanel: React.FC<ChangeRequestsPanelProps> = ({
  onRequestsChanged,
}) => {
  const { user } = useAuth();
  const [tab, setTab] = useState<'pending' | 'all'>('pending');
  const [requests, setRequests] = useState<ChangeRequestDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [reviewDialog, setReviewDialog] = useState<{
    open: boolean;
    request: ChangeRequestDto | null;
  }>({ open: false, request: null });
  const [reviewComment, setReviewComment] = useState('');
  const [reviewLoading, setReviewLoading] = useState(false);

  const loadRequests = useCallback(async () => {
    setLoading(true);
    try {
      const data =
        tab === 'pending'
          ? await getPendingChangeRequests()
          : await getMyChangeRequests();
      setRequests(data);
    } catch (error) {
      console.error('Failed to load change requests:', error);
    } finally {
      setLoading(false);
    }
  }, [tab]);

  useEffect(() => {
    loadRequests();
  }, [loadRequests]);

  const getStatusColor = (
    status: string
  ): 'default' | 'primary' | 'success' | 'error' | 'warning' => {
    switch (status) {
      case 'Pending':
        return 'warning';
      case 'Approved':
        return 'success';
      case 'Rejected':
        return 'error';
      case 'Cancelled':
        return 'default';
      default:
        return 'default';
    }
  };

  const canReview = (request: ChangeRequestDto): boolean => {
    if (!user || request.status !== 'Pending') return false;
    // Can review if the request affects your days (CurrentParent matches your role)
    const userParent = user.role === 'ParentA' ? 'A' : 'B';
    return request.currentParent === userParent;
  };

  const canCancel = (request: ChangeRequestDto): boolean => {
    if (!user || request.status !== 'Pending') return false;
    // Can cancel own requests
    return request.requestedByName === user.displayName;
  };

  const handleReview = (request: ChangeRequestDto) => {
    setReviewDialog({ open: true, request });
    setReviewComment('');
  };

  const handleReviewSubmit = async (approved: boolean) => {
    if (!reviewDialog.request) return;

    setReviewLoading(true);
    try {
      await reviewChangeRequest(
        reviewDialog.request.id,
        approved,
        reviewComment || undefined
      );
      setReviewDialog({ open: false, request: null });
      setReviewComment('');
      await loadRequests();
      onRequestsChanged?.();
    } catch (error) {
      console.error('Failed to review request:', error);
    } finally {
      setReviewLoading(false);
    }
  };

  const handleCancel = async (request: ChangeRequestDto) => {
    if (!confirm(sv.changeRequest.confirmCancel)) return;

    try {
      await cancelChangeRequest(request.id);
      await loadRequests();
      onRequestsChanged?.();
    } catch (error) {
      console.error('Failed to cancel request:', error);
    }
  };

  return (
    <Paper sx={{ p: 2 }}>
      <Tabs
        value={tab}
        onChange={(_, newValue) => setTab(newValue)}
        sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}
      >
        <Tab
          label={sv.changeRequest.tabs.pending}
          value="pending"
          data-testid="pending-tab"
        />
        <Tab label={sv.changeRequest.tabs.all} value="all" data-testid="all-tab" />
      </Tabs>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
          <CircularProgress />
        </Box>
      ) : requests.length === 0 ? (
        <Typography
          color="text.secondary"
          sx={{ p: 3, textAlign: 'center' }}
          data-testid="no-requests"
        >
          {sv.changeRequest.noRequests}
        </Typography>
      ) : (
        <List data-testid="requests-list">
          {requests.map((request) => (
            <ListItem
              key={request.id}
              sx={{
                flexDirection: 'column',
                alignItems: 'stretch',
                border: 1,
                borderColor: 'divider',
                borderRadius: 1,
                mb: 1,
              }}
              data-testid={`request-${request.id}`}
            >
              <Box
                sx={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  width: '100%',
                }}
              >
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                      <Typography variant="body1">
                        {request.requestedForDate}
                      </Typography>
                      <Chip
                        label={request.status}
                        color={getStatusColor(request.status)}
                        size="small"
                        data-testid={`status-${request.id}`}
                      />
                    </Box>
                  }
                  secondary={
                    <>
                      <Typography variant="body2" component="span">
                        {sv.changeRequest.requestedBy}: {request.requestedByName} •{' '}
                        {request.currentParent} → {request.requestedParent}
                      </Typography>
                      {request.comment && (
                        <Typography
                          variant="body2"
                          color="text.secondary"
                          sx={{ mt: 0.5 }}
                        >
                          {sv.changeRequest.comment}: {request.comment}
                        </Typography>
                      )}
                      {request.reviewedByName && (
                        <Typography
                          variant="body2"
                          color="text.secondary"
                          sx={{ mt: 0.5 }}
                        >
                          {sv.changeRequest.reviewedBy}: {request.reviewedByName}
                        </Typography>
                      )}
                    </>
                  }
                />
                <Box sx={{ display: 'flex', gap: 1 }}>
                  {canReview(request) && (
                    <Button
                      size="small"
                      variant="contained"
                      onClick={() => handleReview(request)}
                      data-testid={`review-button-${request.id}`}
                    >
                      {sv.changeRequest.review}
                    </Button>
                  )}
                  {canCancel(request) && (
                    <Button
                      size="small"
                      variant="outlined"
                      color="error"
                      onClick={() => handleCancel(request)}
                      data-testid={`cancel-button-${request.id}`}
                    >
                      {sv.common.cancel}
                    </Button>
                  )}
                </Box>
              </Box>
            </ListItem>
          ))}
        </List>
      )}

      {/* Review Dialog */}
      <Dialog
        open={reviewDialog.open}
        onClose={() => setReviewDialog({ open: false, request: null })}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>{sv.changeRequest.reviewDialog.title}</DialogTitle>
        <DialogContent>
          {reviewDialog.request && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
              <Typography>
                {sv.changeRequest.reviewDialog.date}:{' '}
                {reviewDialog.request.requestedForDate}
              </Typography>
              <Typography>
                {sv.changeRequest.reviewDialog.requestedBy}:{' '}
                {reviewDialog.request.requestedByName}
              </Typography>
              <Typography>
                {reviewDialog.request.currentParent} → {reviewDialog.request.requestedParent}
              </Typography>
              {reviewDialog.request.comment && (
                <Typography>
                  {sv.changeRequest.comment}: {reviewDialog.request.comment}
                </Typography>
              )}
              <TextField
                label={sv.changeRequest.reviewDialog.responseComment}
                multiline
                rows={3}
                value={reviewComment}
                onChange={(e) => setReviewComment(e.target.value)}
                fullWidth
                data-testid="review-comment-input"
                inputProps={{ maxLength: 1000 }}
              />
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button
            onClick={() => handleReviewSubmit(false)}
            color="error"
            disabled={reviewLoading}
            data-testid="reject-button"
          >
            {sv.changeRequest.reviewDialog.reject}
          </Button>
          <Button
            onClick={() => handleReviewSubmit(true)}
            color="success"
            variant="contained"
            disabled={reviewLoading}
            data-testid="approve-button"
          >
            {sv.changeRequest.reviewDialog.approve}
          </Button>
        </DialogActions>
      </Dialog>
    </Paper>
  );
};
