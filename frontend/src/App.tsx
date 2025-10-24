import { useState, useEffect, useCallback } from 'react';
import './App.css';
import { CalendarData, ParentNames, DayData } from './types';
import Header from './components/Header';
import Controls from './components/Controls';
import Legend from './components/Legend';
import Month from './components/Month';
import Statistics from './components/Statistics';
import DayModal from './components/DayModal';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:8080';

interface DayAssignmentDto {
  id: number;
  date: string;
  parent: string | null;
  isVAB: boolean;
  comment: string | null;
}

interface MonthDataDto {
  year: number;
  month: number;
  days: DayAssignmentDto[];
}

function App() {
  const now = new Date();
  const [currentYear, setCurrentYear] = useState(now.getFullYear());
  const [currentMonth, setCurrentMonth] = useState(now.getMonth()); // 0-11
  const [calendarData, setCalendarData] = useState<CalendarData>({});
  const [parentNames, setParentNames] = useState<ParentNames>({
    parentA: 'Parent A',
    parentB: 'Parent B',
  });
  const [editingDate, setEditingDate] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const loadParentNames = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/config/parent-names`);
      if (response.ok) {
        const data = await response.json();
        setParentNames({
          parentA: data.parentAName,
          parentB: data.parentBName,
        });
      }
    } catch (error) {
      console.error('Error loading parent names:', error);
    }
  };

  const loadMonthData = useCallback(async () => {
    setLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/api/days/${currentYear}/${currentMonth + 1}`);
      if (response.ok) {
        const data: MonthDataDto = await response.json();
        const monthData: CalendarData = {};
        data.days.forEach((day) => {
          const dateKey = new Date(day.date).toISOString().split('T')[0];
          let parent: '' | 'parentA' | 'parentB' = '';
          if (day.parent === 'A') parent = 'parentA';
          else if (day.parent === 'B') parent = 'parentB';
          
          monthData[dateKey] = {
            parent,
            isVAB: day.isVAB,
            comment: day.comment || '',
          };
        });
        setCalendarData(monthData);
      }
    } catch (error) {
      console.error('Error loading month data:', error);
    } finally {
      setLoading(false);
    }
  }, [currentYear, currentMonth]);

  // Load parent names from API on mount
  useEffect(() => {
    loadParentNames();
  }, []);

  // Load month data when year or month changes
  useEffect(() => {
    loadMonthData();
  }, [currentYear, currentMonth, loadMonthData]);

  const updateParentNames = async (newNames: ParentNames) => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/config/parent-names`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          parentAName: newNames.parentA,
          parentBName: newNames.parentB,
        }),
      });
      if (response.ok) {
        setParentNames(newNames);
      }
    } catch (error) {
      console.error('Error updating parent names:', error);
    }
  };

  const updateDay = async (dateKey: string, dayData: DayData) => {
    try {
      const date = new Date(dateKey);
      // Map parentA/parentB to A/B for API
      let apiParent: string | null = null;
      if (dayData.parent === 'parentA') apiParent = 'A';
      else if (dayData.parent === 'parentB') apiParent = 'B';
      
      const response = await fetch(
        `${API_BASE_URL}/api/days/${date.getFullYear()}/${date.getMonth() + 1}/${date.getDate()}`,
        {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            parent: apiParent,
            isVAB: dayData.isVAB,
            comment: dayData.comment || null,
          }),
        }
      );
      
      if (response.ok) {
        setCalendarData({
          ...calendarData,
          [dateKey]: dayData,
        });
      }
    } catch (error) {
      console.error('Error updating day:', error);
    }
  };

  const initializeMonth = async () => {
    setLoading(true);
    try {
      const response = await fetch(
        `${API_BASE_URL}/api/days/${currentYear}/${currentMonth + 1}/initialize`,
        { method: 'POST' }
      );
      if (response.ok) {
        await loadMonthData();
      }
    } catch (error) {
      console.error('Error initializing month:', error);
    } finally {
      setLoading(false);
    }
  };

  const fillMonth = async (parent: 'parentA' | 'parentB') => {
    const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();
    
    for (let day = 1; day <= daysInMonth; day++) {
      const dateKey = getDateKey(currentYear, currentMonth, day);
      const existing = calendarData[dateKey] || { parent: '', isVAB: false, comment: '' };
      await updateDay(dateKey, {
        ...existing,
        parent,
      });
    }
    
    await loadMonthData();
  };

  const alternateMonth = async () => {
    const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();
    
    for (let day = 1; day <= daysInMonth; day++) {
      const dateKey = getDateKey(currentYear, currentMonth, day);
      const existing = calendarData[dateKey] || { parent: '', isVAB: false, comment: '' };
      const parent = day % 2 === 1 ? 'parentA' : 'parentB';
      await updateDay(dateKey, {
        ...existing,
        parent,
      });
    }
    
    await loadMonthData();
  };

  const changeMonth = (delta: number) => {
    let newMonth = currentMonth + delta;
    let newYear = currentYear;
    
    if (newMonth < 0) {
      newMonth = 11;
      newYear--;
    } else if (newMonth > 11) {
      newMonth = 0;
      newYear++;
    }
    
    setCurrentMonth(newMonth);
    setCurrentYear(newYear);
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
    link.download = `coparenting-calendar-${currentYear}-${currentMonth + 1}.json`;
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
        currentMonth={currentMonth}
        onYearChange={setCurrentYear}
        onMonthChange={setCurrentMonth}
        onNavigateMonth={changeMonth}
        parentNames={parentNames}
        onParentNamesChange={updateParentNames}
        onExport={exportData}
        onImport={importData}
      />
      <Legend parentNames={parentNames} />
      <div className="calendar-container">
        {loading ? (
          <div className="loading">Loading...</div>
        ) : (
          <Month
            monthName={getMonthName(currentMonth)}
            monthIndex={currentMonth}
            currentYear={currentYear}
            calendarData={calendarData}
            parentNames={parentNames}
            onDayClick={setEditingDate}
            onFillMonth={fillMonth}
            onAlternateMonth={alternateMonth}
            onInitializeMonth={initializeMonth}
          />
        )}
      </div>
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

function getMonthName(monthIndex: number): string {
  const names = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];
  return names[monthIndex];
}

export default App;
