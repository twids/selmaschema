import { useState, useEffect } from 'react';
import { DayData, ParentNames } from '../types';
import { sv, formatSwedishDate, formatSwedishTime } from '../i18n/sv';
import './DayModal.css';
import './DayModal.new.css';

interface DayModalProps {
  dateKey: string;
  currentYear: number;
  dayData?: DayData;
  parentNames: ParentNames;
  onSave: (dayData: DayData) => void;
  onAddComment: (parent: 'parentA' | 'parentB', commentText: string) => void;
  onClose: () => void;
}

export default function DayModal({
  dateKey,
  dayData,
  parentNames,
  onSave,
  onAddComment,
  onClose,
}: DayModalProps) {
  const [parent, setParent] = useState<'' | 'parentA' | 'parentB'>(
    dayData?.parent || ''
  );
  const [isVAB, setIsVAB] = useState(dayData?.isVAB || false);
  const [specialStatus, setSpecialStatus] = useState<string | null>(
    dayData?.specialStatus || null
  );

  const [parentACommentText, setParentACommentText] = useState('');
  const [parentBCommentText, setParentBCommentText] = useState('');

  useEffect(() => {
    document.body.style.overflow = 'hidden';
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, []);

  const handleSave = () => {
    onSave({
      id: dayData?.id,
      parent,
      isVAB,
      specialStatus,
      parentAComments: dayData?.parentAComments || [],
      parentBComments: dayData?.parentBComments || [],
    });
  };

  const handleAddParentAComment = () => {
    if (parentACommentText.trim()) {
      onAddComment('parentA', parentACommentText);
      setParentACommentText('');
    }
  };

  const handleAddParentBComment = () => {
    if (parentBCommentText.trim()) {
      onAddComment('parentB', parentBCommentText);
      setParentBCommentText('');
    }
  };

  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <div className="modal-backdrop" onClick={handleBackdropClick}>
      <div className="modal-content modal-content-wide">
        <h3>{formatSwedishDate(new Date(dateKey + 'T00:00:00'))}</h3>
        <div className="modal-body">
          {/* Assignment Controls */}
          <div className="assignment-controls">
            <div className="form-group">
              <label htmlFor="parentSelect">{sv.dayModal.assignedTo}</label>
              <select
                id="parentSelect"
                value={parent}
                onChange={(e) =>
                  setParent(e.target.value as '' | 'parentA' | 'parentB')
                }
              >
                <option value="">{sv.dayModal.unassigned}</option>
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
                {sv.dayModal.markAsVAB}
              </label>
            </div>

            <div className="form-group">
              <label htmlFor="specialStatusSelect">
                {sv.dayModal.specialStatus}
              </label>
              <select
                id="specialStatusSelect"
                value={specialStatus || ''}
                onChange={(e) =>
                  setSpecialStatus(e.target.value || null)
                }
              >
                <option value="">{sv.specialStatus.normal}</option>
                <option value="PreschoolClosed">
                  {sv.specialStatus.preschoolClosed}
                </option>
                <option value="Holiday">{sv.specialStatus.holiday}</option>
              </select>
            </div>
          </div>

          {/* Comments Sections */}
          <div className="comments-container">
            {/* Tomas Comments */}
            <div className="comments-section comments-section-a">
              <h4>{sv.comments.commentTomas}</h4>
              <div className="comments-list">
                {dayData?.parentAComments.length === 0 ? (
                  <p className="no-comments">{sv.comments.noComments}</p>
                ) : (
                  dayData?.parentAComments.map((comment) => (
                    <div key={comment.id} className="comment">
                      <span className="comment-timestamp">
                        {formatSwedishTime(comment.createdAt)}
                      </span>
                      <p className="comment-text">{comment.commentText}</p>
                    </div>
                  ))
                )}
              </div>
              <div className="comment-input-group">
                <textarea
                  value={parentACommentText}
                  onChange={(e) => setParentACommentText(e.target.value)}
                  placeholder={sv.comments.commentPlaceholder}
                  rows={2}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' && e.ctrlKey) {
                      handleAddParentAComment();
                    }
                  }}
                />
                <button
                  className="add-comment-btn"
                  onClick={handleAddParentAComment}
                  disabled={!parentACommentText.trim()}
                >
                  {sv.comments.addComment}
                </button>
              </div>
            </div>

            {/* Caroline Comments */}
            <div className="comments-section comments-section-b">
              <h4>{sv.comments.commentCaroline}</h4>
              <div className="comments-list">
                {dayData?.parentBComments.length === 0 ? (
                  <p className="no-comments">{sv.comments.noComments}</p>
                ) : (
                  dayData?.parentBComments.map((comment) => (
                    <div key={comment.id} className="comment">
                      <span className="comment-timestamp">
                        {formatSwedishTime(comment.createdAt)}
                      </span>
                      <p className="comment-text">{comment.commentText}</p>
                    </div>
                  ))
                )}
              </div>
              <div className="comment-input-group">
                <textarea
                  value={parentBCommentText}
                  onChange={(e) => setParentBCommentText(e.target.value)}
                  placeholder={sv.comments.commentPlaceholder}
                  rows={2}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' && e.ctrlKey) {
                      handleAddParentBComment();
                    }
                  }}
                />
                <button
                  className="add-comment-btn"
                  onClick={handleAddParentBComment}
                  disabled={!parentBCommentText.trim()}
                >
                  {sv.comments.addComment}
                </button>
              </div>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="modal-actions">
            <button className="save-button" onClick={handleSave}>
              {sv.actions.save}
            </button>
            <button className="cancel-button" onClick={onClose}>
              {sv.actions.cancel}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
