import { apiGet, apiPost } from "./client";
import type { Role } from "../auth/AuthContext";

export interface InvitationDto {
  id: number;
  emailHint: string | null;
  role: Role;
  createdByUserId: number;
  createdByName: string;
  createdAt: string;
  expiresAt: string;
  consumedAt: string | null;
  status: "Pending" | "Consumed" | "Expired";
}

export interface CreatedInvitationDto {
  invitation: InvitationDto;
  invitationUrl: string;
}

export interface PendingInvitationDto {
  invitationId: number;
  emailHint: string | null;
  verifiedEmail: string;
  displayName: string;
  role: Role;
  expiresAt: string;
}

export function createInvitation(role: Role, emailHint?: string): Promise<CreatedInvitationDto> {
  return apiPost<CreatedInvitationDto>("/api/invitations", {
    role,
    emailHint: emailHint?.trim() || null,
  });
}

export function getInvitations(): Promise<InvitationDto[]> {
  return apiGet<InvitationDto[]>("/api/invitations");
}

export function getPendingInvitation(): Promise<PendingInvitationDto> {
  return apiGet<PendingInvitationDto>("/api/auth/invitations/pending");
}

export function cancelPendingInvitation(): Promise<void> {
  return apiPost<void>("/api/auth/invitations/cancel");
}
