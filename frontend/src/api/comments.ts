import { apiGet, apiPost } from "./client";
import type { CommentDto, CreateCommentDto } from "./types";

export async function addComment(
  dayAssignmentId: number,
  data: CreateCommentDto,
  getAuthHeader: () => Record<string, string>,
): Promise<CommentDto> {
  return apiPost<CommentDto>(
    `/api/comments/${dayAssignmentId}`,
    data,
    getAuthHeader,
  );
}

export async function getComments(
  dayAssignmentId: number,
  getAuthHeader: () => Record<string, string>,
): Promise<CommentDto[]> {
  return apiGet<CommentDto[]>(
    `/api/comments/${dayAssignmentId}`,
    getAuthHeader,
  );
}
