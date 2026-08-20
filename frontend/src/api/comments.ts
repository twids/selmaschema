import { apiGet, apiPost } from "./client";
import type { CommentDto, CreateCommentDto } from "./types";

export async function addComment(
  dayAssignmentId: number,
  data: CreateCommentDto,
): Promise<CommentDto> {
  return apiPost<CommentDto>(`/api/comments/${dayAssignmentId}`, data);
}

export async function getComments(
  dayAssignmentId: number,
): Promise<CommentDto[]> {
  return apiGet<CommentDto[]>(`/api/comments/${dayAssignmentId}`);
}
