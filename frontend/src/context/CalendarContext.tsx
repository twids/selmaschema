/* eslint-disable react-refresh/only-export-components */
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from "react";
import { useAuth } from "../auth/AuthContext";
import { apiGet, apiPost, apiPut } from "../api/client";
import type {
  CalendarData,
  MonthDataDto,
  UpdateDayAssignmentDto,
} from "../api/types";

/* ---------- context shape ---------- */

interface CalendarContextValue {
  currentYear: number;
  currentMonth: number;
  calendarData: CalendarData;
  loading: boolean;
  error: string | null;
  loadMonthData: (year: number, month: number) => Promise<void>;
  updateDay: (
    year: number,
    month: number,
    day: number,
    data: UpdateDayAssignmentDto,
  ) => Promise<void>;
  initializeMonth: (year: number, month: number) => Promise<void>;
  setMonth: (month: number) => void;
  setYear: (year: number) => void;
  navigateMonth: (direction: 1 | -1) => void;
}

const CalendarContext = createContext<CalendarContextValue | undefined>(
  undefined,
);

/* ---------- helpers ---------- */

/** Convert an ISO date string to a YYYY-MM-DD key. */
function toDateKey(isoDate: string): string {
  return isoDate.slice(0, 10);
}

/** Convert a MonthDataDto.days array into a keyed CalendarData map. */
function toDayMap(data: MonthDataDto): CalendarData {
  const map: CalendarData = {};
  for (const day of data.days) {
    map[toDateKey(day.date)] = day;
  }
  return map;
}

/* ---------- provider ---------- */

export function CalendarProvider({ children }: { children: React.ReactNode }) {
  const { authHeader } = useAuth();

  const now = new Date();
  const [currentYear, setCurrentYear] = useState(now.getFullYear());
  const [currentMonth, setCurrentMonth] = useState(now.getMonth() + 1);
  const [calendarData, setCalendarData] = useState<CalendarData>({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadMonthData = useCallback(
    async (year: number, month: number) => {
      setLoading(true);
      setError(null);
      try {
        const data = await apiGet<MonthDataDto>(
          `/api/days/${year}/${month}`,
          authHeader,
        );
        setCalendarData(toDayMap(data));
      } catch (err) {
        setError(err instanceof Error ? err.message : String(err));
      } finally {
        setLoading(false);
      }
    },
    [authHeader],
  );

  const updateDay = useCallback(
    async (
      year: number,
      month: number,
      day: number,
      data: UpdateDayAssignmentDto,
    ) => {
      await apiPut(`/api/days/${year}/${month}/${day}`, data, authHeader);
      await loadMonthData(year, month);
    },
    [authHeader, loadMonthData],
  );

  const initializeMonth = useCallback(
    async (year: number, month: number) => {
      await apiPost(`/api/days/${year}/${month}/initialize`, undefined, authHeader);
      await loadMonthData(year, month);
    },
    [authHeader, loadMonthData],
  );

  const setMonth = useCallback((month: number) => {
    setCurrentMonth(month);
  }, []);

  const setYear = useCallback((year: number) => {
    setCurrentYear(year);
  }, []);

  const navigateMonth = useCallback((direction: 1 | -1) => {
    setCurrentMonth((prev) => {
      const next = prev + direction;
      if (next > 12) {
        setCurrentYear((y) => y + 1);
        return 1;
      }
      if (next < 1) {
        setCurrentYear((y) => y - 1);
        return 12;
      }
      return next;
    });
  }, []);

  // Auto-load data when year/month changes
  useEffect(() => {
    void loadMonthData(currentYear, currentMonth);
  }, [currentYear, currentMonth, loadMonthData]);

  const value: CalendarContextValue = {
    currentYear,
    currentMonth,
    calendarData,
    loading,
    error,
    loadMonthData,
    updateDay,
    initializeMonth,
    setMonth,
    setYear,
    navigateMonth,
  };

  return (
    <CalendarContext.Provider value={value}>{children}</CalendarContext.Provider>
  );
}

/* ---------- hook ---------- */

export function useCalendar(): CalendarContextValue {
  const ctx = useContext(CalendarContext);
  if (!ctx) {
    throw new Error("useCalendar must be used within CalendarProvider");
  }
  return ctx;
}
