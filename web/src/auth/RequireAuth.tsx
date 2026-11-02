import { Navigate, Outlet, useLocation } from 'react-router';
import { Loading } from '../components/Loading';
import { useAuth } from './AuthContext';

// Sends signed-out visitors to the sign-in page, which brings them back afterwards.
export function RequireAuth() {
  const { auth } = useAuth();
  const location = useLocation();

  if (auth.status === 'loading') {
    return <Loading />;
  }
  if (auth.status === 'signedOut') {
    const next = encodeURIComponent(location.pathname + location.search);
    return <Navigate to={`/login?next=${next}`} replace />;
  }
  return <Outlet />;
}

// Only paths on this site, so a crafted link cannot send someone elsewhere after signing in.
export function safeNext(next: string | null) {
  return next && next.startsWith('/') && !next.startsWith('//') && !next.startsWith('/\\') ? next : '/';
}
