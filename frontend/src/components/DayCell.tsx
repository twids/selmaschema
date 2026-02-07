import { DayData, ParentNames } from '../types';
import './DayCell.css';

interface DayCellProps {
  day: number;
  dateKey: string;
  dayData?: DayData;
  parentNames: ParentNames;
  onClick: () => void;
}

export default function DayCell({
  day,
  dayData,
  parentNames,
  onClick,
}: DayCellProps) {
  const getParentName = () => {
    if (!dayData || !dayData.parent) return 'Unassigned';
    return dayData.parent === 'parentA' ? parentNames.parentA : parentNames.parentB;
  };

  const getClassName = () => {
    const classes = ['day-cell'];
    if (!dayData || !dayData.parent) {
      classes.push('unassigned');
    } else if (dayData.parent === 'parentA') {
      classes.push('parent-a');
    } else if (dayData.parent === 'parentB') {
      classes.push('parent-b');
    }
    if (dayData?.isVAB) {
      classes.push('has-vab');
    }
    return classes.join(' ');
  };

  return (
    <div className={getClassName()} onClick={onClick}>
      <div className="day-number">{day}</div>
      <div className="day-parent">{getParentName()}</div>
      {(dayData && (dayData.parentAComments?.length ?? 0) > 0 || (dayData?.parentBComments?.length ?? 0) > 0) && (
        <div className="day-comment">
          {dayData?.parentAComments && dayData.parentAComments.length > 0 && <span>A: {dayData.parentAComments[0].commentText.substring(0, 15)}...</span>}
          {dayData?.parentBComments && dayData.parentBComments.length > 0 && <span>B: {dayData.parentBComments[0].commentText.substring(0, 15)}...</span>}
        </div>
      )}
      {dayData?.isVAB && <div className="vab-badge">VAB</div>}
    </div>
  );
}
