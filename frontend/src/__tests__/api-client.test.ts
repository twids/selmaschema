import { afterEach, describe, expect, it, vi } from "vitest";
import { apiDelete, apiGet, apiPost, apiPut } from "../api/client";

describe("cookie API client", () => {
  afterEach(() => vi.restoreAllMocks());

  it.each([
    ["GET", () => apiGet("/api/test")],
    ["POST", () => apiPost("/api/test", { value: 1 })],
    ["PUT", () => apiPut("/api/test", { value: 1 })],
    ["DELETE", () => apiDelete("/api/test")],
  ])("uses credentials include for %s without Authorization", async (method, request) => {
    const fetchMock = vi.spyOn(globalThis, "fetch").mockResolvedValue(
      new Response(method === "DELETE" ? null : JSON.stringify({ ok: true }), {
        status: method === "DELETE" ? 204 : 200,
        headers: { "Content-Type": "application/json" },
      }),
    );

    await request();

    const init = fetchMock.mock.calls[0][1] as RequestInit;
    expect(init.credentials).toBe("include");
    expect(new Headers(init.headers).has("Authorization")).toBe(false);
  });
});
