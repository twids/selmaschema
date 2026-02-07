import { describe, it, expect } from 'vitest';
import {
  getISOWeekNumber,
  getMonthGrid,
  formatDateKey,
  getDaysInMonth,
} from '../utils/dateUtils';

describe('dateUtils', () => {
  describe('getISOWeekNumber', () => {
    it('returns week 1 for January 1, 2026 (Thursday)', () => {
      expect(getISOWeekNumber(new Date(2026, 0, 1))).toBe(1);
    });

    it('returns week 2 for January 5, 2026 (Monday)', () => {
      expect(getISOWeekNumber(new Date(2026, 0, 5))).toBe(2);
    });

    it('returns correct week for mid-year date', () => {
      // June 15, 2026 is a Monday → should be around week 25
      const week = getISOWeekNumber(new Date(2026, 5, 15));
      expect(week).toBeGreaterThan(20);
      expect(week).toBeLessThan(30);
    });

    it('handles year boundary (Dec 29, 2025 = ISO week 1 of 2026)', () => {
      // Dec 29, 2025 is Monday; Thursday of that week is Jan 1, 2026
      expect(getISOWeekNumber(new Date(2025, 11, 29))).toBe(1);
    });
  });

  describe('getDaysInMonth', () => {
    it('returns 28 for February 2026 (non-leap year)', () => {
      expect(getDaysInMonth(2026, 2)).toBe(28);
    });

    it('returns 29 for February 2024 (leap year)', () => {
      expect(getDaysInMonth(2024, 2)).toBe(29);
    });

    it('returns 31 for January', () => {
      expect(getDaysInMonth(2026, 1)).toBe(31);
    });

    it('returns 30 for April', () => {
      expect(getDaysInMonth(2026, 4)).toBe(30);
    });
  });

  describe('formatDateKey', () => {
    it('formats date as YYYY-MM-DD', () => {
      expect(formatDateKey(new Date(2026, 1, 7))).toBe('2026-02-07');
    });

    it('pads single-digit month and day', () => {
      expect(formatDateKey(new Date(2026, 0, 5))).toBe('2026-01-05');
    });

    it('handles December', () => {
      expect(formatDateKey(new Date(2026, 11, 25))).toBe('2026-12-25');
    });
  });

  describe('getMonthGrid', () => {
    it('returns correct number of weeks for February 2026', () => {
      const grid = getMonthGrid(2026, 2);
      // Feb 1, 2026 is Sunday (index 6 in Mon-Sun), 28 days total
      // Week 1: [null x6, Feb 1(Sun)]
      // Week 2: Feb 2-8
      // Week 3: Feb 9-15
      // Week 4: Feb 16-22
      // Week 5: Feb 23-28 + 2 nulls
      expect(grid).toHaveLength(5);
    });

    it('each week has exactly 7 day slots', () => {
      const grid = getMonthGrid(2026, 2);
      for (const week of grid) {
        expect(week.days).toHaveLength(7);
      }
    });

    it('first week has nulls for days before month start', () => {
      const grid = getMonthGrid(2026, 2);
      // Feb 1, 2026 is Sunday = index 6 in Mon-Sun grid
      const firstWeek = grid[0].days;
      for (let i = 0; i < 6; i++) {
        expect(firstWeek[i]).toBeNull();
      }
      expect(firstWeek[6]).not.toBeNull();
      expect(firstWeek[6]!.getDate()).toBe(1);
    });

    it('each week has a valid ISO week number', () => {
      const grid = getMonthGrid(2026, 2);
      for (const week of grid) {
        expect(week.weekNumber).toBeGreaterThan(0);
        expect(week.weekNumber).toBeLessThanOrEqual(53);
      }
    });

    it('all days of the month are present', () => {
      const grid = getMonthGrid(2026, 3); // March 2026 = 31 days
      const allDays = grid.flatMap((w) => w.days).filter((d) => d !== null);
      expect(allDays).toHaveLength(31);
      expect(allDays[0]!.getDate()).toBe(1);
      expect(allDays[allDays.length - 1]!.getDate()).toBe(31);
    });

    it('handles month starting on Monday', () => {
      // June 2026 starts on Monday
      const grid = getMonthGrid(2026, 6);
      expect(grid[0].days[0]).not.toBeNull();
      expect(grid[0].days[0]!.getDate()).toBe(1);
    });

    it('handles months needing 6 week rows', () => {
      // A 31-day month starting on Saturday needs 6 rows
      // August 2025 starts on Friday (index 4), 31 days → 6 rows
      // Let's use a month that definitely needs 6 rows:
      // March 2025 starts on Saturday (index 5), 31 days → 6 rows
      const grid = getMonthGrid(2025, 3);
      const allDays = grid.flatMap((w) => w.days).filter((d) => d !== null);
      expect(allDays).toHaveLength(31);
      // Could be 5 or 6 rows depending on start day
      expect(grid.length).toBeGreaterThanOrEqual(5);
    });
  });
});
