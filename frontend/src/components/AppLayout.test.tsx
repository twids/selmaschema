import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { AuthContext } from "../auth/AuthContext";
import AppLayout from "./AppLayout";

describe("AppLayout v2", () => {
  it("shows family navigation but never exposes the platform admin route", () => {
    render(<MemoryRouter initialEntries={["/families/f"]}><AuthContext.Provider value={{
      account: { id: "a", email: "owner@example.test", displayName: "Owner" },
      memberships: [{ familyId: "f", familyName: "Familjen", permission: "Owner", side: "A", status: "Active" }],
      isAuthenticated: true, isLoading: false, startOidcLogin: () => undefined,
      logout: async () => undefined, refresh: async () => undefined,
    }}><Routes><Route path="/families/:familyId" element={<AppLayout><div>innehåll</div></AppLayout>} /></Routes></AuthContext.Provider></MemoryRouter>);
    expect(screen.getByRole("link", { name: "Inställningar" })).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: /admin/i })).not.toBeInTheDocument();
  });
});
