import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, act, waitFor } from "@testing-library/react";
import { CalendarProvider, useCalendar } from "../context/CalendarContext";
import { AuthContext } from "../auth/AuthContext";
import type { MonthDataDto, DayAssignmentDto } from "../api/types";

/* ---------- helpers ---------- */

function mockAuthValue() {
  return {
    token: "test-token",
    user: null,
    isAuthenticated: true,
    loginAdmin: vi.fn(),
    exchangeMagicToken: vi.fn(),
    logout: vi.fn(),
    authHeader: () => ({ Authorization: "Bearer test-token" }),
  };
}

function wrapper({ children }: { children: React.ReactNode }) {
  return (
    <AuthContext.Provider value={mockAuthValue()}>
      <CalendarProvider>{children}</CalendarProvider>
    </AuthContext.Provider>
  );
}

/** A consumer component that exposes CalendarContext values for testing. */
function CalendarConsumer({
  onCtx,
}: {
  onCtx: (ctx: ReturnType<typeof useCalendar>) => void;
}) {
  const ctx = useCalendar();
  onCtx(ctx);
  return (
    <div>
      <span data-testid="year">{ctx.currentYear}</span>
      <span data-testid="month">{ctx.currentMonth}</span>
      <span data-testid="loading">{String(ctx.loading)}</span>
      <span data-testid="error">{ctx.error ?? "none"}</span>
    </div>
  );
}

function buildMonthData(year: number, month: number): MonthDataDto {
  const day: DayAssignmentDto = {
    id: 1,
    date: `${year}-${String(month).padStart(2, "0")}-15T00:00:00Z`,
    parent: "A",
    isVAB: false,
    specialStatus: null,
    parentAComments: [],
    parentBComments: [],
  };
  return { year, month, days: [day] };
}

/* ---------- tests ---------- */

