/**
 * Date utility functions for the Swedish calendar.
 *
 * Swedish weeks start on Monday (ISO 8601) and week numbering
 * follows the ISO standard where week 1 contains the first Thursday of the year.
 */

/** ISO 8601 week number (Swedish standard). */
export function getISOWeekNumber(date: Date): number {
  const target = new Date(date.getTime());
  target.setHours(0, 0, 0, 0);
  // Make Sunday = 7 so Monday = 1, Tuesday = 2, …, Sunday = 7
  const dayNum = target.getDay() || 7;
  // Set to the Thursday of the current ISO week
  target.setDate(target.getDate() + 4 - dayNum);
  const yearStart = new Date(target.getFullYear(), 0, 1);
  return Math.ceil(
    ((target.getTime() - yearStart.getTime()) / 86_400_000 + 1) / 7,
  );
}

/** Number of days in a given month (1-indexed). */
export function getDaysInMonth(year: number, month: number): number {
  return new Date(year, month, 0).getDate();
}

/** Format a Date as `YYYY-MM-DD` key string. */
export function formatDateKey(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

/**
 * Build a month grid for Swedish calendar (Mon–Sun weeks).
 *
 * @param year  Full year, e.g. 2026
 * @param month 1-indexed month (January = 1)
 * @returns Array of week rows. Each row has an ISO `weekNumber` and
 *          7 day slots (Mon = index 0 … Sun = index 6).
 *          Slots outside the current month are `null`.
 */
export function getMonthGrid(
  year: number,
  month: number,
): { weekNumber: number; days: (Date | null)[] }[] {
  const totalDays = getDaysInMonth(year, month);
  const weeks: { weekNumber: number; days: (Date | null)[] }[] = [];

  // Monday-based day-of-week index: Monday = 0, …, Sunday = 6
  const firstDate = new Date(year, month - 1, 1);
  const startDow = (firstDate.getDay() + 6) % 7;

  let currentDay = 1;

  while (currentDay <= totalDays) {
    const days: (Date | null)[] = Array.from({ length: 7 }, () => null);
    const start = currentDay === 1 ? startDow : 0;

    for (let i = start; i < 7 && currentDay <= totalDays; i++) {
      days[i] = new Date(year, month - 1, currentDay);
      currentDay++;
    }

    // Use first non-null date to derive the ISO week number
    const firstNonNull = days.find((d) => d !== null)!;
    const weekNumber = getISOWeekNumber(firstNonNull);

    weeks.push({ weekNumber, days });
  }

  return weeks;
}
