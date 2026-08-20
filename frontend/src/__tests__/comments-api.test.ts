import { afterEach, describe, expect, it, vi } from "vitest";
import { addComment, getComments } from "../api/comments";

describe("comments API", () => {
  afterEach(() => vi.restoreAllMocks());

  it("uses the cookie client for adding and reading comments", async () => {
    const fetchMock = vi.spyOn(globalThis, "fetch")
      .mockResolvedValueOnce(new Response(JSON.stringify({ id: 1 }), { status: 200 }))
      .mockResolvedValueOnce(new Response(JSON.stringify([]), { status: 200 }));

    await addComment(7, { parent: "A", commentText: "Hej" });
    await getComments(7);

    expect(fetchMock.mock.calls[0][0]).toBe("/api/comments/7");
    expect((fetchMock.mock.calls[0][1] as RequestInit).credentials).toBe("include");
    expect(fetchMock.mock.calls[1][0]).toBe("/api/comments/7");
  });
});
