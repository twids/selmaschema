import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { AuthContext } from "../auth/AuthContext";
import ProtectedRoute from "./ProtectedRoute";

const base = {
  account: null,
  memberships: [],
  isAuthenticated: false,
  isLoading: false,
  startOidcLogin: () => undefined,
  logout: async () => undefined,
  refresh: async () => undefined,
};

describe("ProtectedRoute v2", () => {
  it("keeps protected content hidden during auth bootstrap", () => {
    render(<MemoryRouter><AuthContext.Provider value={{ ...base, isLoading: true }}><ProtectedRoute><div>hemligt</div></ProtectedRoute></AuthContext.Provider></MemoryRouter>);
    expect(screen.getByRole("progressbar")).toBeInTheDocument();
    expect(screen.queryByText("hemligt")).not.toBeInTheDocument();
  });

  it("redirects unauthenticated visitors to login", () => {
    render(<MemoryRouter initialEntries={["/families/f"]}><AuthContext.Provider value={base}><Routes><Route path="/login" element={<div>login</div>} /><Route path="/families/:id" element={<ProtectedRoute><div>hemligt</div></ProtectedRoute>} /></Routes></AuthContext.Provider></MemoryRouter>);
    expect(screen.getByText("login")).toBeInTheDocument();
  });
});
