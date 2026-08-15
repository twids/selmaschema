import { apiGet, apiPost, apiDelete } from './client';

export interface ChangeRequestDto {
  id: number;
  requestedByName: string;
  requestedForDate: string; // ISO date string "YYYY-MM-DD"
  currentParent: string; // "A" or "B"
  requestedParent: string; // "A" or "B"
  status: string; // "Pending", "Approved", "Rejected", "Cancelled"
  comment?: string;
  createdAt: string; // ISO date-time string
  reviewedAt?: string; // ISO date-time string
  reviewedByName?: string;
}

export interface CreateChangeRequestDto {
  dates: string[]; // ISO date strings "YYYY-MM-DD"
  requestedParent: string; // "A" or "B"
  comment?: string;
}

export interface ReviewChangeRequestDto {
  approved: boolean;
  comment?: string;
}

/**
 * Creates change requests for multiple dates
 */
export async function createChangeRequests(
  dates: string[],
  requestedParent: string,
  comment?: string
): Promise<ChangeRequestDto[]> {
  const dto: CreateChangeRequestDto = {
    dates,
    requestedParent,
    comment,
  };

  return apiPost<ChangeRequestDto[]>('/api/change-requests', dto);
}

/**
 * Gets all change requests created by or affecting the current user
 */
export async function getMyChangeRequests(): Promise<ChangeRequestDto[]> {
  return apiGet<ChangeRequestDto[]>('/api/change-requests');
}

/**
 * Gets all pending change requests
 */
export async function getPendingChangeRequests(): Promise<ChangeRequestDto[]> {
  return apiGet<ChangeRequestDto[]>('/api/change-requests/pending');
}

/**
 * Gets a specific change request by ID
 */
export async function getChangeRequest(
  id: number,
): Promise<ChangeRequestDto> {
  return apiGet<ChangeRequestDto>(`/api/change-requests/${id}`);
}

/**
 * Reviews a change request (approve or reject)
 */
export async function reviewChangeRequest(
  id: number,
  approved: boolean,
  comment?: string
): Promise<ChangeRequestDto> {
  const dto: ReviewChangeRequestDto = {
    approved,
    comment,
  };

  return apiPost<ChangeRequestDto>(
    `/api/change-requests/${id}/review`,
    dto,
  );
}

/**
 * Cancels a pending change request (only creator can cancel)
 */
export async function cancelChangeRequest(
  id: number,
): Promise<void> {
  return apiDelete(`/api/change-requests/${id}`);
}
