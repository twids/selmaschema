import { CalendarData, ParentNames } from '../types';
import DayCell from './DayCell';
import './Month.css';

interface MonthProps {
  monthName: string;
  monthIndex: number;
  currentYear: number;
  calendarData: CalendarData;
  parentNames: ParentNames;
  onDayClick: (dateKey: string) => void;
  onFillMonth: (month: number, parent: 'parentA' | 'parentB') => void;
  onAlternateMonth: (month: number) => void;
}

const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

function getDaysInMonth(year: number, month: number): number {
  return new Date(year, month + 1, 0).getDate();
}

function getFirstDayOfMonth(year: number, month: number): number {
  return new Date(year, month, 1).getDay();
}

function getDateKey(year: number, month: number, day: number): string {
  return `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
}

export default function Month({
  monthName,
  monthIndex,
  currentYear,
  calendarData,
  parentNames,
  onDayClick,
  onFillMonth,
  onAlternateMonth,
}: MonthProps) {
  const daysInMonth = getDaysInMonth(currentYear, monthIndex);
  const firstDay = getFirstDayOfMonth(currentYear, monthIndex);

  const days = [];

  // Add empty cells for days before month starts
  for (let i = 0; i < firstDay; i++) {
    days.push(<div key={`empty-${i}`} className="day-cell empty-day"></div>);
  }

  // Add day cells
  for (let day = 1; day <= daysInMonth; day++) {
    const dateKey = getDateKey(currentYear, monthIndex, day);
    const dayData = calendarData[dateKey];

    days.push(
      <DayCell
        key={dateKey}
        day={day}
        dateKey={dateKey}
        dayData={dayData}
        parentNames={parentNames}
        onClick={() => onDayClick(dateKey)}
      />
    );
  }

  return (
    <div className="month-section">
      <div className="month-header">
        <h2>
          {monthName} {currentYear}
        </h2>
        <div className="month-actions">
          <button onClick={() => onFillMonth(monthIndex, 'parentA')}>
            Fill {parentNames.parentA}
          </button>
          <button onClick={() => onFillMonth(monthIndex, 'parentB')}>
            Fill {parentNames.parentB}
          </button>
          <button onClick={() => onAlternateMonth(monthIndex)}>Alternate Days</button>
        </div>
      </div>
      <div className="calendar-grid">
        {dayNames.map((dayName) => (
          <div key={dayName} className="day-header">
            {dayName}
          </div>
        ))}
        {days}
      </div>
    </div>
  );
}
