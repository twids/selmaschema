import { useRef } from 'react';
import { ParentNames } from '../types';
import './Controls.css';

interface ControlsProps {
  currentYear: number;
  onYearChange: (year: number) => void;
  parentNames: ParentNames;
  onParentNamesChange: (names: ParentNames) => void;
  onExport: () => void;
  onImport: (file: File) => void;
}

export default function Controls({
  currentYear,
  onYearChange,
  parentNames,
  onParentNamesChange,
  onExport,
  onImport,
}: ControlsProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const parentARef = useRef<HTMLInputElement>(null);
  const parentBRef = useRef<HTMLInputElement>(null);

  const handleUpdateNames = () => {
    onParentNamesChange({
      parentA: parentARef.current?.value || 'Parent A',
      parentB: parentBRef.current?.value || 'Parent B',
    });
  };

  const handleImportClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      onImport(file);
      // Reset file input
      e.target.value = '';
    }
  };

  const years = [];
  const currentYearActual = new Date().getFullYear();
  for (let year = currentYearActual - 2; year <= currentYearActual + 5; year++) {
    years.push(year);
  }

  return (
    <div className="controls">
      <label htmlFor="yearSelect">Year:</label>
      <select
        id="yearSelect"
        value={currentYear}
        onChange={(e) => onYearChange(parseInt(e.target.value))}
      >
        {years.map((year) => (
          <option key={year} value={year}>
            {year}
          </option>
        ))}
      </select>

      <label htmlFor="parentAName">Parent A:</label>
      <input
        ref={parentARef}
        type="text"
        id="parentAName"
        placeholder="Parent A name"
        defaultValue={parentNames.parentA}
      />

      <label htmlFor="parentBName">Parent B:</label>
      <input
        ref={parentBRef}
        type="text"
        id="parentBName"
        placeholder="Parent B name"
        defaultValue={parentNames.parentB}
      />

      <button onClick={handleUpdateNames}>Update Names</button>
      <button onClick={onExport}>Export Data</button>
      <button onClick={handleImportClick}>Import Data</button>
      <input
        ref={fileInputRef}
        type="file"
        accept=".json"
        style={{ display: 'none' }}
        onChange={handleFileChange}
      />
    </div>
  );
}
