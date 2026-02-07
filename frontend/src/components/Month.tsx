import { CalendarData, ParentNames } from "../types";
import DayCell from "./DayCell";
import { sv } from "../i18n/sv";
import "./Month.css";

interface MonthProps {
  monthName: string;
  monthIndex: number;
  currentYear: number;
  calendarData: CalendarData;
  parentNames: ParentNames;
  onDayClick: (dateKey: string) => void;
  onInitializeMonth: () => void;
}

// Swedish calendar starts on Monday (Mån-Sön)
const dayNames = ["Mån", "Tis", "Ons", "Tor", "Fre", "Lör", "Sön"];

function getDaysInMonth(year: number, month: number): number {
  return new Date(year, month + 1, 0).getDate();
}

function getFirstDayOfMonth(year: number, month: number): number {
  // Adjust for Monday start (0 = Monday, 6 = Sunday)
  const day = new Date(year, month, 1).getDay();
  return day === 0 ? 6 : day - 1;
}

function getDateKey(year: number, month: number, day: number): string {
  return `${year}-${String(month + 1).padStart(2, "0")}-${String(day).padStart(
    2,
    "0"
  )}`;
}

// ISO 8601 week number calculation (Swedish standard)
function getWeekNumber(date: Date): number {
  const d = new Date(
    Date.UTC(date.getFullYear(), date.getMonth(), date.getDate())
  );
  const dayNum = d.getUTCDay() || 7;
  d.setUTCDate(d.getUTCDate() + 4 - dayNum);
  const yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1));
  return Math.ceil(((d.getTime() - yearStart.getTime()) / 86400000 + 1) / 7);
}

export default function Month({
  monthName,
  monthIndex,
  currentYear,
  calendarData,
  parentNames,
  onDayClick,
  onInitializeMonth,
}: MonthProps) {
  const daysInMonth = getDaysInMonth(currentYear, monthIndex);
  const firstDay = getFirstDayOfMonth(currentYear, monthIndex);

  // Build the calendar as rows of weeks
  const weeks: JSX.Element[][] = [];
  let currentWeek: JSX.Element[] = [];
  let weekNumber: number | null = null;
  let dayCounter = 1;

  // Add empty cells for days before month starts
  for (let i = 0; i < firstDay; i++) {
    currentWeek.push(
      <div key={`empty-${i}`} className="day-cell empty-day"></div>
    );
  }

  // Add day cells
  while (dayCounter <= daysInMonth) {
    const dateKey = getDateKey(currentYear, monthIndex, dayCounter);
    const dayData = calendarData[dateKey];
    const date = new Date(currentYear, monthIndex, dayCounter);

    // Get week number for the first day of the week or when starting
    if (currentWeek.length === 0) {
      weekNumber = getWeekNumber(date);
    }

    currentWeek.push(
      <DayCell
        key={dateKey}
        day={dayCounter}
        dateKey={dateKey}
        dayData={dayData}
        parentNames={parentNames}
        onClick={() => onDayClick(dateKey)}
      />
    );

    // When week is complete, add it to weeks array with week number
    if (currentWeek.length === 7) {
      weeks.push([
        <div key={`week-${weekNumber}`} className="week-number">
          v{weekNumber}
        </div>,
        ...currentWeek,
      ]);
      currentWeek = [];
    }

    dayCounter++;
  }

  // Add remaining days if the last week is incomplete
  if (currentWeek.length > 0) {
    // Fill the rest of the week with empty cells
    while (currentWeek.length < 7) {
      currentWeek.push(
        <div
          key={`empty-end-${currentWeek.length}`}
          className="day-cell empty-day"
        ></div>
      );
    }
    weekNumber = getWeekNumber(
      new Date(currentYear, monthIndex, dayCounter - 1)
    );
    weeks.push([
      <div key={`week-${weekNumber}`} className="week-number">
        v{weekNumber}
      </div>,
      ...currentWeek,
    ]);
  }

  return (
    <div className="month-section">
      <div className="month-header">
        <h2>
          {monthName} {currentYear}
        </h2>
        <div className="month-actions">
          <button onClick={() => onInitializeMonth()} className="init-button">
            Initialize with Defaults
          </button>
        </div>
      </div>
      <div className="calendar-grid-with-weeks">
        <div className="week-header">{sv.week}</div>
        {dayNames.map((dayName) => (
          <div key={dayName} className="day-header">
            {dayName}
          </div>
        ))}
        {weeks.flat()}
      </div>
    </div>
  );
}
