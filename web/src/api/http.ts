import { getAccessToken, refreshSession } from './session';

export const API = '/api/v1';

// A failed API call. Status 0 means the server could not be reached.
export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly fieldErrors: Record<string, string[]> = {},
    readonly detail: string | null = null,
  ) {
    super(`API error ${status}`);
  }
}

type RequestOptions = {
  method?: string;
  // Sent as JSON, or as it is for FormData.
  body?: unknown;
  // After a 401, renew the session once and repeat the request. Off for the sign-in calls, where 401 is an answer.
  retryOn401?: boolean;
};

async function send(path: string, options: RequestOptions): Promise<Response> {
  const headers = new Headers();
  const token = getAccessToken();
  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  let body: BodyInit | undefined;
  if (options.body instanceof FormData) {
    body = options.body;
  } else if (options.body !== undefined) {
    headers.set('Content-Type', 'application/json');
    body = JSON.stringify(options.body);
  }

  try {
    return await fetch(path, { method: options.method ?? 'GET', headers, body });
  } catch {
    throw new ApiError(0);
  }
}

async function toApiError(response: Response): Promise<ApiError> {
  try {
    const problem = (await response.json()) as { errors?: Record<string, string[]>; detail?: string };
    return new ApiError(response.status, problem.errors ?? {}, problem.detail ?? null);
  } catch {
    return new ApiError(response.status);
  }
}

export async function api<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const retryOn401 = options.retryOn401 ?? true;

  let response = await send(path, options);
  if (response.status === 401 && retryOn401 && (await refreshSession())) {
    response = await send(path, options);
  }

  if (!response.ok) {
    throw await toApiError(response);
  }

  const text = await response.text();
  return (text ? JSON.parse(text) : undefined) as T;
}
