import { Paper, Typography, Chip, Box } from '@mui/material';
import CommentIcon from '@mui/icons-material/Comment';
import { useConfig } from '../context/ConfigContext';
import { formatDateKey } from '../utils/dateUtils';
import type { DayAssignmentDto } from '../api/types';

interface DayCellProps {
  date: Date;
  dayData?: DayAssignmentDto;
  onClick?: (date: Date) => void;
}

export default function DayCell({ date, dayData, onClick }: DayCellProps) {
  const { parentNames } = useConfig();

  const dateKey = formatDateKey(date);
  const dayNum = date.getDate();
  const isWeekend = date.getDay() === 0 || date.getDay() === 6;

  const parent = dayData?.parent ?? null;
  const parentName =
    parent === 'A'
      ? parentNames.parentAName
      : parent === 'B'
        ? parentNames.parentBName
        : null;

  const hasComments =
    (dayData?.parentAComments?.length ?? 0) +
      (dayData?.parentBComments?.length ?? 0) >
    0;

  const getBgColor = (): string => {
    if (parent === 'A') return 'primary.light';
    if (parent === 'B') return 'secondary.light';
    return isWeekend ? 'grey.200' : 'grey.100';
  };

  return (
    <Paper
      data-testid={`day-cell-${dateKey}`}
      elevation={1}
      onClick={() => onClick?.(date)}
      sx={{
        p: 0.5,
        minHeight: { xs: 48, sm: 64 },
        cursor: 'pointer',
        bgcolor: getBgColor(),
        '&:hover': { filter: 'brightness(0.95)' },
        display: 'flex',
        flexDirection: 'column',
        position: 'relative',
        overflow: 'hidden',
      }}
    >
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'flex-start',
        }}
      >
        <Typography variant="body2" fontWeight="bold">
          {dayNum}
        </Typography>
        {hasComments && (
          <CommentIcon
            data-testid="comment-indicator"
            sx={{ fontSize: 14, color: 'text.secondary' }}
          />
        )}
      </Box>

      {parentName && (
        <Typography
          variant="caption"
          sx={{ textAlign: 'center', mt: 'auto' }}
        >
          {parentName}
        </Typography>
      )}

      {dayData?.isVAB && (
        <Chip
          label="VAB"
          size="small"
          color="warning"
          sx={{ mt: 0.5, height: 18, fontSize: '0.65rem' }}
        />
      )}
    </Paper>
  );
}
