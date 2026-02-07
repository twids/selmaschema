import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { ThemeProvider } from '@mui/material/styles';
import { theme } from '../theme';
import MonthControls from '../components/MonthControls';

/* ---------- mock ---------- */

const mockUseCalendar = vi.fn();

vi.mock('../context/CalendarContext', () => ({
  useCalendar: () => mockUseCalendar(),
}));

/* ---------- helpers ---------- */

function renderWithTheme(ui: React.ReactElement) {
  return render(<ThemeProvider theme={theme}>{ui}</ThemeProvider>);
}

const defaultCalendarCtx = {
  currentYear: 2026,
  currentMonth: 2,
  calendarData: {},
  loading: false,
  error: null,
  loadMonthData: vi.fn(),
  updateDay: vi.fn(),
  initializeMonth: vi.fn(),
  setMonth: vi.fn(),
  setYear: vi.fn(),
  navigateMonth: vi.fn(),
};

/* ---------- tests ---------- */

describe('MonthControls', () => {
  beforeEach(() => {
    mockUseCalendar.mockReturnValue(defaultCalendarCtx);
  });

  it('renders previous and next navigation buttons', () => {
    renderWithTheme(<MonthControls />);
    expect(screen.getByLabelText('Föregående månad')).toBeInTheDocument();
    expect(screen.getByLabelText('Nästa månad')).toBeInTheDocument();
  });

  it('calls navigateMonth(-1) when previous button is clicked', () => {
    const navigateMonth = vi.fn();
    mockUseCalendar.mockReturnValue({ ...defaultCalendarCtx, navigateMonth });
    renderWithTheme(<MonthControls />);
    fireEvent.click(screen.getByLabelText('Föregående månad'));
    expect(navigateMonth).toHaveBeenCalledWith(-1);
  });

  it('calls navigateMonth(1) when next button is clicked', () => {
    const navigateMonth = vi.fn();
    mockUseCalendar.mockReturnValue({ ...defaultCalendarCtx, navigateMonth });
    renderWithTheme(<MonthControls />);
    fireEvent.click(screen.getByLabelText('Nästa månad'));
    expect(navigateMonth).toHaveBeenCalledWith(1);
  });

  it('displays the current month name', () => {
    renderWithTheme(<MonthControls />);
    // February = Februari in Swedish, shown as selected value in month Select
    expect(screen.getByText('Februari')).toBeInTheDocument();
  });

  it('displays the current year', () => {
    renderWithTheme(<MonthControls />);
    expect(screen.getByText('2026')).toBeInTheDocument();
  });

  it('renders initialize button', () => {
    renderWithTheme(<MonthControls />);
    expect(
      screen.getByRole('button', { name: /initiera/i }),
    ).toBeInTheDocument();
  });

  it('calls initializeMonth when initialize button is clicked', () => {
    const initializeMonth = vi.fn();
    mockUseCalendar.mockReturnValue({ ...defaultCalendarCtx, initializeMonth });
    renderWithTheme(<MonthControls />);
    fireEvent.click(screen.getByRole('button', { name: /initiera/i }));
    expect(initializeMonth).toHaveBeenCalledWith(2026, 2);
  });
});
