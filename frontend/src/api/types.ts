/** DTOs matching the backend C# API contracts exactly. */

export interface CommentDto {
  id: number;
  dayAssignmentId: number;
  parent: string;
  commentText: string;
  createdAt: string;
  modifiedAt: string | null;
}

export interface DayAssignmentDto {
  id: number;
  date: string;
  parent: string | null;
  isVAB: boolean;
  specialStatus: string | null;
  parentAComments: CommentDto[];
  parentBComments: CommentDto[];
}

export interface MonthDataDto {
  year: number;
  month: number;
  days: DayAssignmentDto[];
}

export interface ParentNamesDto {
  parentAName: string;
  parentBName: string;
}

export interface StatisticsDto {
  parentADays: number;
  parentBDays: number;
  vabDays: number;
  unassignedDays: number;
  daysWithComments: number;
}

export interface CreateCommentDto {
  parent: string;
  commentText: string;
}

export interface UpdateDayAssignmentDto {
  parent: string | null;
  isVAB: boolean;
  specialStatus: string | null;
}

/** Map from date key (YYYY-MM-DD) to DayAssignmentDto. */
export type CalendarData = Record<string, DayAssignmentDto>;
