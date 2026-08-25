import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { AuthContext } from "../auth/AuthContext";
import OnboardingPage from "./OnboardingPage";

const mocks = vi.hoisted(() => ({ create: vi.fn(), join: vi.fn() }));
vi.mock("../api/v2", () => ({ familyApi: { create: mocks.create }, joinApi: { code: mocks.join } }));

describe("OnboardingPage", () => {
  beforeEach(() => { mocks.create.mockReset(); mocks.join.mockReset(); });
  it("offers create family and join by code", async () => {
    mocks.create.mockResolvedValue({ id: "family" });
    const refresh = vi.fn().mockResolvedValue(undefined);
    render(<MemoryRouter initialEntries={["/onboarding"]}><AuthContext.Provider value={{ account: { id: "a", email: "x", displayName: "X" }, memberships: [], isAuthenticated: true, isLoading: false, startOidcLogin: () => undefined, logout: async () => undefined, refresh }}><Routes><Route path="/onboarding" element={<OnboardingPage />} /><Route path="/families/:familyId/setup" element={<div>Familjeguiden</div>} /></Routes></AuthContext.Provider></MemoryRouter>);
    expect(screen.getByRole("heading", { name: "Skapa familj" })).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "Gå med med kod" })).toBeInTheDocument();
    fireEvent.change(screen.getByLabelText("Familjens namn"), { target: { value: "Ny familj" } });
    fireEvent.click(screen.getByRole("button", { name: "Skapa familj" }));
    await waitFor(() => expect(mocks.create).toHaveBeenCalledWith("Ny familj"));
    expect(await screen.findByText("Familjeguiden")).toBeInTheDocument();
  });
});
