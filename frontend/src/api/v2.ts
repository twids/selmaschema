import { apiDelete, apiGet, apiPost, apiPut } from "./client";
import type { FamilyPermission, ScheduleSide } from "../auth/AuthContext";

export type ExchangeDetailLevel = "Day" | "DayAndTime" | "DayTimeAndPlace";
export type FamilyStatus = "Active" | "Suspended";
export type ScheduleTemplate =
  | "AlternatingWeeks"
  | "TwoTwoThree"
  | "TwoTwoFiveFive"
  | "ThreeFourFourThree"
  | "PrimaryAlternateWeekends";

export interface CalendarSummaryDto {
  id: string;
  name: string;
  isActive: boolean;
  childCount: number;
}

export interface FamilyDto {
  id: string;
  name: string;
  timeZoneId: string;
  sideALabel: string;
  sideBLabel: string;
  exchangeDetailLevel: ExchangeDetailLevel;
  status: FamilyStatus;
  myMemberId: string;
  myPermission: FamilyPermission;
  mySide: ScheduleSide | null;
  calendars: CalendarSummaryDto[];
  activeChildren: number;
  hasActiveSchedule: boolean;
}

export interface MemberDto {
  id: string;
  accountId: string;
  email: string;
  displayName: string;
  permission: FamilyPermission;
  side: ScheduleSide | null;
  isActive: boolean;
  joinedAt: string;
}

export interface ChildDto {
  id: string;
  displayName: string;
  calendarId: string | null;
  isActive: boolean;
}

export interface ResidenceCalendarDto {
  id: string;
  name: string;
  isActive: boolean;
  children: ChildDto[];
}

export interface InvitationDto {
  id: string;
  familyId: string;
  familyName: string;
  emailHint: string | null;
  permission: "Editor" | "Viewer";
  side: ScheduleSide | null;
  createdAt: string;
  expiresAt: string;
  consumedAt: string | null;
  revokedAt: string | null;
  status: "Active" | "Consumed" | "Expired" | "Revoked";
}

export interface CreatedInvitationDto {
  invitation: InvitationDto;
  link: string;
  code: string;
}

export interface JoinPreviewDto {
  invitationId: string;
  familyId: string;
  familyName: string;
  emailHint: string | null;
  permission: "Editor" | "Viewer";
  side: ScheduleSide | null;
  expiresAt: string;
  alreadyMember: boolean;
}

export interface JoinResultDto {
  familyId: string;
  familyName: string;
  joined: boolean;
}

export interface ScheduleDraft {
  template: ScheduleTemplate;
  anchorDate: string;
  anchorSide: ScheduleSide;
  effectiveFrom: string;
  parameters: {
    weekendStartsOn: number;
    weekendLengthDays: number;
    recurringWeekday: number | null;
    recurringWeekdayOvernight: boolean;
  };
  changeoverTime: string | null;
  changeoverPlace: string | null;
}

