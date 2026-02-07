import { useState, useEffect } from 'react';
import {
  Alert,
  Box,
  Button,
  Checkbox,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Select,
  type SelectChangeEvent,
} from '@mui/material';
import { useCalendar } from '../context/CalendarContext';
import { useConfig } from '../context/ConfigContext';
import { sv, formatSwedishDate } from '../i18n/sv';
import type { CommentDto, UpdateDayAssignmentDto } from '../api/types';
import CommentSection from './CommentSection';

interface DayModalProps {
  open: boolean;
  dateKey: string | null;
  onClose: () => void;
}

/** Parse a YYYY-MM-DD key into { year, month, day }. */
function parseDateKey(key: string) {
  const [year, month, day] = key.split('-').map(Number);
  return { year, month, day };
}

export default function DayModal({ open, dateKey, onClose }: DayModalProps) {
  const { calendarData, updateDay } = useCalendar();
  const { parentNames } = useConfig();

  const existing = dateKey ? calendarData[dateKey] : undefined;

  const [parent, setParent] = useState<string | null>(null);
  const [isVAB, setIsVAB] = useState(false);
  const [specialStatus, setSpecialStatus] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [localParentAComments, setLocalParentAComments] = useState<CommentDto[]>([]);
  const [localParentBComments, setLocalParentBComments] = useState<CommentDto[]>([]);

  // Sync local state when the modal opens or dateKey changes
  useEffect(() => {
    if (open) {
      setParent(existing?.parent ?? null);
      setIsVAB(existing?.isVAB ?? false);
      setSpecialStatus(existing?.specialStatus ?? null);
      setError(null);
      setSaving(false);
      setLocalParentAComments(existing?.parentAComments ?? []);
      setLocalParentBComments(existing?.parentBComments ?? []);
    }
  }, [open, dateKey, existing]);

  const handleParentChange = (e: SelectChangeEvent<string>) => {
    const val = e.target.value;
    setParent(val === '' ? null : val);
  };

  const handleSpecialStatusChange = (e: SelectChangeEvent<string>) => {
    const val = e.target.value;
    setSpecialStatus(val === '' ? null : val);
  };

  const handleSave = async () => {
    if (!dateKey) return;
    setSaving(true);
    setError(null);

    const { year, month, day } = parseDateKey(dateKey);
    const data: UpdateDayAssignmentDto = { parent, isVAB, specialStatus };

    try {
      await updateDay(year, month, day, data);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    } finally {
      setSaving(false);
    }
  };

  const formattedDate = dateKey
    ? formatSwedishDate(new Date(dateKey + 'T00:00:00'))
    : '';

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="md"
      fullWidth
      data-testid="day-modal"
    >
      <DialogTitle>{formattedDate}</DialogTitle>

      <DialogContent>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
          {/* Parent assignment */}
          <FormControl fullWidth size="small">
            <InputLabel id="parent-select-label">
              {sv.dayModal.assignedTo}
            </InputLabel>
            <Select
              labelId="parent-select-label"
              label={sv.dayModal.assignedTo}
              value={parent ?? ''}
              onChange={handleParentChange}
            >
              <MenuItem value="">{sv.dayModal.unassigned}</MenuItem>
              <MenuItem value="A">{parentNames.parentAName}</MenuItem>
              <MenuItem value="B">{parentNames.parentBName}</MenuItem>
            </Select>
          </FormControl>

          {/* VAB checkbox */}
          <FormControlLabel
            control={
              <Checkbox
                checked={isVAB}
                onChange={(_, checked) => setIsVAB(checked)}
              />
            }
            label={sv.dayModal.markAsVAB}
          />

          {/* Special status */}
          <FormControl fullWidth size="small">
            <InputLabel id="status-select-label">
              {sv.dayModal.specialStatus}
            </InputLabel>
            <Select
              labelId="status-select-label"
              label={sv.dayModal.specialStatus}
              value={specialStatus ?? ''}
              onChange={handleSpecialStatusChange}
            >
              <MenuItem value="">{sv.specialStatus.normal}</MenuItem>
              <MenuItem value="PreschoolClosed">
                {sv.specialStatus.preschoolClosed}
              </MenuItem>
              <MenuItem value="Holiday">{sv.specialStatus.holiday}</MenuItem>
            </Select>
          </FormControl>

          {/* Error */}
          {error && <Alert severity="error">{error}</Alert>}

          {/* Comment sections */}
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' },
              gap: 2,
              mt: 2,
            }}
          >
            <CommentSection
              parent="A"
              parentName={parentNames.parentAName}
              comments={localParentAComments}
              dayAssignmentId={existing?.id}
              onCommentAdded={(c) =>
                setLocalParentAComments((prev) => [...prev, c])
              }
            />
            <CommentSection
              parent="B"
              parentName={parentNames.parentBName}
              comments={localParentBComments}
              dayAssignmentId={existing?.id}
              onCommentAdded={(c) =>
                setLocalParentBComments((prev) => [...prev, c])
              }
            />
          </Box>
        </Box>
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} disabled={saving}>
          {sv.actions.cancel}
        </Button>
        <Button
          variant="contained"
          onClick={handleSave}
          disabled={saving}
          startIcon={saving ? <CircularProgress size={18} /> : undefined}
        >
          {sv.actions.save}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
