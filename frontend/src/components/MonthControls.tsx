import {
  Box,
  Button,
  IconButton,
  MenuItem,
  Select,
  Stack,
} from '@mui/material';
import NavigateBefore from '@mui/icons-material/NavigateBefore';
import NavigateNext from '@mui/icons-material/NavigateNext';
import { useCalendar } from '../context/CalendarContext';
import { sv } from '../i18n/sv';

/** Year range: current real year -2 … +5. */
const BASE_YEAR = new Date().getFullYear();
const yearOptions = Array.from({ length: 8 }, (_, i) => BASE_YEAR - 2 + i);

export default function MonthControls() {
  const {
    currentYear,
    currentMonth,
    navigateMonth,
    setMonth,
    setYear,
    initializeMonth,
  } = useCalendar();

  return (
    <Stack direction="row" alignItems="center" spacing={1} flexWrap="wrap">
      <IconButton
        onClick={() => navigateMonth(-1)}
        aria-label="Föregående månad"
      >
        <NavigateBefore />
      </IconButton>

      <Select
        value={currentMonth}
        onChange={(e) => setMonth(Number(e.target.value))}
        size="small"
        aria-label="Månad"
      >
        {sv.months.long.map((name, idx) => (
          <MenuItem key={idx + 1} value={idx + 1}>
            {name}
          </MenuItem>
        ))}
      </Select>

      <Select
        value={currentYear}
        onChange={(e) => setYear(Number(e.target.value))}
        size="small"
        aria-label="År"
      >
        {yearOptions.map((y) => (
          <MenuItem key={y} value={y}>
            {y}
          </MenuItem>
        ))}
      </Select>

      <IconButton
        onClick={() => navigateMonth(1)}
        aria-label="Nästa månad"
      >
        <NavigateNext />
      </IconButton>

      <Box sx={{ flexGrow: 1 }} />

      <Button
        variant="outlined"
        size="small"
        onClick={() => initializeMonth(currentYear, currentMonth)}
      >
        Initiera månad
      </Button>
    </Stack>
  );
}
