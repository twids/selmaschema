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
  const target = new Date(date.valueOf());
  const dayNum = (date.getDay() + 6) % 7; // Convert to Monday = 0
  target.setDate(target.getDate() - dayNum + 3); // Thursday of this week
  const firstThursday = new Date(target.getFullYear(), 0, 4);
  const diff = target.getTime() - firstThursday.getTime();
  return 1 + Math.round(diff / 604800000); // 604800000 = 7 * 24 * 60 * 60 * 1000
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
  
  while (currentDay <= daysInMonth || weekIndex === 0) {
    // Calculate week number based on the Monday of this week
    // Find what date is the Monday of this week row
    let mondayDate: Date;
    if (weekIndex === 0 && firstDay !== 1) {
      // First week - find the Monday
      if (firstDay === 0) {
        // Sunday start, Monday is day 2
        mondayDate = new Date(currentYear, monthIndex, 2);
      } else {
        // Monday is before the 1st, or after
        mondayDate = new Date(currentYear, monthIndex, 1 - firstDay + 1);
      }
    } else {
      // For other weeks, calculate based on current position
      const daysFromStart = weekIndex * 7;
      const targetDay = 1 - firstDay + 1 + daysFromStart;
      mondayDate = new Date(currentYear, monthIndex, targetDay);
    }
    
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
    
    // Stop if we've rendered all days and filled the last week
    if (currentDay > daysInMonth && weekIndex > 0) {
      break;
    }
  }

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
