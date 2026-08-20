import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ThemeProvider } from '@mui/material/styles';
import { theme } from '../theme';
import DayModal from '../components/DayModal';
import type { CalendarData, DayAssignmentDto } from '../api/types';

/* ---------- mocks ---------- */

const mockUpdateDay = vi.fn();
const mockUseCalendar = vi.fn();
const mockUseConfig = vi.fn();

vi.mock('../context/CalendarContext', () => ({
  useCalendar: () => mockUseCalendar(),
}));

vi.mock('../context/ConfigContext', () => ({
  useConfig: () => mockUseConfig(),
}));

/* ---------- helpers ---------- */

function renderWithTheme(ui: React.ReactElement) {
  return render(<ThemeProvider theme={theme}>{ui}</ThemeProvider>);
}

const baseDayData: DayAssignmentDto = {
  id: 1,
  date: '2026-02-09',
  parent: null,
  isVAB: false,
  specialStatus: null,
  parentAComments: [],
  parentBComments: [],
};

const defaultCalendarCtx = {
  currentYear: 2026,
  currentMonth: 2,
  calendarData: {
    '2026-02-09': baseDayData,
  } as CalendarData,
  loading: false,
  error: null,
  loadMonthData: vi.fn(),
  updateDay: mockUpdateDay,
  initializeMonth: vi.fn(),
  setMonth: vi.fn(),
  setYear: vi.fn(),
  navigateMonth: vi.fn(),
};

const defaultConfigCtx = {
  parentNames: { parentAName: 'Tomas', parentBName: 'Carro' },
  loading: false,
  updateParentNames: vi.fn(),
};

/* ---------- tests ---------- */

