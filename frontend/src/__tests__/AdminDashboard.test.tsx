import { render, screen } from "@testing-library/react";
import { ThemeProvider } from "@mui/material/styles";
import { beforeEach, describe, expect, it, vi } from "vitest";
import AdminDashboard from "../components/AdminDashboard";
import { theme } from "../theme";

const getUsers = vi.fn();
vi.mock("../api/admin", () => ({ getUsers: () => getUsers() }));

describe("AdminDashboard", () => {
  beforeEach(() => getUsers.mockResolvedValue([]));

  it("contains user administration without invitation controls", async () => {
    render(<ThemeProvider theme={theme}><AdminDashboard /></ThemeProvider>);
    expect(screen.getByText("Administration")).toBeInTheDocument();
    expect(await screen.findByTestId("users-table")).toBeInTheDocument();
    expect(screen.queryByText(/magisk länk/i)).not.toBeInTheDocument();
  });
});
