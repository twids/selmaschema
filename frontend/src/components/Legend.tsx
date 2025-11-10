import { ParentNames } from '../types';
import './Legend.css';

interface LegendProps {
  parentNames: ParentNames;
}

export default function Legend({ parentNames }: LegendProps) {
  return (
    <div className="legend">
      <div className="legend-item">
        <div className="legend-color parent-a"></div>
        <span>{parentNames.parentA}</span>
      </div>
      <div className="legend-item">
        <div className="legend-color parent-b"></div>
        <span>{parentNames.parentB}</span>
      </div>
      <div className="legend-item">
        <div className="legend-color unassigned"></div>
        <span>Unassigned</span>
      </div>
      <div className="legend-item">
        <div className="legend-color vab-day"></div>
        <span>VAB (Child Care Leave)</span>
      </div>
    </div>
  );
}
