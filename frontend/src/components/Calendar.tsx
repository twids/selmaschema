import { CalendarData, ParentNames } from '../types';
import Month from './Month';
import './Calendar.css';

interface CalendarProps {
  currentYear: number;
  calendarData: CalendarData;
  parentNames: ParentNames;
  onDayClick: (dateKey: string) => void;
  onFillMonth: (month: number, parent: 'parentA' | 'parentB') => void;
  onAlternateMonth: (month: number) => void;
}

const monthNames = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
];

export default function Calendar({
  currentYear,
  calendarData,
  parentNames,
  onDayClick,
  onFillMonth,
  onAlternateMonth,
}: CalendarProps) {
  return (
    <div className="calendar-container">
      {monthNames.map((monthName, monthIndex) => (
        <Month
          key={monthIndex}
          monthName={monthName}
          monthIndex={monthIndex}
          currentYear={currentYear}
          calendarData={calendarData}
          parentNames={parentNames}
          onDayClick={onDayClick}
          onFillMonth={onFillMonth}
          onAlternateMonth={onAlternateMonth}
        />
      ))}
    </div>
  );
}
