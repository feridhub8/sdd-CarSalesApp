import { useState } from 'react';
import { Link, Outlet, useNavigate } from 'react-router';
import { useAuth } from '../auth/AuthContext';
import { az } from '../i18n/az';

export function Layout() {
  const { auth, signOut } = useAuth();
  const navigate = useNavigate();
  const [signOutFailed, setSignOutFailed] = useState(false);

  const handleSignOut = async () => {
    setSignOutFailed(false);
    try {
      await signOut();
      navigate('/');
    } catch {
      setSignOutFailed(true);
    }
  };

  return (
    <div className="flex min-h-screen flex-col">
      <header className="border-b border-line bg-surface">
        <div className="mx-auto flex min-h-16 w-full max-w-6xl flex-wrap items-center justify-between gap-x-4 gap-y-2 px-4 py-3">
          <Link to="/" className="text-xl font-bold tracking-tight text-accent">
            {az.brand}
          </Link>
          <nav className="flex flex-wrap items-center gap-x-4 gap-y-2 text-sm font-medium">
            <Link to="/" className="hover:text-accent">
              {az.nav.listings}
            </Link>
            {auth.status === 'signedIn' ? (
              <>
                <Link to="/my-listings" className="hover:text-accent">
                  {az.nav.myListings}
                </Link>
                <Link to="/listings/new" className="rounded-lg bg-accent px-3 py-1.5 text-white hover:bg-accent-strong">
                  {az.nav.newListing}
                </Link>
                {auth.isAdmin ? (
                  <Link to="/admin/catalog" className="hover:text-accent">
                    {az.nav.admin}
                  </Link>
                ) : null}
                <Link to="/profile" className="max-w-40 truncate text-muted hover:text-accent" data-testid="current-user">
                  {auth.user.username}
                </Link>
                <button type="button" onClick={handleSignOut} className="font-medium hover:text-accent">
                  {az.nav.signOut}
                </button>
              </>
            ) : auth.status === 'signedOut' ? (
              <>
                <Link to="/login" className="hover:text-accent">
                  {az.nav.signIn}
                </Link>
                <Link to="/register" className="rounded-lg bg-accent px-3 py-1.5 text-white hover:bg-accent-strong">
                  {az.nav.register}
                </Link>
              </>
            ) : null}
          </nav>
        </div>
      </header>
      {signOutFailed ? (
        <div role="alert" className="border-b border-line bg-surface px-4 py-3 text-center text-sm text-danger">
          {az.errors.unavailable}
        </div>
      ) : null}
      <main className="mx-auto w-full max-w-6xl flex-1 px-4 py-8">
        <Outlet />
      </main>
      <footer className="border-t border-line bg-surface">
        <div className="mx-auto w-full max-w-6xl px-4 py-6 text-sm text-muted">{az.footer}</div>
      </footer>
    </div>
  );
}