describe("CalendarContext", () => {
  beforeEach(() => {
    vi.stubGlobal("fetch", vi.fn() as unknown as typeof globalThis.fetch);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("provides default year and month as current date", () => {
    // suppress data-loading fetch
    vi.mocked(globalThis.fetch).mockImplementation(() =>
      Promise.resolve(
        new Response(JSON.stringify({ year: 2026, month: 2, days: [] }), {
          status: 200,
        })
      )
    );

    const now = new Date();
    let capturedCtx: ReturnType<typeof useCalendar> | undefined;

    render(
      <CalendarConsumer onCtx={(c) => (capturedCtx = c)} />,
      { wrapper }
    );

    expect(capturedCtx!.currentYear).toBe(now.getFullYear());
    expect(capturedCtx!.currentMonth).toBe(now.getMonth() + 1);
  });

  it("loads month data and converts to CalendarData map", async () => {
    const monthData = buildMonthData(2025, 6);

    vi.mocked(globalThis.fetch).mockImplementation(() =>
      Promise.resolve(new Response(JSON.stringify(monthData), { status: 200 }))
    );

    let capturedCtx: ReturnType<typeof useCalendar> | undefined;

    render(
      <CalendarConsumer onCtx={(c) => (capturedCtx = c)} />,
      { wrapper }
    );

    await waitFor(() => {
      expect(capturedCtx!.loading).toBe(false);
    });

    // The data should be keyed by YYYY-MM-DD
    const key = "2025-06-15";
    expect(capturedCtx!.calendarData[key]).toBeDefined();
    expect(capturedCtx!.calendarData[key].parent).toBe("A");
  });

  it("navigateMonth increments month", async () => {
    vi.mocked(globalThis.fetch).mockImplementation(() =>
      Promise.resolve(
        new Response(JSON.stringify({ year: 2025, month: 6, days: [] }), {
          status: 200,
        })
      )
    );

    let capturedCtx: ReturnType<typeof useCalendar> | undefined;

    render(
      <CalendarConsumer onCtx={(c) => (capturedCtx = c)} />,
      { wrapper }
    );

    await waitFor(() => expect(capturedCtx!.loading).toBe(false));

    await act(async () => {
      capturedCtx!.navigateMonth(1);
    });

    // month should have incremented by 1 from default
    const expectedMonth = new Date().getMonth() + 2; // current + 1
    if (expectedMonth <= 12) {
      expect(capturedCtx!.currentMonth).toBe(expectedMonth);
    } else {
      // wrapped to next year
      expect(capturedCtx!.currentMonth).toBe(1);
      expect(capturedCtx!.currentYear).toBe(new Date().getFullYear() + 1);
    }
  });

  it("navigateMonth decrements and rolls year back", async () => {
    vi.mocked(globalThis.fetch).mockImplementation(() =>
      Promise.resolve(
        new Response(JSON.stringify({ year: 2025, month: 1, days: [] }), {
          status: 200,
        })
      )
    );

    let capturedCtx: ReturnType<typeof useCalendar> | undefined;

    render(
      <CalendarConsumer onCtx={(c) => (capturedCtx = c)} />,
      { wrapper }
    );

    await waitFor(() => expect(capturedCtx!.loading).toBe(false));

    // Set to January first
    await act(async () => {
      capturedCtx!.setMonth(1);
      capturedCtx!.setYear(2025);
    });

    await waitFor(() => expect(capturedCtx!.currentMonth).toBe(1));

    await act(async () => {
      capturedCtx!.navigateMonth(-1);
    });

    expect(capturedCtx!.currentMonth).toBe(12);
    expect(capturedCtx!.currentYear).toBe(2024);
  });

  it("setMonth and setYear update state", async () => {
    vi.mocked(globalThis.fetch).mockImplementation(() =>
      Promise.resolve(
        new Response(JSON.stringify({ year: 2025, month: 3, days: [] }), {
          status: 200,
        })
      )
    );

    let capturedCtx: ReturnType<typeof useCalendar> | undefined;

    render(
      <CalendarConsumer onCtx={(c) => (capturedCtx = c)} />,
      { wrapper }
    );

    await waitFor(() => expect(capturedCtx!.loading).toBe(false));

    await act(async () => {
      capturedCtx!.setYear(2030);
      capturedCtx!.setMonth(11);
    });

    await waitFor(() => {
      expect(capturedCtx!.currentYear).toBe(2030);
      expect(capturedCtx!.currentMonth).toBe(11);
    });
  });

  it("updateDay calls PUT and reloads data", async () => {
    const monthData = buildMonthData(2025, 6);

    vi.mocked(globalThis.fetch).mockImplementation(() =>
      Promise.resolve(new Response(JSON.stringify(monthData), { status: 200 }))
    );

    let capturedCtx: ReturnType<typeof useCalendar> | undefined;

    render(
      <CalendarConsumer onCtx={(c) => (capturedCtx = c)} />,
      { wrapper }
    );

    await waitFor(() => expect(capturedCtx!.loading).toBe(false));

    // Set to the month we have data for
    await act(async () => {
      capturedCtx!.setYear(2025);
      capturedCtx!.setMonth(6);
    });

    await waitFor(() => expect(capturedCtx!.loading).toBe(false));

    // Now call updateDay
    await act(async () => {
      await capturedCtx!.updateDay(2025, 6, 15, {
        parent: "B",
        isVAB: true,
        specialStatus: null,
      });
    });

    // Should have called PUT
    const calls = vi.mocked(globalThis.fetch).mock.calls;
    const putCall = calls.find(
      (c) => typeof c[1] === "object" && c[1]?.method === "PUT"
    );
    expect(putCall).toBeDefined();
    expect(putCall![0]).toContain("/api/days/2025/6/15");
  });

  it("initializeMonth calls POST and reloads data", async () => {
    vi.mocked(globalThis.fetch).mockImplementation(() =>
      Promise.resolve(
        new Response(JSON.stringify({ year: 2025, month: 6, days: [] }), {
          status: 200,
        })
      )
    );

    let capturedCtx: ReturnType<typeof useCalendar> | undefined;

    render(
      <CalendarConsumer onCtx={(c) => (capturedCtx = c)} />,
      { wrapper }
    );

    await waitFor(() => expect(capturedCtx!.loading).toBe(false));

    await act(async () => {
      capturedCtx!.setYear(2025);
      capturedCtx!.setMonth(6);
    });

    await waitFor(() => expect(capturedCtx!.loading).toBe(false));

    await act(async () => {
      await capturedCtx!.initializeMonth(2025, 6);
    });

    const calls = vi.mocked(globalThis.fetch).mock.calls;
    const postCall = calls.find(
      (c) => typeof c[1] === "object" && c[1]?.method === "POST"
    );
    expect(postCall).toBeDefined();
    expect(postCall![0]).toContain("/api/days/2025/6/initialize");
  });

  it("sets error when load fails", async () => {
    vi.mocked(globalThis.fetch).mockImplementation(() =>
      Promise.resolve(
        new Response("Server Error", { status: 500, statusText: "Internal Server Error" })
      )
    );

    let capturedCtx: ReturnType<typeof useCalendar> | undefined;

    render(
      <CalendarConsumer onCtx={(c) => (capturedCtx = c)} />,
      { wrapper }
    );

    await waitFor(() => {
      expect(capturedCtx!.loading).toBe(false);
    });

    expect(capturedCtx!.error).toBeTruthy();
  });

  it("throws when useCalendar is used outside provider", () => {
    const consoleError = vi.spyOn(console, "error").mockImplementation(() => {});
    expect(() =>
      render(<CalendarConsumer onCtx={() => {}} />)
    ).toThrow("useCalendar must be used within CalendarProvider");
    consoleError.mockRestore();
  });
});
