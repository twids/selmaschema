export interface Comment {
  id: number;
  dayAssignmentId: number;
  parent: string;
  commentText: string;
  createdAt: string;
  modifiedAt: string | null;
}

export interface DayData {
  id?: number;
  parent: 'parentA' | 'parentB' | '';
  isVAB: boolean;
  specialStatus: string | null;
  parentAComments: Comment[];
  parentBComments: Comment[];
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
