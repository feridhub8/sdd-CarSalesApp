import { useQueryClient } from '@tanstack/react-query';
import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react';
import * as authApi from '../api/auth';
import { ApiError } from '../api/http';
import { getAccessToken, onSessionChange, refreshSession, setAccessToken, tokenHasRole } from '../api/session';

type AuthState =
  | { status: 'loading' }
  | { status: 'signedOut' }
  | { status: 'signedIn'; user: authApi.User; isAdmin: boolean };

type AuthContextValue = {
  auth: AuthState;
  signIn: (login: string, password: string) => Promise<void>;
  signOut: () => Promise<void>;
  // Saves a new number on the account and updates everything that shows it.
  savePhone: (phone: string) => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

async function loadSignedIn(): Promise<AuthState> {
  const user = await authApi.getMe();
  return { status: 'signedIn', user, isAdmin: tokenHasRole('admin') };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState<AuthState>({ status: 'loading' });
  const queryClient = useQueryClient();

  // Finds out on load whether the cookie still holds a session. Failing to reach the API counts as
  // signed out for now, but keeps the cookie, so a reload signs the person back in.
  useEffect(() => {
    let active = true;
    (async () => {
      try {
        const next = (await refreshSession()) ? await loadSignedIn() : ({ status: 'signedOut' } as const);
        if (active) {
          setAuth(next);
        }
      } catch {
        if (active) {
          setAuth({ status: 'signedOut' });
        }
      }
    })();
    return () => {
      active = false;
    };
  }, []);

  // A session that ends during an action (a refresh answered 401) shows as signed out at once.
  useEffect(
    () =>
      onSessionChange(() => {
        setAuth((current) => (current.status === 'signedIn' && !getAccessToken() ? { status: 'signedOut' } : current));
      }),
    [],
  );

  const signIn = useCallback(async (login: string, password: string) => {
    const tokens = await authApi.login(login, password);
    setAccessToken(tokens.accessToken);
    setAuth(await loadSignedIn());
    queryClient.clear();
  }, [queryClient]);

  const signOut = useCallback(async () => {
    try {
      await authApi.logout();
    } catch (error) {
      // The session is already gone on 400 and 401; anything else may still be alive, so say so.
      if (!(error instanceof ApiError) || (error.status !== 400 && error.status !== 401)) {
        throw error;
      }
    }
    setAccessToken(null);
    setAuth({ status: 'signedOut' });
    queryClient.clear();
  }, [queryClient]);

  const savePhone = useCallback(async (phone: string) => {
    const user = await authApi.updatePhone(phone);
    setAuth((current) => (current.status === 'signedIn' ? { ...current, user } : current));
    await queryClient.invalidateQueries({ queryKey: ['seller'] });
  }, [queryClient]);

  return <AuthContext.Provider value={{ auth, signIn, signOut, savePhone }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) {
    throw new Error('useAuth must be used inside AuthProvider');
  }
  return value;
}
