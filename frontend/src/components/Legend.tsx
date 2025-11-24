import { Paper, Stack, Typography, Chip } from '@mui/material';
import { ParentNames } from '../types';

interface LegendProps {
  parentNames: ParentNames;
}

export default function Legend({ parentNames }: LegendProps) {
  return (
    <Paper elevation={1} sx={{ p: 2, mt: 2 }}>
      <Typography variant="subtitle2" gutterBottom>
        Legend
      </Typography>
      <Stack direction="row" spacing={2} flexWrap="wrap">
        <Chip
          label={parentNames.parentA}
          sx={{ bgcolor: '#bbdefb', color: '#000' }}
        />
        <Chip
          label={parentNames.parentB}
          sx={{ bgcolor: '#f8bbd0', color: '#000' }}
        />
        <Chip
          label="Unassigned"
          sx={{ bgcolor: '#f5f5f5', color: '#000' }}
        />
        <Chip
          label="VAB (Child Care Leave)"
          variant="outlined"
          sx={{ borderColor: '#ffc107', borderWidth: 2 }}
        />
      </Stack>
    </Paper>
  );
}

