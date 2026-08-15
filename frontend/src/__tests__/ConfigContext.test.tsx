import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, waitFor, act } from "@testing-library/react";
import { ConfigProvider, useConfig } from "../context/ConfigContext";
import type { ParentNamesDto } from "../api/types";

/* ---------- helpers ---------- */

function wrapper({ children }: { children: React.ReactNode }) {
  return <ConfigProvider>{children}</ConfigProvider>;
}

function ConfigConsumer({
  onCtx,
}: {
  onCtx: (ctx: ReturnType<typeof useConfig>) => void;
}) {
  const ctx = useConfig();
  onCtx(ctx);
  return (
    <div>
      <span data-testid="parentA">{ctx.parentNames.parentAName}</span>
      <span data-testid="parentB">{ctx.parentNames.parentBName}</span>
      <span data-testid="loading">{String(ctx.loading)}</span>
    </div>
  );
}

/* ---------- tests ---------- */

describe("ConfigContext", () => {
  beforeEach(() => {
    vi.stubGlobal("fetch", vi.fn() as unknown as typeof globalThis.fetch);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("loads parent names on mount", async () => {
    const names: ParentNamesDto = {
      parentAName: "Alice",
      parentBName: "Bob",
    };

    vi.mocked(globalThis.fetch).mockImplementation(() =>
      Promise.resolve(new Response(JSON.stringify(names), { status: 200 }))
    );

    let capturedCtx: ReturnType<typeof useConfig> | undefined;

    render(<ConfigConsumer onCtx={(c) => (capturedCtx = c)} />, { wrapper });

    await waitFor(() => {
      expect(capturedCtx!.loading).toBe(false);
    });

    expect(capturedCtx!.parentNames.parentAName).toBe("Alice");
    expect(capturedCtx!.parentNames.parentBName).toBe("Bob");
  });

  it("provides default parent names while loading", () => {
    vi.mocked(globalThis.fetch).mockReturnValue(new Promise(() => {})); // never resolves

    let capturedCtx: ReturnType<typeof useConfig> | undefined;

    render(<ConfigConsumer onCtx={(c) => (capturedCtx = c)} />, { wrapper });

    // Defaults should be reasonable (non-empty)
    expect(capturedCtx!.parentNames.parentAName).toBeDefined();
    expect(capturedCtx!.parentNames.parentBName).toBeDefined();
    expect(capturedCtx!.loading).toBe(true);
  });

  it("updateParentNames calls PUT and updates state", async () => {
    const initialNames: ParentNamesDto = {
      parentAName: "Alice",
      parentBName: "Bob",
    };

    const updatedNames: ParentNamesDto = {
      parentAName: "Carol",
      parentBName: "Dave",
    };

    vi.mocked(globalThis.fetch)
      .mockResolvedValueOnce(
        new Response(JSON.stringify(initialNames), { status: 200 })
      )
      .mockResolvedValueOnce(
        new Response(JSON.stringify(updatedNames), { status: 200 })
      );

    let capturedCtx: ReturnType<typeof useConfig> | undefined;

    render(<ConfigConsumer onCtx={(c) => (capturedCtx = c)} />, { wrapper });

    await waitFor(() => expect(capturedCtx!.loading).toBe(false));

    await act(async () => {
      await capturedCtx!.updateParentNames("Carol", "Dave");
    });

    // Check that PUT was called
    const calls = vi.mocked(globalThis.fetch).mock.calls;
    const putCall = calls.find(
      (c) => typeof c[1] === "object" && c[1]?.method === "PUT"
    );
    expect(putCall).toBeDefined();
    expect(putCall![0]).toContain("/api/config/parent-names");

    expect(capturedCtx!.parentNames.parentAName).toBe("Carol");
    expect(capturedCtx!.parentNames.parentBName).toBe("Dave");
  });

  it("throws when useConfig is used outside provider", () => {
    const consoleError = vi.spyOn(console, "error").mockImplementation(() => {});
    expect(() =>
      render(<ConfigConsumer onCtx={() => {}} />)
    ).toThrow("useConfig must be used within ConfigProvider");
    consoleError.mockRestore();
  });
});
