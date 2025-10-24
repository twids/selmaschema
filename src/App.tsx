import { useState, useEffect } from 'react';
import './App.css';
import { CalendarData, ParentNames, DayData } from './types';
import Header from './components/Header';
import Controls from './components/Controls';
import Legend from './components/Legend';
import Calendar from './components/Calendar';
import Statistics from './components/Statistics';
import DayModal from './components/DayModal';

function App() {
  const [currentYear, setCurrentYear] = useState(new Date().getFullYear());
  const [calendarData, setCalendarData] = useState<CalendarData>({});
  const [parentNames, setParentNames] = useState<ParentNames>({
    parentA: 'Parent A',
    parentB: 'Parent B',
  });
  const [editingDate, setEditingDate] = useState<string | null>(null);

  // Load data from localStorage on mount
  useEffect(() => {
    const savedData = localStorage.getItem('coParentingCalendar');
    if (savedData) {
      try {
        const data = JSON.parse(savedData);
        if (data.calendarData) setCalendarData(data.calendarData);
        if (data.parentNames) setParentNames(data.parentNames);
      } catch (error) {
        console.error('Error loading data:', error);
      }
    }
  }, []);

  // Save data to localStorage whenever it changes
  useEffect(() => {
    const data = {
      calendarData,
      parentNames,
    };
    localStorage.setItem('coParentingCalendar', JSON.stringify(data));
  }, [calendarData, parentNames]);

  const updateParentNames = (newNames: ParentNames) => {
    setParentNames(newNames);
  };

  const updateDay = (dateKey: string, dayData: DayData) => {
    if (!dayData.parent && !dayData.isVAB && !dayData.comment) {
      // Remove empty entries
      const newData = { ...calendarData };
      delete newData[dateKey];
      setCalendarData(newData);
    } else {
      setCalendarData({
        ...calendarData,
        [dateKey]: dayData,
      });
    }
  };

  const fillMonth = (month: number, parent: 'parentA' | 'parentB') => {
    const daysInMonth = new Date(currentYear, month + 1, 0).getDate();
    const newData = { ...calendarData };

    for (let day = 1; day <= daysInMonth; day++) {
      const dateKey = getDateKey(currentYear, month, day);
      const existing = calendarData[dateKey] || { parent: '', isVAB: false, comment: '' };
      newData[dateKey] = {
        ...existing,
        parent,
      };
    }

    setCalendarData(newData);
  };

  const alternateMonth = (month: number) => {
    const daysInMonth = new Date(currentYear, month + 1, 0).getDate();
    const newData = { ...calendarData };

    for (let day = 1; day <= daysInMonth; day++) {
      const dateKey = getDateKey(currentYear, month, day);
      const existing = calendarData[dateKey] || { parent: '', isVAB: false, comment: '' };
      const parent = day % 2 === 1 ? 'parentA' : 'parentB';
      newData[dateKey] = {
        ...existing,
        parent,
      };
    }

    setCalendarData(newData);
  };

  const exportData = () => {
    const data = {
      calendarData,
      parentNames,
      exportDate: new Date().toISOString(),
    };

    const dataStr = JSON.stringify(data, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);

    const link = document.createElement('a');
    link.href = url;
    link.download = `coparenting-calendar-${currentYear}.json`;
    link.click();

    URL.revokeObjectURL(url);
  };

  const importData = (file: File) => {
    const reader = new FileReader();
    reader.onload = (event) => {
      try {
        const data = JSON.parse(event.target?.result as string);
        if (data.calendarData) setCalendarData(data.calendarData);
        if (data.parentNames) setParentNames(data.parentNames);
        alert('Data imported successfully!');
      } catch (error) {
        alert('Error importing data: ' + (error as Error).message);
      }
    };
    reader.readAsText(file);
  };

  return (
    <div className="container">
      <Header />
      <Controls
        currentYear={currentYear}
        onYearChange={setCurrentYear}
        parentNames={parentNames}
        onParentNamesChange={updateParentNames}
        onExport={exportData}
        onImport={importData}
      />
      <Legend parentNames={parentNames} />
      <Calendar
        currentYear={currentYear}
        calendarData={calendarData}
        parentNames={parentNames}
        onDayClick={setEditingDate}
        onFillMonth={fillMonth}
        onAlternateMonth={alternateMonth}
      />
      <Statistics
        currentYear={currentYear}
        calendarData={calendarData}
        parentNames={parentNames}
      />
      {editingDate && (
        <DayModal
          dateKey={editingDate}
          currentYear={currentYear}
          dayData={calendarData[editingDate]}
          parentNames={parentNames}
          onSave={(dayData) => {
            updateDay(editingDate, dayData);
            setEditingDate(null);
          }}
          onClose={() => setEditingDate(null)}
        />
      )}
    </div>
  );
}

function getDateKey(year: number, month: number, day: number): string {
  return `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
}

export default App;
