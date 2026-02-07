import { Box, Chip, Stack, Typography } from '@mui/material';
import { useConfig } from '../context/ConfigContext';

export default function Legend() {
  const { parentNames } = useConfig();

  return (
    <Stack
      direction="row"
      spacing={2}
      alignItems="center"
      flexWrap="wrap"
      sx={{ py: 1 }}
    >
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
        <Box
          sx={{
            width: 16,
            height: 16,
            bgcolor: 'primary.light',
            borderRadius: 0.5,
          }}
        />
        <Typography variant="body2">{parentNames.parentAName}</Typography>
      </Box>

      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
        <Box
          sx={{
            width: 16,
            height: 16,
            bgcolor: 'secondary.light',
            borderRadius: 0.5,
          }}
        />
        <Typography variant="body2">{parentNames.parentBName}</Typography>
      </Box>

      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
        <Box
          sx={{
            width: 16,
            height: 16,
            bgcolor: 'grey.300',
            borderRadius: 0.5,
          }}
        />
        <Typography variant="body2">Ej tilldelad</Typography>
      </Box>

      <Chip label="VAB" size="small" color="warning" />
    </Stack>
  );
}
