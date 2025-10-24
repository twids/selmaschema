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
