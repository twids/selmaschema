import { useState } from 'react';
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
  FormControlLabel,
  Checkbox,
  Stack,
} from '@mui/material';
import { DayData, ParentNames } from '../types';

interface DayModalProps {
  dateKey: string;
  currentYear: number;
  dayData?: DayData;
  parentNames: ParentNames;
  onSave: (dayData: DayData) => void;
  onClose: () => void;
}

function formatDate(dateKey: string): string {
  const date = new Date(dateKey + 'T00:00:00');
  return date.toLocaleDateString('en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

export default function DayModal({
  dateKey,
  dayData,
  parentNames,
  onSave,
  onClose,
}: DayModalProps) {
  const [parent, setParent] = useState<'' | 'parentA' | 'parentB'>(
    dayData?.parent || ''
  );
  const [isVAB, setIsVAB] = useState(dayData?.isVAB || false);
  const [comment, setComment] = useState(dayData?.comment || '');

  const handleSave = () => {
    onSave({
      parent,
      isVAB,
      comment,
    });
  };

  return (
    <Dialog open={true} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{formatDate(dateKey)}</DialogTitle>
      <DialogContent>
        <Stack spacing={3} sx={{ mt: 1 }}>
          <FormControl fullWidth>
            <InputLabel>Assigned to</InputLabel>
            <Select
              value={parent}
              label="Assigned to"
              onChange={(e) => setParent(e.target.value as '' | 'parentA' | 'parentB')}
            >
              <MenuItem value="">Unassigned</MenuItem>
              <MenuItem value="parentA">{parentNames.parentA}</MenuItem>
              <MenuItem value="parentB">{parentNames.parentB}</MenuItem>
            </Select>
          </FormControl>

          <FormControlLabel
            control={
              <Checkbox
                checked={isVAB}
                onChange={(e) => setIsVAB(e.target.checked)}
              />
            }
            label="Mark as VAB (Child Care Leave)"
          />

          <TextField
            fullWidth
            multiline
            rows={4}
            label="Comment"
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            placeholder="Add notes or comments for this day..."
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button onClick={handleSave} variant="contained">
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
}
