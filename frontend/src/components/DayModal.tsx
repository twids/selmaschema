import { useState, useEffect } from 'react';
import { DayData, ParentNames } from '../types';
import './DayModal.css';

interface DayModalProps {
  dateKey: string;
  currentYear: number;
  dayData?: DayData;
  parentNames: ParentNames;
  onSave: (dayData: DayData) => void;
  onClose: () => void;
}

function formatDate(dateKey: string): string {
  const date = new Date(dateKey + 'T00:00:00');
  return date.toLocaleDateString('en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

export default function DayModal({
  dateKey,
  dayData,
  parentNames,
  onSave,
  onClose,
}: DayModalProps) {
  const [parent, setParent] = useState<'' | 'parentA' | 'parentB'>(
    dayData?.parent || ''
  );
  const [isVAB, setIsVAB] = useState(dayData?.isVAB || false);
  const [comment, setComment] = useState(dayData?.comment || '');

  useEffect(() => {
    // Prevent background scrolling when modal is open
    document.body.style.overflow = 'hidden';
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, []);

  const handleSave = () => {
    onSave({
      parent,
      isVAB,
      comment,
    });
  };

  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <div className="modal-backdrop" onClick={handleBackdropClick}>
      <div className="modal-content">
        <h3>{formatDate(dateKey)}</h3>
        <div className="modal-body">
          <div className="form-group">
            <label htmlFor="parentSelect">Assigned to:</label>
            <select
              id="parentSelect"
              value={parent}
              onChange={(e) => setParent(e.target.value as '' | 'parentA' | 'parentB')}
            >
              <option value="">Unassigned</option>
              <option value="parentA">{parentNames.parentA}</option>
              <option value="parentB">{parentNames.parentB}</option>
            </select>
          </div>

          <div className="form-group checkbox-group">
            <label>
              <input
                type="checkbox"
                checked={isVAB}
                onChange={(e) => setIsVAB(e.target.checked)}
              />
              Mark as VAB (Child Care Leave)
            </label>
          </div>

          <div className="form-group">
            <label htmlFor="commentInput">Comment:</label>
            <textarea
              id="commentInput"
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              placeholder="Add notes or comments for this day..."
              rows={4}
            />
          </div>

          <div className="modal-actions">
            <button className="save-button" onClick={handleSave}>
              Save
            </button>
            <button className="cancel-button" onClick={onClose}>
              Cancel
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
