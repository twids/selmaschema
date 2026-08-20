import { apiGet } from "./client";

export interface UserDto {
  id: number;
  email: string;
  role: string;
  displayName: string;
  lastLoginAt: string | null;
}

export async function getUsers(): Promise<UserDto[]> {
  return apiGet<UserDto[]>("/api/admin/users");
}
