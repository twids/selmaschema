import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { ThemeProvider } from '@mui/material/styles';
import { theme } from '../theme';
import DayCell from '../components/DayCell';
import type { DayAssignmentDto } from '../api/types';

/* ---------- mock useConfig ---------- */

const mockUseConfig = vi.fn();

vi.mock('../context/ConfigContext', () => ({
  useConfig: () => mockUseConfig(),
}));

/* ---------- helpers ---------- */

const parentNames = { parentAName: 'Alice', parentBName: 'Bob' };

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

/* ---------- tests ---------- */

describe('DayCell', () => {
  beforeEach(() => {
    mockUseConfig.mockReturnValue({
      parentNames,
      loading: false,
      updateParentNames: vi.fn(),
    });
  });

  it('displays the day number', () => {
    const date = new Date(2026, 1, 9);
    renderWithTheme(<DayCell date={date} />);
    expect(screen.getByText('9')).toBeInTheDocument();
  });

  it('has correct data-testid', () => {
    const date = new Date(2026, 1, 9);
    renderWithTheme(<DayCell date={date} />);
    expect(screen.getByTestId('day-cell-2026-02-09')).toBeInTheDocument();
  });

  it('shows Parent A name when assigned to A', () => {
    const date = new Date(2026, 1, 9);
    const dayData: DayAssignmentDto = { ...baseDayData, parent: 'A' };
    renderWithTheme(<DayCell date={date} dayData={dayData} />);
    expect(screen.getByText('Alice')).toBeInTheDocument();
  });

  it('shows Parent B name when assigned to B', () => {
    const date = new Date(2026, 1, 9);
    const dayData: DayAssignmentDto = { ...baseDayData, parent: 'B' };
    renderWithTheme(<DayCell date={date} dayData={dayData} />);
    expect(screen.getByText('Bob')).toBeInTheDocument();
  });

  it('shows no parent name when unassigned (no dayData)', () => {
    const date = new Date(2026, 1, 9);
    renderWithTheme(<DayCell date={date} />);
    expect(screen.queryByText('Alice')).not.toBeInTheDocument();
    expect(screen.queryByText('Bob')).not.toBeInTheDocument();
  });

  it('shows no parent name when parent is null', () => {
    const date = new Date(2026, 1, 9);
    renderWithTheme(<DayCell date={date} dayData={baseDayData} />);
    expect(screen.queryByText('Alice')).not.toBeInTheDocument();
    expect(screen.queryByText('Bob')).not.toBeInTheDocument();
  });

  it('shows VAB chip when isVAB is true', () => {
    const date = new Date(2026, 1, 9);
    const dayData: DayAssignmentDto = { ...baseDayData, parent: 'A', isVAB: true };
    renderWithTheme(<DayCell date={date} dayData={dayData} />);
    expect(screen.getByText('VAB')).toBeInTheDocument();
  });

  it('does not show VAB chip when isVAB is false', () => {
    const date = new Date(2026, 1, 9);
    renderWithTheme(<DayCell date={date} dayData={baseDayData} />);
    expect(screen.queryByText('VAB')).not.toBeInTheDocument();
  });

  it('shows comment indicator when parentA comments exist', () => {
    const date = new Date(2026, 1, 9);
    const dayData: DayAssignmentDto = {
      ...baseDayData,
      parent: 'A',
      parentAComments: [
        {
          id: 1,
          dayAssignmentId: 1,
          parent: 'A',
          commentText: 'test comment',
          createdAt: '2026-01-01T00:00:00Z',
          modifiedAt: null,
        },
      ],
    };
    renderWithTheme(<DayCell date={date} dayData={dayData} />);
    expect(screen.getByTestId('comment-indicator')).toBeInTheDocument();
  });

  it('shows comment indicator when parentB comments exist', () => {
    const date = new Date(2026, 1, 9);
    const dayData: DayAssignmentDto = {
      ...baseDayData,
      parent: 'B',
      parentBComments: [
        {
          id: 2,
          dayAssignmentId: 1,
          parent: 'B',
          commentText: 'another comment',
          createdAt: '2026-01-01T00:00:00Z',
          modifiedAt: null,
        },
      ],
    };
    renderWithTheme(<DayCell date={date} dayData={dayData} />);
    expect(screen.getByTestId('comment-indicator')).toBeInTheDocument();
  });

  it('does not show comment indicator when no comments', () => {
    const date = new Date(2026, 1, 9);
    renderWithTheme(<DayCell date={date} dayData={baseDayData} />);
    expect(screen.queryByTestId('comment-indicator')).not.toBeInTheDocument();
  });

  it('calls onClick with date when clicked', () => {
    const date = new Date(2026, 1, 9);
    const onClick = vi.fn();
    renderWithTheme(<DayCell date={date} onClick={onClick} />);
    fireEvent.click(screen.getByTestId('day-cell-2026-02-09'));
    expect(onClick).toHaveBeenCalledWith(date);
  });

  it('does not throw when clicked without onClick handler', () => {
    const date = new Date(2026, 1, 9);
    renderWithTheme(<DayCell date={date} />);
    expect(() => {
      fireEvent.click(screen.getByTestId('day-cell-2026-02-09'));
    }).not.toThrow();
  });
});
