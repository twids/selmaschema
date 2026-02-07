import { CalendarData, ParentNames } from '../types';
import './Statistics.css';

interface StatisticsProps {
  currentYear: number;
  calendarData: CalendarData;
  parentNames: ParentNames;
}

export default function Statistics({
  currentYear,
  calendarData,
  parentNames,
}: StatisticsProps) {
  const calculateStats = () => {
    let parentADays = 0;
    let parentBDays = 0;
    let vabDays = 0;
    let commentedDays = 0;
    let unassignedDays = 0;

    // Calculate total days in the year
    const isLeapYear = (currentYear % 4 === 0 && currentYear % 100 !== 0) || currentYear % 400 === 0;
    const totalDays = isLeapYear ? 366 : 365;

    // Count assigned days
    Object.values(calendarData).forEach((day) => {
      if (day.parent === 'parentA') parentADays++;
      if (day.parent === 'parentB') parentBDays++;
      if (day.isVAB) vabDays++;
      if (day.parentAComments?.length > 0 || day.parentBComments?.length > 0) commentedDays++;
    });

    unassignedDays = totalDays - parentADays - parentBDays;

    return {
      parentADays,
      parentBDays,
      vabDays,
      commentedDays,
      unassignedDays,
    };
  };

  const stats = calculateStats();

  return (
    <div className="statistics">
      <div className="stat-item">
        <div className="stat-value">{stats.parentADays}</div>
        <div className="stat-label">{parentNames.parentA} Days</div>
      </div>
      <div className="stat-item">
        <div className="stat-value">{stats.parentBDays}</div>
        <div className="stat-label">{parentNames.parentB} Days</div>
      </div>
      <div className="stat-item">
        <div className="stat-value">{stats.vabDays}</div>
        <div className="stat-label">VAB Days</div>
      </div>
      <div className="stat-item">
        <div className="stat-value">{stats.commentedDays}</div>
        <div className="stat-label">Days with Comments</div>
      </div>
      <div className="stat-item">
        <div className="stat-value">{stats.unassignedDays}</div>
        <div className="stat-label">Unassigned Days</div>
      </div>
    </div>
  );
}
