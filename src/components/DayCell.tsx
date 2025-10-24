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

  const truncateComment = (comment: string) => {
    if (!comment) return '';
    return comment.length > 30 ? comment.substring(0, 30) + '...' : comment;
  };

  return (
    <div className={getClassName()} onClick={onClick}>
      <div className="day-number">{day}</div>
      <div className="day-parent">{getParentName()}</div>
      {dayData?.comment && (
        <div className="day-comment">{truncateComment(dayData.comment)}</div>
      )}
      {dayData?.isVAB && <div className="vab-badge">VAB</div>}
    </div>
  );
}