export interface CommentDto {
  id: string;
  authorMemberId: string;
  authorName: string;
  text: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CalendarDayDto {
  date: string;
  side: ScheduleSide | null;
  isOverride: boolean;
  isVab: boolean;
  specialStatus: string | null;
  changeoverTime: string | null;
  changeoverPlace: string | null;
  hasRecurringVisit: boolean;
  comments: CommentDto[];
}

export interface SchedulePreviewDto {
  days: CalendarDayDto[];
  validUntil: string;
}

export interface CalendarMonthDto {
  year: number;
  month: number;
  days: CalendarDayDto[];
}

export interface ScheduleVersionDto extends Omit<ScheduleDraft, "parameters"> {
  id: string;
  parameters: ScheduleDraft["parameters"];
  createdAt: string;
}

export interface ChangeRequestDto {
  id: string;
  calendarId: string;
  calendarName: string;
  requestedByMemberId: string;
  requestedByName: string;
  fromDate: string;
  toDate: string;
  requestedSide: ScheduleSide;
  message: string | null;
  status: "Pending" | "Approved" | "Rejected" | "Cancelled";
  createdAt: string;
  reviewedAt: string | null;
}

export interface AdminFamilyListItemDto {
  id: string;
  name: string;
  timeZoneId: string;
  status: FamilyStatus;
  memberCount: number;
  calendarCount: number;
  scheduleVersionCount: number;
  updatedAt: string;
}

export interface AuditEventDto {
  id: string;
  familyId: string | null;
  actorType: string;
  action: string;
  targetType: string;
  targetId: string;
  reason: string;
  createdAt: string;
}

export interface AdminFamilyDetailDto {
  family: AdminFamilyListItemDto;
  members: MemberDto[];
  calendars: CalendarSummaryDto[];
  scheduleVersions: AdminScheduleVersionDto[];
  invitations: AdminInvitationDto[];
  activity: AuditEventDto[];
}

export interface AdminScheduleVersionDto {
  id: string;
  calendarName: string;
  template: ScheduleTemplate;
  effectiveFrom: string;
  createdAt: string;
}

export interface AdminInvitationDto {
  id: string;
  emailHint: string | null;
  permission: "Editor" | "Viewer";
  side: ScheduleSide | null;
  expiresAt: string;
  status: "Active" | "Consumed" | "Expired" | "Revoked";
}

export const familyApi = {
  list: () => apiGet<FamilyDto[]>("/api/families"),
  get: (familyId: string) => apiGet<FamilyDto>(`/api/families/${familyId}`),
  create: (name: string) => apiPost<FamilyDto>("/api/families", { name }),
  update: (familyId: string, value: Omit<FamilyDto, "id" | "status" | "myMemberId" | "myPermission" | "mySide" | "calendars" | "activeChildren" | "hasActiveSchedule">) =>
    apiPut<FamilyDto>(`/api/families/${familyId}`, value),
  members: (familyId: string) => apiGet<MemberDto[]>(`/api/families/${familyId}/members`),
  updateMember: (familyId: string, memberId: string, permission: FamilyPermission, side: ScheduleSide | null, reason: string) =>
    apiPut<MemberDto>(`/api/families/${familyId}/members/${memberId}`, { permission, side, reason }),
  transferOwnership: (familyId: string, newOwnerMemberId: string, reason: string) =>
    apiPost<void>(`/api/families/${familyId}/ownership-transfer`, { newOwnerMemberId, reason }),
  children: (familyId: string) => apiGet<ChildDto[]>(`/api/families/${familyId}/children`),
  createChild: (familyId: string, displayName: string, calendarId: string | null) =>
    apiPost<ChildDto>(`/api/families/${familyId}/children`, { displayName, calendarId }),
  calendars: (familyId: string) => apiGet<ResidenceCalendarDto[]>(`/api/families/${familyId}/calendars`),
  createCalendar: (familyId: string, name: string) =>
    apiPost<ResidenceCalendarDto>(`/api/families/${familyId}/calendars`, { name }),
  invitations: (familyId: string) => apiGet<InvitationDto[]>(`/api/families/${familyId}/invitations`),
  createInvitation: (familyId: string, permission: "Editor" | "Viewer", side: ScheduleSide | null, emailHint: string) =>
    apiPost<CreatedInvitationDto>(`/api/families/${familyId}/invitations`, { permission, side, emailHint: emailHint || null, validDays: 7 }),
  revokeInvitation: (familyId: string, invitationId: string) =>
    apiDelete(`/api/families/${familyId}/invitations/${invitationId}`),
};

export const joinApi = {
  preview: (token: string) => apiGet<JoinPreviewDto>(`/api/join/${encodeURIComponent(token)}`),
  previewCode: (code: string) => apiPost<JoinPreviewDto>("/api/join/code/preview", { code }),
  complete: (token: string) => apiPost<JoinResultDto>(`/api/join/${encodeURIComponent(token)}/complete`),
  code: (code: string) => apiPost<JoinResultDto>("/api/join/code", { code }),
};

export const scheduleApi = {
  month: (familyId: string, calendarId: string, year: number, month: number) =>
    apiGet<CalendarMonthDto>(`/api/families/${familyId}/calendars/${calendarId}/months/${year}/${month}`),
  preview: (familyId: string, calendarId: string, draft: ScheduleDraft) =>
    apiPost<SchedulePreviewDto>(`/api/families/${familyId}/calendars/${calendarId}/schedule/preview`, draft),
  create: (familyId: string, calendarId: string, draft: ScheduleDraft) =>
    apiPost<ScheduleVersionDto>(`/api/families/${familyId}/calendars/${calendarId}/schedule/versions`, draft),
  versions: (familyId: string, calendarId: string) =>
    apiGet<ScheduleVersionDto[]>(`/api/families/${familyId}/calendars/${calendarId}/schedule/versions`),
  setOverride: (
    familyId: string,
    calendarId: string,
    date: string,
    value: Pick<CalendarDayDto, "side" | "isVab" | "specialStatus" | "changeoverTime" | "changeoverPlace">,
  ) =>
    apiPut<CalendarDayDto>(`/api/families/${familyId}/calendars/${calendarId}/days/${date}`, {
      ...value,
    }),
  clearOverride: (familyId: string, calendarId: string, date: string) =>
    apiDelete(`/api/families/${familyId}/calendars/${calendarId}/days/${date}`),
  addComment: (familyId: string, calendarId: string, date: string, text: string) =>
    apiPost<CommentDto>(`/api/families/${familyId}/calendars/${calendarId}/days/${date}/comments`, { text }),
  updateComment: (familyId: string, commentId: string, text: string) =>
    apiPut<CommentDto>(`/api/families/${familyId}/comments/${commentId}`, { text }),
  deleteComment: (familyId: string, commentId: string) =>
    apiDelete(`/api/families/${familyId}/comments/${commentId}`),
};

export const changeRequestApi = {
  list: (familyId: string) => apiGet<ChangeRequestDto[]>(`/api/families/${familyId}/change-requests`),
  create: (familyId: string, calendarId: string, fromDate: string, toDate: string, requestedSide: ScheduleSide, message: string) =>
    apiPost<ChangeRequestDto>(`/api/families/${familyId}/calendars/${calendarId}/change-requests`, {
      fromDate, toDate, requestedSide, message: message || null,
    }),
  review: (familyId: string, requestId: string, approved: boolean, message?: string) =>
    apiPost<ChangeRequestDto>(`/api/families/${familyId}/change-requests/${requestId}/review`, { approved, message: message || null }),
};

export const adminApi = {
  families: () => apiGet<AdminFamilyListItemDto[]>("/api/admin/families"),
  family: (familyId: string) => apiGet<AdminFamilyDetailDto>(`/api/admin/families/${familyId}`),
  updateFamily: (familyId: string, value: { name?: string; timeZoneId?: string; status?: FamilyStatus; reason: string }) =>
    apiPut<void>(`/api/admin/families/${familyId}`, value),
  updateMember: (familyId: string, memberId: string, permission: FamilyPermission, side: ScheduleSide | null, reason: string) =>
    apiPut<void>(`/api/admin/families/${familyId}/members/${memberId}`, { permission, side, reason }),
  transferOwnership: (familyId: string, newOwnerMemberId: string, reason: string) =>
    apiPost<void>(`/api/admin/families/${familyId}/ownership-transfer`, { newOwnerMemberId, reason }),
  revokeInvitation: (familyId: string, invitationId: string, reason: string) =>
    apiPost<void>(`/api/admin/families/${familyId}/invitations/${invitationId}/revoke`, { reason }),
  revokeSessions: (accountId: string, reason: string) =>
    apiPost<{ revoked: number }>(`/api/admin/accounts/${accountId}/sessions/revoke`, { reason }),
};
