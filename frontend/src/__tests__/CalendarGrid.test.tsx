import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ThemeProvider } from '@mui/material/styles';
import { theme } from '../theme';
import CalendarGrid from '../components/CalendarGrid';
import type { CalendarData, DayAssignmentDto } from '../api/types';

/* ---------- mocks ---------- */

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

const defaultCalendarCtx = {
  currentYear: 2026,
  currentMonth: 6, // June 2026 starts on Monday, 30 days
  calendarData: {} as CalendarData,
  loading: false,
  error: null,
  loadMonthData: vi.fn(),
  updateDay: vi.fn(),
  initializeMonth: vi.fn(),
  setMonth: vi.fn(),
  setYear: vi.fn(),
  navigateMonth: vi.fn(),
};

const defaultConfigCtx = {
  parentNames: { parentAName: 'Alice', parentBName: 'Bob' },
  loading: false,
  updateParentNames: vi.fn(),
};

/* ---------- tests ---------- */

describe('CalendarGrid', () => {
  beforeEach(() => {
    mockUseCalendar.mockReturnValue(defaultCalendarCtx);
    mockUseConfig.mockReturnValue(defaultConfigCtx);
  });

  it('renders day name headers (Mon-Sun in Swedish)', () => {
    renderWithTheme(<CalendarGrid />);
    expect(screen.getByText('Mån')).toBeInTheDocument();
    expect(screen.getByText('Tis')).toBeInTheDocument();
    expect(screen.getByText('Ons')).toBeInTheDocument();
    expect(screen.getByText('Tor')).toBeInTheDocument();
    expect(screen.getByText('Fre')).toBeInTheDocument();
    expect(screen.getByText('Lör')).toBeInTheDocument();
    expect(screen.getByText('Sön')).toBeInTheDocument();
  });

  it('renders week numbers', () => {
    renderWithTheme(<CalendarGrid />);
    const weekNumbers = screen.getAllByTestId(/^week-number-/);
    expect(weekNumbers.length).toBeGreaterThan(0);
  });

  it('renders day cells for each day of the month', () => {
    renderWithTheme(<CalendarGrid />);
    // June has 30 days
    const dayCells = screen.getAllByTestId(/^day-cell-/);
    expect(dayCells).toHaveLength(30);
  });

  it('shows loading skeleton when loading', () => {
    mockUseCalendar.mockReturnValue({ ...defaultCalendarCtx, loading: true });
    renderWithTheme(<CalendarGrid />);
    expect(screen.getByTestId('calendar-loading')).toBeInTheDocument();
  });

  it('does not show day cells when loading', () => {
    mockUseCalendar.mockReturnValue({ ...defaultCalendarCtx, loading: true });
    renderWithTheme(<CalendarGrid />);
    expect(screen.queryAllByTestId(/^day-cell-/)).toHaveLength(0);
  });

  it('renders correct day cells with calendar data', () => {
    const dayAssignment: DayAssignmentDto = {
      id: 1,
      date: '2026-06-01',
      parent: 'A',
      isVAB: false,
      specialStatus: null,
      parentAComments: [],
      parentBComments: [],
    };
    const calendarData: CalendarData = {
      '2026-06-01': dayAssignment,
    };
    mockUseCalendar.mockReturnValue({ ...defaultCalendarCtx, calendarData });
    renderWithTheme(<CalendarGrid />);
    // Parent A name should be displayed for June 1
    expect(screen.getByText('Alice')).toBeInTheDocument();
  });

  it('renders for February 2026 (28 days)', () => {
    mockUseCalendar.mockReturnValue({
      ...defaultCalendarCtx,
      currentMonth: 2,
    });
    renderWithTheme(<CalendarGrid />);
    const dayCells = screen.getAllByTestId(/^day-cell-/);
    expect(dayCells).toHaveLength(28);
  });
});
