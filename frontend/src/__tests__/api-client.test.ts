import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";

// Will import from api/client once created
import { apiGet, apiPost, apiPut, apiDelete } from "../api/client";

describe("API Client", () => {
  const mockAuthHeader = () => ({ Authorization: "Bearer test-token" });
  const noAuthHeader = () => ({} as Record<string, string>);

  beforeEach(() => {
    vi.stubGlobal(
      "fetch",
      vi.fn() as unknown as typeof globalThis.fetch
    );
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe("apiGet", () => {
    it("sends a GET request with auth headers", async () => {
      const mockData = { id: 1, name: "test" };
      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response(JSON.stringify(mockData), {
          status: 200,
          headers: { "Content-Type": "application/json" },
        })
      );

      const result = await apiGet<{ id: number; name: string }>(
        "/api/test",
        mockAuthHeader
      );

      expect(globalThis.fetch).toHaveBeenCalledWith(
        "/api/test",
        expect.objectContaining({
          method: "GET",
          headers: expect.objectContaining({
            Authorization: "Bearer test-token",
          }),
        })
      );
      expect(result).toEqual(mockData);
    });

    it("sends a GET request without auth when no token", async () => {
      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response(JSON.stringify({ ok: true }), { status: 200 })
      );

      await apiGet("/api/test", noAuthHeader);

      expect(globalThis.fetch).toHaveBeenCalledWith(
        "/api/test",
        expect.objectContaining({
          method: "GET",
          headers: expect.not.objectContaining({
            Authorization: expect.any(String),
          }),
        })
      );
    });

    it("throws on non-OK response", async () => {
      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response("Not Found", { status: 404, statusText: "Not Found" })
      );

      await expect(apiGet("/api/missing", mockAuthHeader)).rejects.toThrow(
        /404/
      );
    });

    it("prepends base URL from env", async () => {
      const originalEnv = import.meta.env.VITE_API_URL;
      // vitest allows mutating import.meta.env at runtime
      (import.meta.env as Record<string, string>).VITE_API_URL = "http://localhost:5000";

      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response(JSON.stringify({}), { status: 200 })
      );

      await apiGet("/api/test", noAuthHeader);

      expect(globalThis.fetch).toHaveBeenCalledWith(
        "http://localhost:5000/api/test",
        expect.any(Object)
      );

      if (originalEnv === undefined) {
        delete (import.meta.env as Record<string, string>).VITE_API_URL;
      } else {
        (import.meta.env as Record<string, string>).VITE_API_URL = originalEnv;
      }
    });
  });

  describe("apiPost", () => {
    it("sends POST with JSON body and auth headers", async () => {
      const requestBody = { parent: "A", commentText: "Hello" };
      const responseData = { id: 1, ...requestBody };

      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response(JSON.stringify(responseData), { status: 201 })
      );

      const result = await apiPost<typeof responseData>(
        "/api/comments",
        requestBody,
        mockAuthHeader
      );

      expect(globalThis.fetch).toHaveBeenCalledWith(
        "/api/comments",
        expect.objectContaining({
          method: "POST",
          headers: expect.objectContaining({
            "Content-Type": "application/json",
            Authorization: "Bearer test-token",
          }),
          body: JSON.stringify(requestBody),
        })
      );
      expect(result).toEqual(responseData);
    });

    it("sends POST without body when body is undefined", async () => {
      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response(JSON.stringify({ initialized: true }), { status: 200 })
      );

      await apiPost("/api/days/2025/1/initialize", undefined, mockAuthHeader);

      expect(globalThis.fetch).toHaveBeenCalledWith(
        "/api/days/2025/1/initialize",
        expect.objectContaining({
          method: "POST",
        })
      );
    });

    it("throws on server error", async () => {
      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response("Internal Server Error", {
          status: 500,
          statusText: "Internal Server Error",
        })
      );

      await expect(
        apiPost("/api/test", { data: "bad" }, mockAuthHeader)
      ).rejects.toThrow(/500/);
    });
  });

  describe("apiPut", () => {
    it("sends PUT with JSON body and auth headers", async () => {
      const body = { parent: "A", isVAB: false, specialStatus: null };
      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response(JSON.stringify({ success: true }), { status: 200 })
      );

      const result = await apiPut<{ success: boolean }>(
        "/api/days/2025/1/15",
        body,
        mockAuthHeader
      );

      expect(globalThis.fetch).toHaveBeenCalledWith(
        "/api/days/2025/1/15",
        expect.objectContaining({
          method: "PUT",
          headers: expect.objectContaining({
            "Content-Type": "application/json",
            Authorization: "Bearer test-token",
          }),
          body: JSON.stringify(body),
        })
      );
      expect(result).toEqual({ success: true });
    });
  });

  describe("apiDelete", () => {
    it("sends DELETE with auth headers", async () => {
      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response(null, { status: 204 })
      );

      await apiDelete("/api/comments/1", mockAuthHeader);

      expect(globalThis.fetch).toHaveBeenCalledWith(
        "/api/comments/1",
        expect.objectContaining({
          method: "DELETE",
          headers: expect.objectContaining({
            Authorization: "Bearer test-token",
          }),
        })
      );
    });

    it("throws on non-OK response for delete", async () => {
      vi.mocked(globalThis.fetch).mockResolvedValueOnce(
        new Response("Forbidden", { status: 403, statusText: "Forbidden" })
      );

      await expect(
        apiDelete("/api/comments/999", mockAuthHeader)
      ).rejects.toThrow(/403/);
    });
  });
});
