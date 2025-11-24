import { Box, Typography, Tooltip, Chip, useTheme } from '@mui/material';
import { DayData, ParentNames } from '../types';

interface DayCellProps {
  day: number;
  dateKey: string;
  dayData?: DayData;
  parentNames: ParentNames;
  onClick: () => void;
}

export default function DayCell({
  day,
  dayData,
  parentNames,
  onClick,
}: DayCellProps) {
  const theme = useTheme();
  
  const getParentName = () => {
    if (!dayData || !dayData.parent) return 'Unassigned';
    return dayData.parent === 'parentA' ? parentNames.parentA : parentNames.parentB;
  };

  const getBgColor = () => {
    if (!dayData || !dayData.parent) return '#f5f5f5';
    return dayData.parent === 'parentA' ? '#bbdefb' : '#f8bbd0';
  };

  const truncateComment = (comment: string) => {
    if (!comment) return '';
    return comment.length > 30 ? comment.substring(0, 30) + '...' : comment;
  };

  const cellContent = (
    <Box
      onClick={onClick}
      sx={{
        bgcolor: getBgColor(),
        border: dayData?.isVAB ? `3px solid ${theme.palette.warning.main}` : '1px solid #ddd',
        borderRadius: 1,
        p: 1,
        minHeight: '100px',
        cursor: 'pointer',
        '&:hover': {
          opacity: 0.8,
          boxShadow: 2,
        },
        display: 'flex',
        flexDirection: 'column',
      }}
    >
      <Typography variant="h6" fontWeight="bold">
        {day}
      </Typography>
      <Typography variant="caption" sx={{ mt: 0.5 }}>
        {getParentName()}
      </Typography>
      {dayData?.comment && (
        <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5 }}>
          {truncateComment(dayData.comment)}
        </Typography>
      )}
      {dayData?.isVAB && (
        <Chip label="VAB" size="small" color="warning" sx={{ mt: 'auto', alignSelf: 'flex-start' }} />
      )}
    </Box>
  );

  return dayData?.comment ? (
    <Tooltip title={dayData.comment} arrow>
      {cellContent}
    </Tooltip>
  ) : (
    cellContent
  );
}
