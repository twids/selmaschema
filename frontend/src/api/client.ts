/**
 * Typed fetch wrapper for the CoParenting API.
 *
 * The client does NOT import AuthContext directly — callers pass an
 * `authHeader` function that returns the headers to inject.
 */

type AuthHeaderFn = () => Record<string, string>;

function baseUrl(): string {
  return import.meta.env.VITE_API_URL || "";
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = await response.text().catch(() => "");
    throw new Error(
      `API ${response.status} ${response.statusText}${body ? `: ${body}` : ""}`
    );
  }

  // 204 No Content — nothing to parse
  if (response.status === 204) {
    return undefined as unknown as T;
  }

  return response.json() as Promise<T>;
}

/**
 * Send a GET request and return the parsed JSON body.
 */
export async function apiGet<T>(
  path: string,
  authHeader: AuthHeaderFn,
): Promise<T> {
  const response = await fetch(`${baseUrl()}${path}`, {
    method: "GET",
    headers: { ...authHeader() },
  });
  return handleResponse<T>(response);
}

/**
 * Send a POST request with an optional JSON body.
 */
export async function apiPost<T>(
  path: string,
  body: unknown | undefined,
  authHeader: AuthHeaderFn,
): Promise<T> {
  const headers: Record<string, string> = { ...authHeader() };
  const init: RequestInit = { method: "POST", headers };

  if (body !== undefined) {
    headers["Content-Type"] = "application/json";
    init.body = JSON.stringify(body);
  }

  const response = await fetch(`${baseUrl()}${path}`, init);
  return handleResponse<T>(response);
}

/**
 * Send a PUT request with a JSON body.
 */
export async function apiPut<T>(
  path: string,
  body: unknown,
  authHeader: AuthHeaderFn,
): Promise<T> {
  const response = await fetch(`${baseUrl()}${path}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      ...authHeader(),
    },
    body: JSON.stringify(body),
  });
  return handleResponse<T>(response);
}

/**
 * Send a DELETE request. Returns void.
 */
export async function apiDelete(
  path: string,
  authHeader: AuthHeaderFn,
): Promise<void> {
  const response = await fetch(`${baseUrl()}${path}`, {
    method: "DELETE",
    headers: { ...authHeader() },
  });

  if (!response.ok) {
    const body = await response.text().catch(() => "");
    throw new Error(
      `API ${response.status} ${response.statusText}${body ? `: ${body}` : ""}`
    );
  }
}
