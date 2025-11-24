import { useRef } from 'react';
import {
  Box,
  Paper,
  Stack,
  Button,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  IconButton,
  Typography,
} from '@mui/material';
import NavigateBeforeIcon from '@mui/icons-material/NavigateBefore';
import NavigateNextIcon from '@mui/icons-material/NavigateNext';
import DownloadIcon from '@mui/icons-material/Download';
import UploadIcon from '@mui/icons-material/Upload';
import { ParentNames } from '../types';

interface ControlsProps {
  currentYear: number;
  currentMonth: number;
  onYearChange: (year: number) => void;
  onMonthChange: (month: number) => void;
  onNavigateMonth: (delta: number) => void;
  parentNames: ParentNames;
  onParentNamesChange: (names: ParentNames) => void;
  onExport: () => void;
  onImport: (file: File) => void;
}

export default function Controls({
  currentYear,
  currentMonth,
  onYearChange,
  onMonthChange,
  onNavigateMonth,
  parentNames,
  onParentNamesChange,
  onExport,
  onImport,
}: ControlsProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const parentARef = useRef<HTMLInputElement>(null);
  const parentBRef = useRef<HTMLInputElement>(null);

  const handleUpdateNames = () => {
    onParentNamesChange({
      parentA: parentARef.current?.value || 'Parent A',
      parentB: parentBRef.current?.value || 'Parent B',
    });
  };

  const handleImportClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      onImport(file);
      e.target.value = '';
    }
  };

  const years = [];
  const currentYearActual = new Date().getFullYear();
  for (let year = currentYearActual - 2; year <= currentYearActual + 5; year++) {
    years.push(year);
  }

  const months = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  return (
    <Paper elevation={2} sx={{ p: 2 }}>
      <Stack spacing={3}>
        {/* Month Navigation */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
          <IconButton onClick={() => onNavigateMonth(-1)} color="primary">
            <NavigateBeforeIcon />
          </IconButton>
          
          <FormControl size="small" sx={{ minWidth: 150 }}>
            <InputLabel>Month</InputLabel>
            <Select
              value={currentMonth}
              label="Month"
              onChange={(e) => onMonthChange(Number(e.target.value))}
            >
              {months.map((month, index) => (
                <MenuItem key={index} value={index}>
                  {month}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <FormControl size="small" sx={{ minWidth: 100 }}>
            <InputLabel>Year</InputLabel>
            <Select
              value={currentYear}
              label="Year"
              onChange={(e) => onYearChange(Number(e.target.value))}
            >
              {years.map((year) => (
                <MenuItem key={year} value={year}>
                  {year}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <IconButton onClick={() => onNavigateMonth(1)} color="primary">
            <NavigateNextIcon />
          </IconButton>
        </Box>

        {/* Parent Names */}
        <Box>
          <Typography variant="subtitle2" gutterBottom>
            Parent Names
          </Typography>
          <Stack direction="row" spacing={2} alignItems="center" flexWrap="wrap">
            <TextField
              inputRef={parentARef}
              size="small"
              label="Parent A"
              defaultValue={parentNames.parentA}
              sx={{ minWidth: 150 }}
            />
            <TextField
              inputRef={parentBRef}
              size="small"
              label="Parent B"
              defaultValue={parentNames.parentB}
              sx={{ minWidth: 150 }}
            />
            <Button variant="contained" onClick={handleUpdateNames}>
              Update Names
            </Button>
          </Stack>
        </Box>

        {/* Data Actions */}
        <Box>
          <Typography variant="subtitle2" gutterBottom>
            Data Management
          </Typography>
          <Stack direction="row" spacing={2}>
            <Button
              variant="outlined"
              startIcon={<DownloadIcon />}
              onClick={onExport}
            >
              Export Data
            </Button>
            <Button
              variant="outlined"
              startIcon={<UploadIcon />}
              onClick={handleImportClick}
            >
              Import Data
            </Button>
            <input
              ref={fileInputRef}
              type="file"
              accept=".json"
              style={{ display: 'none' }}
              onChange={handleFileChange}
            />
          </Stack>
        </Box>
      </Stack>
    </Paper>
  );
}
