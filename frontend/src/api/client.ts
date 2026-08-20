/** Typed fetch wrapper. Authentication is carried only by HttpOnly cookies. */

function baseUrl(): string {
  return import.meta.env.VITE_API_URL || "";
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.text().catch(() => "");
    throw new Error(
      `API ${response.status} ${response.statusText}${body ? `: ${body}` : ""}`,
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export async function apiGet<T>(path: string): Promise<T> {
  const response = await fetch(`${baseUrl()}${path}`, {
    method: "GET",
    credentials: "include",
  });
  return handleResponse<T>(response);
}

export async function apiPost<T>(
  path: string,
  body?: unknown,
): Promise<T> {
  const headers: Record<string, string> = {};
  const init: RequestInit = {
    method: "POST",
    headers,
    credentials: "include",
  };
  if (body !== undefined) {
    headers["Content-Type"] = "application/json";
    init.body = JSON.stringify(body);
  }

  return handleResponse<T>(await fetch(`${baseUrl()}${path}`, init));
}

export async function apiPut<T>(path: string, body: unknown): Promise<T> {
  return handleResponse<T>(
    await fetch(`${baseUrl()}${path}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
      credentials: "include",
    }),
  );
}

export async function apiDelete(path: string): Promise<void> {
  return handleResponse<void>(
    await fetch(`${baseUrl()}${path}`, {
      method: "DELETE",
      credentials: "include",
    }),
  );
}
