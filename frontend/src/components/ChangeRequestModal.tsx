import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Box,
  Typography,
  Chip,
} from '@mui/material';
import { createChangeRequests } from '../api/changeRequests';
import { sv } from '../i18n/sv';

interface ChangeRequestModalProps {
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const ChangeRequestModal: React.FC<ChangeRequestModalProps> = ({
  open,
  onClose,
  onSuccess,
}) => {
  const [selectedDates, setSelectedDates] = useState<string[]>([]);
  const [currentDate, setCurrentDate] = useState<string>('');
  const [requestedParent, setRequestedParent] = useState<string>('A');
  const [comment, setComment] = useState<string>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleAddDate = () => {
    if (currentDate && !selectedDates.includes(currentDate)) {
      setSelectedDates([...selectedDates, currentDate]);
      setCurrentDate('');
    }
  };

  const handleRemoveDate = (date: string) => {
    setSelectedDates(selectedDates.filter((d) => d !== date));
  };

  const handleSubmit = async () => {
    if (selectedDates.length === 0) {
      setError(sv.changeRequest.errors.noDates);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      await createChangeRequests(selectedDates, requestedParent, comment || undefined);
      onSuccess();
      handleClose();
    } catch (err) {
      setError(
        err instanceof Error ? err.message : sv.changeRequest.errors.createFailed
      );
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setSelectedDates([]);
    setCurrentDate('');
    setRequestedParent('A');
    setComment('');
    setError(null);
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>{sv.changeRequest.create.title}</DialogTitle>
      <DialogContent>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
          {/* Date Picker */}
          <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
            <TextField
              type="date"
              label={sv.changeRequest.create.selectDate}
              value={currentDate}
              onChange={(e) => setCurrentDate(e.target.value)}
              fullWidth
              InputLabelProps={{ shrink: true }}
              data-testid="date-picker"
            />
            <Button
              onClick={handleAddDate}
              disabled={!currentDate}
              variant="outlined"
              data-testid="add-date-button"
            >
              {sv.changeRequest.create.addDate}
            </Button>
          </Box>

          {/* Selected Dates */}
          {selectedDates.length > 0 && (
            <Box>
              <Typography variant="body2" gutterBottom>
                {sv.changeRequest.create.selectedDates}:
              </Typography>
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                {selectedDates.map((date, index) => (
                  <Chip
                    key={index}
                    label={date}
                    onDelete={() => handleRemoveDate(date)}
                    data-testid={`date-chip-${index}`}
                  />
                ))}
              </Box>
            </Box>
          )}

          {/* Parent Selector */}
          <FormControl fullWidth>
            <InputLabel>{sv.changeRequest.create.requestedParent}</InputLabel>
            <Select
              value={requestedParent}
              onChange={(e) => setRequestedParent(e.target.value)}
              label={sv.changeRequest.create.requestedParent}
              data-testid="parent-selector"
            >
              <MenuItem value="A" data-testid="parent-a-option">
                {sv.calendar.parentA}
              </MenuItem>
              <MenuItem value="B" data-testid="parent-b-option">
                {sv.calendar.parentB}
              </MenuItem>
            </Select>
          </FormControl>

          {/* Comment */}
          <TextField
            label={sv.changeRequest.create.comment}
            multiline
            rows={3}
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            fullWidth
            data-testid="comment-input"
            inputProps={{ maxLength: 1000 }}
            helperText={`${comment.length}/1000`}
          />

          {/* Error Message */}
          {error && (
            <Typography color="error" variant="body2" data-testid="error-message">
              {error}
            </Typography>
          )}
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={loading} data-testid="cancel-button">
          {sv.common.cancel}
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={loading || selectedDates.length === 0}
          data-testid="submit-button"
        >
          {loading ? sv.common.loading : sv.changeRequest.create.submit}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
