import { Box, Paper, Typography, Button, Stack } from '@mui/material';
import { CalendarData, ParentNames } from '../types';
import DayCell from './DayCell';

interface MonthProps {
  monthName: string;
  monthIndex: number;
  currentYear: number;
  calendarData: CalendarData;
  parentNames: ParentNames;
  onDayClick: (dateKey: string) => void;
  onFillMonth: (parent: 'parentA' | 'parentB', monthIndex: number, year: number) => void;
  onAlternateMonth: (monthIndex: number, year: number) => void;
  onInitializeMonth: () => void;
}

const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

function getDaysInMonth(year: number, month: number): number {
  return new Date(year, month + 1, 0).getDate();
}

function getFirstDayOfMonth(year: number, month: number): number {
  return new Date(year, month, 1).getDay();
}

function getDateKey(year: number, month: number, day: number): string {
  return `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
}

export default function Month({
  monthName,
  monthIndex,
  currentYear,
  calendarData,
  parentNames,
  onDayClick,
  onFillMonth,
  onAlternateMonth,
  onInitializeMonth,
}: MonthProps) {
  const daysInMonth = getDaysInMonth(currentYear, monthIndex);
  const firstDay = getFirstDayOfMonth(currentYear, monthIndex);

  const days = [];

  // Add empty cells for days before month starts
  for (let i = 0; i < firstDay; i++) {
    days.push(<Box key={`empty-${i}`} />);
  }

  // Add day cells
  for (let day = 1; day <= daysInMonth; day++) {
    const dateKey = getDateKey(currentYear, monthIndex, day);
    const dayData = calendarData[dateKey];

    days.push(
      <DayCell
        key={dateKey}
        day={day}
        dateKey={dateKey}
        dayData={dayData}
        parentNames={parentNames}
        onClick={() => onDayClick(dateKey)}
      />
    );
  }

  return (
    <Paper elevation={2} sx={{ p: 2 }}>
      <Box sx={{ mb: 2 }}>
        <Typography variant="h5" gutterBottom>
          {monthName} {currentYear}
        </Typography>
        <Stack direction="row" spacing={1} flexWrap="wrap">
          <Button
            variant="outlined"
            size="small"
            onClick={() => onFillMonth('parentA', monthIndex, currentYear)}
          >
            Fill {parentNames.parentA}
          </Button>
          <Button
            variant="outlined"
            size="small"
            onClick={() => onFillMonth('parentB', monthIndex, currentYear)}
          >
            Fill {parentNames.parentB}
          </Button>
          <Button
            variant="outlined"
            size="small"
            onClick={() => onAlternateMonth(monthIndex, currentYear)}
          >
            Alternate Days
          </Button>
          <Button
            variant="contained"
            size="small"
            onClick={() => onInitializeMonth()}
          >
            Initialize with Defaults
          </Button>
        </Stack>
      </Box>
      
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: 'repeat(7, 1fr)',
          gap: 1,
        }}
      >
        {dayNames.map((dayName) => (
          <Box
            key={dayName}
            sx={{
              fontWeight: 'bold',
              textAlign: 'center',
              py: 1,
              bgcolor: 'primary.main',
              color: 'primary.contrastText',
              borderRadius: 1,
            }}
          >
            {dayName}
          </Box>
        ))}
        {days}
      </Box>
    </Paper>
  );
}
