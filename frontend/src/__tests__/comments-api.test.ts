import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";

import { addComment, getComments } from "../api/comments";
import type { CommentDto, CreateCommentDto } from "../api/types";

describe("Comments API", () => {
  const mockAuthHeader = () => ({ Authorization: "Bearer test-token" });

  beforeEach(() => {
    vi.stubGlobal(
      "fetch",
      vi.fn() as unknown as typeof globalThis.fetch,
    );
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe("addComment", () => {
    it("sends POST to /api/comments/{dayAssignmentId} with body", async () => {
      const returnedComment: CommentDto = {
        id: 42,
        dayAssignmentId: 7,
        parent: "A",
        commentText: "Hello",
        createdAt: "2026-02-07T10:00:00Z",
        modifiedAt: null,
      };

      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response(JSON.stringify(returnedComment), {
          status: 200,
          headers: { "Content-Type": "application/json" },
        }),
      );

      const data: CreateCommentDto = { parent: "A", commentText: "Hello" };
      const result = await addComment(7, data, mockAuthHeader);

      expect(globalThis.fetch).toHaveBeenCalledWith(
        "/api/comments/7",
        expect.objectContaining({
          method: "POST",
          headers: expect.objectContaining({
            Authorization: "Bearer test-token",
            "Content-Type": "application/json",
          }),
          body: JSON.stringify({ parent: "A", commentText: "Hello" }),
        }),
      );
      expect(result).toEqual(returnedComment);
    });

    it("sends POST for parent B comment", async () => {
      const returnedComment: CommentDto = {
        id: 43,
        dayAssignmentId: 7,
        parent: "B",
        commentText: "World",
        createdAt: "2026-02-07T11:00:00Z",
        modifiedAt: null,
      };

      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response(JSON.stringify(returnedComment), {
          status: 200,
          headers: { "Content-Type": "application/json" },
        }),
      );

      const data: CreateCommentDto = { parent: "B", commentText: "World" };
      const result = await addComment(7, data, mockAuthHeader);

      expect(result.parent).toBe("B");
      expect(result.commentText).toBe("World");
    });

    it("throws on non-OK response", async () => {
      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response("Server Error", { status: 500, statusText: "Internal Server Error" }),
      );

      await expect(
        addComment(7, { parent: "A", commentText: "fail" }, mockAuthHeader),
      ).rejects.toThrow(/500/);
    });
  });

  describe("getComments", () => {
    it("sends GET to /api/comments/{dayAssignmentId}", async () => {
      const comments: CommentDto[] = [
        {
          id: 1,
          dayAssignmentId: 5,
          parent: "A",
          commentText: "First",
          createdAt: "2026-02-07T09:00:00Z",
          modifiedAt: null,
        },
        {
          id: 2,
          dayAssignmentId: 5,
          parent: "B",
          commentText: "Second",
          createdAt: "2026-02-07T10:00:00Z",
          modifiedAt: null,
        },
      ];

      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response(JSON.stringify(comments), {
          status: 200,
          headers: { "Content-Type": "application/json" },
        }),
      );

      const result = await getComments(5, mockAuthHeader);

      expect(globalThis.fetch).toHaveBeenCalledWith(
        "/api/comments/5",
        expect.objectContaining({
          method: "GET",
          headers: expect.objectContaining({
            Authorization: "Bearer test-token",
          }),
        }),
      );
      expect(result).toEqual(comments);
      expect(result).toHaveLength(2);
    });

    it("returns empty array when no comments", async () => {
      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response(JSON.stringify([]), {
          status: 200,
          headers: { "Content-Type": "application/json" },
        }),
      );

      const result = await getComments(99, mockAuthHeader);
      expect(result).toEqual([]);
    });

    it("throws on non-OK response", async () => {
      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response("Not Found", { status: 404, statusText: "Not Found" }),
      );

      await expect(getComments(999, mockAuthHeader)).rejects.toThrow(/404/);
    });
  });
});
