export interface DayData {
  parent: 'parentA' | 'parentB' | '';
  isVAB: boolean;
  comment: string;
}

export interface CalendarData {
  [dateKey: string]: DayData;
}

export interface ParentNames {
  parentA: string;
  parentB: string;
}

export interface AppData {
  calendarData: CalendarData;
  parentNames: ParentNames;
}

export interface UserInfo {
  id: number;
  email: string;
  name: string;
  isDemo: boolean;
  children?: { id: number; name: string }[];
}

export interface Invitation {
  id: number;
  childId: number;
  childName: string;
  inviterName: string;
  createdAt: string;
}

