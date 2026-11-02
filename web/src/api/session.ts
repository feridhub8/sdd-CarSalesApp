// The access token stays in this variable and nowhere else, so a script cannot find a saved copy.
// The refresh token is in an HttpOnly cookie, out of reach of scripts.
let accessToken: string | null = null;
let refreshing: Promise<boolean> | null = null;
const listeners = new Set<() => void>();

const REFRESH_URL = '/api/v1/Auth/Refresh';
const REFRESH_LOCK = 'carsales-session-refresh';

export function getAccessToken() {
  return accessToken;
}

export function setAccessToken(token: string | null) {
  accessToken = token;
  listeners.forEach((listener) => listener());
}

export function onSessionChange(listener: () => void) {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

async function requestRefresh(): Promise<boolean> {
  const response = await fetch(REFRESH_URL, { method: 'POST' });
  if (response.ok) {
    const tokens = (await response.json()) as { accessToken: string };
    setAccessToken(tokens.accessToken);
    return true;
  }

  // Only a real answer ends the session; a service being down must not sign the person out.
  if (response.status === 401) {
    setAccessToken(null);
    return false;
  }
  throw new Error(`Refresh failed with status ${response.status}`);
}

// A refresh token works once, and reusing one ends the login. So parallel calls in a tab share one
// request, and tabs take turns: the browser sends each tab the cookie the previous tab just received.
export function refreshSession(): Promise<boolean> {
  refreshing ??= (navigator.locks ? navigator.locks.request(REFRESH_LOCK, requestRefresh) : requestRefresh()).finally(() => {
    refreshing = null;
  });
  return refreshing;
}

// The role only decides what the site shows; the APIs enforce it on every request.
export function tokenHasRole(role: string) {
  if (!accessToken) {
    return false;
  }
  try {
    const payload = accessToken.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    const bytes = Uint8Array.from(atob(payload), (char) => char.charCodeAt(0));
    const claims = JSON.parse(new TextDecoder().decode(bytes)) as { realm_access?: { roles?: string[] } };
    return claims.realm_access?.roles?.includes(role) === true;
  } catch {
    return false;
  }
}
