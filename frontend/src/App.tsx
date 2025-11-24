import { useState, useEffect, useCallback } from 'react';
import { ThemeProvider, createTheme, CssBaseline, Box, Container } from '@mui/material';
import { CalendarData, ParentNames, DayData, UserInfo } from './types';
import Header from './components/Header';
import Controls from './components/Controls';
import Legend from './components/Legend';
import Month from './components/Month';
import Statistics from './components/Statistics';
import DayModal from './components/DayModal';
import LoginScreen from './components/LoginScreen';
import InvitationsDialog from './components/InvitationsDialog';

const API_BASE_URL = import.meta.env.VITE_API_URL || '';

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

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

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
  const [user, setUser] = useState<UserInfo | null>(null);
  const [checkingAuth, setCheckingAuth] = useState(true);
  const [showInvitations, setShowInvitations] = useState(false);

  const checkAuth = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/user`, {
        credentials: 'include',
      });
      if (response.ok) {
        const userData = await response.json();
        setUser(userData);
      } else {
        setUser(null);
      }
    } catch (error) {
      console.error('Error checking auth:', error);
      setUser(null);
    } finally {
      setCheckingAuth(false);
    }
  };

  useEffect(() => {
    checkAuth();
  }, []);

  const handleLogin = async (isDemo: boolean) => {
    if (isDemo) {
      try {
        const response = await fetch(`${API_BASE_URL}/api/auth/demo`, {
          method: 'POST',
          credentials: 'include',
        });
        if (response.ok) {
          await checkAuth();
        }
      } catch (error) {
        console.error('Error during demo login:', error);
      }
    } else {
      window.location.href = `${API_BASE_URL}/api/auth/login`;
    }
  };

  const handleLogout = async () => {
    try {
      await fetch(`${API_BASE_URL}/api/auth/logout`, {
        method: 'POST',
        credentials: 'include',
      });
      setUser(null);
    } catch (error) {
      console.error('Error during logout:', error);
    }
  };

  const loadParentNames = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/config/parent-names`, {
        credentials: 'include',
      });
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
    if (!user) return;
    
    setLoading(true);
    try {
      const response = await fetch(
        `${API_BASE_URL}/api/days/${currentYear}/${currentMonth + 1}`,
        { credentials: 'include' }
      );
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
  }, [currentYear, currentMonth, user]);

  useEffect(() => {
    if (user) {
      loadParentNames();
      loadMonthData();
    }
  }, [currentYear, currentMonth, user, loadMonthData]);

  const updateParentNames = async (newNames: ParentNames) => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/config/parent-names`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
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
      let apiParent: string | null = null;
      if (dayData.parent === 'parentA') apiParent = 'A';
      else if (dayData.parent === 'parentB') apiParent = 'B';
      
      const response = await fetch(
        `${API_BASE_URL}/api/days/${date.getFullYear()}/${date.getMonth() + 1}/${date.getDate()}`,
        {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          credentials: 'include',
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
        { 
          method: 'POST',
          credentials: 'include',
        }
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

  const fillMonth = async (
    parent: 'parentA' | 'parentB',
    month: number,
    year: number
  ) => {
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    
    for (let day = 1; day <= daysInMonth; day++) {
      const dateKey = getDateKey(year, month, day);
      const existing = calendarData[dateKey] || { parent: '', isVAB: false, comment: '' };
      await updateDay(dateKey, {
        ...existing,
        parent,
      });
    }
    
    await loadMonthData();
  };

  const alternateMonth = async (month: number, year: number) => {
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    
    for (let day = 1; day <= daysInMonth; day++) {
      const dateKey = getDateKey(year, month, day);
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

  if (checkingAuth) {
    return (
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
          Loading...
        </Box>
      </ThemeProvider>
    );
  }

  if (!user) {
    return (
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <LoginScreen onLogin={handleLogin} />
      </ThemeProvider>
    );
  }

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
        <Header user={user} onLogout={handleLogout} onShowInvitations={() => setShowInvitations(true)} />
        <Container maxWidth="xl" sx={{ py: 3 }}>
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
          <Box sx={{ mt: 3 }}>
            {loading ? (
              <Box display="flex" justifyContent="center" p={4}>
                Loading...
              </Box>
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
          </Box>
          <Statistics
            currentYear={currentYear}
            calendarData={calendarData}
            parentNames={parentNames}
          />
        </Container>
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
        <InvitationsDialog
          open={showInvitations}
          onClose={() => setShowInvitations(false)}
        />
      </Box>
    </ThemeProvider>
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
