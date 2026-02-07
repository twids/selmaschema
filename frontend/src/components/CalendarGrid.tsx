import { Fragment } from 'react';
import { Box, Typography, Skeleton } from '@mui/material';
import { useCalendar } from '../context/CalendarContext';
import { getMonthGrid, formatDateKey } from '../utils/dateUtils';
import { sv } from '../i18n/sv';
import DayCell from './DayCell';

interface CalendarGridProps {
  onDayClick?: (date: Date) => void;
}

/** Monday-first day name headers derived from sv.ts (which is Sunday-first). */
const dayHeaders = [
  sv.days.short[1], // Mån
  sv.days.short[2], // Tis
  sv.days.short[3], // Ons
  sv.days.short[4], // Tor
  sv.days.short[5], // Fre
  sv.days.short[6], // Lör
  sv.days.short[0], // Sön
];

export default function CalendarGrid({ onDayClick }: CalendarGridProps) {
  const { currentYear, currentMonth, calendarData, loading } = useCalendar();

  if (loading) {
    return (
      <Box data-testid="calendar-loading" sx={{ p: 2 }}>
        {Array.from({ length: 5 }, (_, i) => (
          <Skeleton
            key={i}
            variant="rectangular"
            height={64}
            sx={{ mb: 1, borderRadius: 1 }}
          />
        ))}
      </Box>
    );
  }

  const grid = getMonthGrid(currentYear, currentMonth);

  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: {
          xs: 'auto repeat(7, 1fr)',
          sm: '48px repeat(7, 1fr)',
        },
        gap: 0.5,
      }}
    >
      {/* Header row: empty corner + 7 day names */}
      <Box />
      {dayHeaders.map((name) => (
        <Typography
          key={name}
          variant="body2"
          fontWeight="bold"
          textAlign="center"
          sx={{ py: 0.5 }}
        >
          {name}
        </Typography>
      ))}

      {/* Week rows */}
      {grid.map((week) => (
        <Fragment key={week.weekNumber}>
          {/* Week number column */}
          <Box
            data-testid={`week-number-${week.weekNumber}`}
            sx={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'text.secondary',
            }}
          >
            <Typography variant="caption">{week.weekNumber}</Typography>
          </Box>

          {/* 7 day slots */}
          {week.days.map((day, idx) =>
            day ? (
              <DayCell
                key={formatDateKey(day)}
                date={day}
                dayData={calendarData[formatDateKey(day)]}
                onClick={onDayClick}
              />
            ) : (
              <Box key={`empty-${week.weekNumber}-${idx}`} />
            ),
          )}
        </Fragment>
      ))}
    </Box>
  );
}
