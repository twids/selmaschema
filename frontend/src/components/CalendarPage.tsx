import { Box, Typography } from '@mui/material';
import { CalendarProvider } from '../context/CalendarContext';
import { ConfigProvider } from '../context/ConfigContext';
import CalendarGrid from './CalendarGrid';
import MonthControls from './MonthControls';
import Legend from './Legend';

export default function CalendarPage() {
  return (
    <CalendarProvider>
      <ConfigProvider>
        <Box sx={{ p: 2 }}>
          <Typography variant="h4" gutterBottom>
            Kalender
          </Typography>
          <MonthControls />
          <Legend />
          <CalendarGrid />
        </Box>
      </ConfigProvider>
    </CalendarProvider>
  );
}