describe('DayModal', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUpdateDay.mockResolvedValue(undefined);
    mockUseCalendar.mockReturnValue({ ...defaultCalendarCtx, updateDay: mockUpdateDay });
    mockUseConfig.mockReturnValue(defaultConfigCtx);
  });

  describe('rendering', () => {
    it('renders dialog with data-testid when open', () => {
      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );
      expect(screen.getByTestId('day-modal')).toBeInTheDocument();
    });

    it('does not render dialog content when closed', () => {
      renderWithTheme(
        <DayModal open={false} dateKey="2026-02-09" onClose={vi.fn()} />,
      );
      expect(screen.queryByTestId('day-modal')).not.toBeInTheDocument();
    });

    it('shows formatted Swedish date in title', () => {
      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );
      // Feb 9, 2026 is a Monday
      expect(screen.getByText('Måndag 9 Februari 2026')).toBeInTheDocument();
    });

    it('shows parent assignment select with parent names', () => {
      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );
      expect(screen.getByLabelText(new RegExp('Tilldelad till'))).toBeInTheDocument();
    });

    it('shows VAB checkbox', () => {
      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );
      expect(screen.getByLabelText('Markera som VAB')).toBeInTheDocument();
    });

    it('shows special status select', () => {
      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );
      expect(screen.getByLabelText(new RegExp('Special status'))).toBeInTheDocument();
    });

    it('shows Save and Cancel buttons', () => {
      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );
      expect(screen.getByRole('button', { name: 'Spara' })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: 'Avbryt' })).toBeInTheDocument();
    });
  });

  describe('defaults from existing data', () => {
    it('pre-fills parent A when day data has parent A', () => {
      const calCtx = {
        ...defaultCalendarCtx,
        calendarData: {
          '2026-02-09': { ...baseDayData, parent: 'A' },
        },
        updateDay: mockUpdateDay,
      };
      mockUseCalendar.mockReturnValue(calCtx);

      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );

      // The parent select should show "Tomas" (parent A name)
      const parentSelect = screen.getByLabelText(new RegExp('Tilldelad till'));
      expect(parentSelect).toHaveTextContent('Tomas');
    });

    it('pre-fills VAB checkbox when day is VAB', () => {
      const calCtx = {
        ...defaultCalendarCtx,
        calendarData: {
          '2026-02-09': { ...baseDayData, isVAB: true },
        },
        updateDay: mockUpdateDay,
      };
      mockUseCalendar.mockReturnValue(calCtx);

      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );

      const checkbox = screen.getByLabelText('Markera som VAB');
      expect(checkbox).toBeChecked();
    });

    it('pre-fills special status when day has one', () => {
      const calCtx = {
        ...defaultCalendarCtx,
        calendarData: {
          '2026-02-09': { ...baseDayData, specialStatus: 'Holiday' },
        },
        updateDay: mockUpdateDay,
      };
      mockUseCalendar.mockReturnValue(calCtx);

      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );

      const statusSelect = screen.getByLabelText(new RegExp('Special status'));
      expect(statusSelect).toHaveTextContent('Helgdag');
    });

    it('defaults to unassigned when no day data exists', () => {
      const calCtx = {
        ...defaultCalendarCtx,
        calendarData: {},
        updateDay: mockUpdateDay,
      };
      mockUseCalendar.mockReturnValue(calCtx);

      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );

      // MUI Select with value="" won't display text; verify neither parent name is shown
      const parentSelect = screen.getByLabelText(new RegExp('Tilldelad till'));
      expect(parentSelect).not.toHaveTextContent('Tomas');
      expect(parentSelect).not.toHaveTextContent('Carro');
    });
  });

  describe('interactions', () => {
    it('calls onClose when Cancel button is clicked', async () => {
      const onClose = vi.fn();
      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={onClose} />,
      );

      fireEvent.click(screen.getByRole('button', { name: 'Avbryt' }));
      expect(onClose).toHaveBeenCalledOnce();
    });

    it('calls updateDay with correct data on save', async () => {
      const onClose = vi.fn();
      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={onClose} />,
      );

      fireEvent.click(screen.getByRole('button', { name: 'Spara' }));

      await waitFor(() => {
        expect(mockUpdateDay).toHaveBeenCalledWith(2026, 2, 9, {
          parent: null,
          isVAB: false,
          specialStatus: null,
        });
      });
    });

    it('closes dialog after successful save', async () => {
      const onClose = vi.fn();
      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={onClose} />,
      );

      fireEvent.click(screen.getByRole('button', { name: 'Spara' }));

      await waitFor(() => {
        expect(onClose).toHaveBeenCalledOnce();
      });
    });

    it('toggles VAB checkbox', async () => {
      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );

      const checkbox = screen.getByLabelText('Markera som VAB');
      expect(checkbox).not.toBeChecked();

      fireEvent.click(checkbox);
      expect(checkbox).toBeChecked();
    });
  });

  describe('loading state', () => {
    it('disables Save button while saving', async () => {
      // Make updateDay hang so we can check loading state
      mockUpdateDay.mockImplementation(
        () => new Promise(() => {}), // never resolves
      );

      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );

      fireEvent.click(screen.getByRole('button', { name: 'Spara' }));

      await waitFor(() => {
        expect(screen.getByRole('button', { name: 'Spara' })).toBeDisabled();
      });
    });

    it('disables Cancel button while saving', async () => {
      mockUpdateDay.mockImplementation(
        () => new Promise(() => {}),
      );

      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );

      fireEvent.click(screen.getByRole('button', { name: 'Spara' }));

      await waitFor(() => {
        expect(screen.getByRole('button', { name: 'Avbryt' })).toBeDisabled();
      });
    });

    it('shows CircularProgress while saving', async () => {
      mockUpdateDay.mockImplementation(
        () => new Promise(() => {}),
      );

      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );

      fireEvent.click(screen.getByRole('button', { name: 'Spara' }));

      await waitFor(() => {
        expect(screen.getByRole('progressbar')).toBeInTheDocument();
      });
    });
  });

  describe('error handling', () => {
    it('shows error Alert when save fails', async () => {
      mockUpdateDay.mockRejectedValue(new Error('Network error'));

      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );

      fireEvent.click(screen.getByRole('button', { name: 'Spara' }));

      await waitFor(() => {
        expect(screen.getByRole('alert')).toBeInTheDocument();
        expect(screen.getByText(/Network error/)).toBeInTheDocument();
      });
    });

    it('re-enables buttons after save error', async () => {
      mockUpdateDay.mockRejectedValue(new Error('Network error'));

      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={vi.fn()} />,
      );

      fireEvent.click(screen.getByRole('button', { name: 'Spara' }));

      await waitFor(() => {
        expect(screen.getByRole('button', { name: 'Spara' })).toBeEnabled();
        expect(screen.getByRole('button', { name: 'Avbryt' })).toBeEnabled();
      });
    });

    it('does not close dialog when save fails', async () => {
      const onClose = vi.fn();
      mockUpdateDay.mockRejectedValue(new Error('fail'));

      renderWithTheme(
        <DayModal open={true} dateKey="2026-02-09" onClose={onClose} />,
      );

      fireEvent.click(screen.getByRole('button', { name: 'Spara' }));

      await waitFor(() => {
        expect(screen.getByRole('alert')).toBeInTheDocument();
      });
      expect(onClose).not.toHaveBeenCalled();
    });
  });

  describe('dateKey parsing', () => {
    it('parses dateKey to extract correct year/month/day for updateDay', async () => {
      const calCtx = {
        ...defaultCalendarCtx,
        calendarData: {
          '2026-12-25': {
            ...baseDayData,
            date: '2026-12-25',
            parent: 'B',
            isVAB: true,
            specialStatus: 'Holiday',
          },
        },
        updateDay: mockUpdateDay,
      };
      mockUseCalendar.mockReturnValue(calCtx);

      renderWithTheme(
        <DayModal open={true} dateKey="2026-12-25" onClose={vi.fn()} />,
      );

      fireEvent.click(screen.getByRole('button', { name: 'Spara' }));

      await waitFor(() => {
        expect(mockUpdateDay).toHaveBeenCalledWith(2026, 12, 25, {
          parent: 'B',
          isVAB: true,
          specialStatus: 'Holiday',
        });
      });
    });
  });

  describe('null dateKey', () => {
    it('does not crash when dateKey is null and open is true', () => {
      renderWithTheme(
        <DayModal open={true} dateKey={null} onClose={vi.fn()} />,
      );
      // Should still render dialog but without meaningful content
      expect(screen.getByTestId('day-modal')).toBeInTheDocument();
    });
  });
});
