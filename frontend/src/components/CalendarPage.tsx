import { useCallback, useState } from 'react';
import { Box, Typography } from '@mui/material';
import { CalendarProvider } from '../context/CalendarContext';
import { ConfigProvider } from '../context/ConfigContext';
import CalendarGrid from './CalendarGrid';
import MonthControls from './MonthControls';
import Legend from './Legend';
import DayModal from './DayModal';
import Statistics from './Statistics';
import ParentNameEditor from './ParentNameEditor';
import { formatDateKey } from '../utils/dateUtils';

function CalendarPageContent() {
  const [selectedDate, setSelectedDate] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);

  const handleDayClick = useCallback((date: Date) => {
    setSelectedDate(formatDateKey(date));
    setModalOpen(true);
  }, []);

  const handleModalClose = useCallback(() => {
    setModalOpen(false);
  }, []);

  return (
    <Box sx={{ p: 2 }}>
      <Typography variant="h4" gutterBottom>
        Kalender
      </Typography>
      <MonthControls />
      <Legend />
      <CalendarGrid onDayClick={handleDayClick} />
      <DayModal
        open={modalOpen}
        dateKey={selectedDate}
        onClose={handleModalClose}
      />
      <Statistics />
      <ParentNameEditor />
    </Box>
  );
}

export default function CalendarPage() {
  return (
    <CalendarProvider>
      <ConfigProvider>
        <CalendarPageContent />
      </ConfigProvider>
    </CalendarProvider>
  );
}
