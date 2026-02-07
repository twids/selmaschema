/**
 * Admin API client functions for magic links and user management.
 */

import { apiGet, apiPost } from "./client";

export interface MagicLinkResponse {
  token: string;
  magicLink: string;
  email: string;
  role: string;
  displayName: string;
  expiresAt: string;
  createdAt: string | null;
}

export interface UserDto {
  id: number;
  email: string;
  role: string;
  displayName: string;
  lastLoginAt: string | null;
}

export interface CreateMagicLinkRequest {
  email: string;
  role: string;
  displayName: string;
}

/** Create a new magic link invitation. */
export async function createMagicLink(
  data: CreateMagicLinkRequest,
  getAuthHeader: () => Record<string, string>,
): Promise<MagicLinkResponse> {
  return apiPost<MagicLinkResponse>("/api/admin/magic-links", data, getAuthHeader);
}

/** Fetch all active (non-expired) magic links. */
export async function getMagicLinks(
  getAuthHeader: () => Record<string, string>,
): Promise<MagicLinkResponse[]> {
  return apiGet<MagicLinkResponse[]>("/api/admin/magic-links", getAuthHeader);
}

/** Fetch all registered users. */
export async function getUsers(
  getAuthHeader: () => Record<string, string>,
): Promise<UserDto[]> {
  return apiGet<UserDto[]>("/api/admin/users", getAuthHeader);
}
