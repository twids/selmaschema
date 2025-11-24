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
  onFillMonth: (parent: 'parentA' | 'parentB', monthIndex: number, year: number) => void;
  onAlternateMonth: (monthIndex: number, year: number) => void;
  onInitializeMonth: () => void;
}

const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
const MILLISECONDS_PER_WEEK = 7 * 24 * 60 * 60 * 1000;

function getDaysInMonth(year: number, month: number): number {
  return new Date(year, month + 1, 0).getDate();
}

function getFirstDayOfMonth(year: number, month: number): number {
  return new Date(year, month, 1).getDay();
}

function getDateKey(year: number, month: number, day: number): string {
  return `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
}

// Calculate ISO 8601 week number
// Week 1 is the week with the first Thursday of the year
// Weeks start on Monday
function getWeekNumber(date: Date): number {
  // Copy date to avoid mutation
  const target = new Date(date.valueOf());
  
  // Set to nearest Thursday (current date + 4 - current day number)
  // Make Sunday's day number 7
  const dayNum = target.getDay() || 7;
  target.setDate(target.getDate() + 4 - dayNum);
  
  // Get first day of year
  const yearStart = new Date(target.getFullYear(), 0, 1);
  
  // Calculate full weeks to nearest Thursday
  const weekNo = Math.ceil((((target.getTime() - yearStart.getTime()) / MILLISECONDS_PER_WEEK) + 1));
  
  return weekNo;
}

// Calculate the day number for Monday of a given week in the calendar display
// firstDay: 0-6 (Sunday=0, Monday=1, ..., Saturday=6)
// weekIndex: 0-based index of the week row in the calendar
function getMondayDayNumber(firstDay: number, weekIndex: number): number {
  const daysFromStart = weekIndex * 7;
  // Convert firstDay to days before Monday (0=6, 1=0, 2=1, ..., 6=5)
  const daysBeforeMonday = firstDay === 0 ? 6 : firstDay - 1;
  return 1 - daysBeforeMonday + daysFromStart;
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
  onInitializeMonth,
}: MonthProps) {
  const daysInMonth = getDaysInMonth(currentYear, monthIndex);
  const firstDay = getFirstDayOfMonth(currentYear, monthIndex);

  // Build calendar with week numbers
  const calendarElements = [];
  
  // Header row: week number header + day names
  calendarElements.push(
    <div key="week-header" className="week-header">Week</div>
  );
  for (const dayName of dayNames) {
    calendarElements.push(
      <div key={dayName} className="day-header">
        {dayName}
      </div>
    );
  }
  
  // Build rows week by week
  let currentDay = 1;
  let weekIndex = 0;
  
  // Use do-while to ensure at least one week row is always rendered
  do {
    // Calculate week number based on the Monday of this week row
    // Find the Monday date for this week (may be in the previous month)
    const mondayDayNumber = getMondayDayNumber(firstDay, weekIndex);
    const mondayDate = new Date(currentYear, monthIndex, mondayDayNumber);
    
    const weekNum = getWeekNumber(mondayDate);
    
    // Add week number cell
    calendarElements.push(
      <div key={`week-${weekIndex}`} className="week-number">
        {weekNum}
      </div>
    );
    
    // Add day cells for this week (7 days)
    for (let i = 0; i < 7; i++) {
      const cellIndex = weekIndex * 7 + i;
      
      if (cellIndex < firstDay || currentDay > daysInMonth) {
        // Empty cell
        calendarElements.push(
          <div key={`empty-${weekIndex}-${i}`} className="day-cell empty-day"></div>
        );
      } else {
        // Regular day cell
        const dateKey = getDateKey(currentYear, monthIndex, currentDay);
        const dayData = calendarData[dateKey];
        
        calendarElements.push(
          <DayCell
            key={dateKey}
            day={currentDay}
            dateKey={dateKey}
            dayData={dayData}
            parentNames={parentNames}
            onClick={() => onDayClick(dateKey)}
          />
        );
        currentDay++;
      }
    }
    
    weekIndex++;
  } while (currentDay <= daysInMonth);

  return (
    <div className="month-section">
      <div className="month-header">
        <h2>
          {monthName} {currentYear}
        </h2>
        <div className="month-actions">
          <button onClick={() => onFillMonth('parentA', monthIndex, currentYear)}>
            Fill {parentNames.parentA}
          </button>
          <button onClick={() => onFillMonth('parentB', monthIndex, currentYear)}>
            Fill {parentNames.parentB}
          </button>
          <button onClick={() => onAlternateMonth(monthIndex, currentYear)}>Alternate Days</button>
          <button onClick={() => onInitializeMonth()} className="init-button">Initialize with Defaults</button>
        </div>
      </div>
      <div className="calendar-grid">
        {calendarElements}
      </div>
    </div>
  );
}
