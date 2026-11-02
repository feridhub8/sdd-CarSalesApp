import { Link, Outlet } from 'react-router';
import { az } from '../i18n/az';

export function Layout() {
  return (
    <div className="flex min-h-screen flex-col">
      <header className="border-b border-line bg-surface">
        <div className="mx-auto flex h-16 w-full max-w-6xl items-center justify-between gap-4 px-4">
          <Link to="/" className="text-xl font-bold tracking-tight text-accent">
            {az.brand}
          </Link>
          <nav className="flex items-center gap-4 text-sm font-medium">
            <Link to="/" className="hover:text-accent">
              {az.nav.listings}
            </Link>
          </nav>
        </div>
      </header>
      <main className="mx-auto w-full max-w-6xl flex-1 px-4 py-8">
        <Outlet />
      </main>
      <footer className="border-t border-line bg-surface">
        <div className="mx-auto w-full max-w-6xl px-4 py-6 text-sm text-muted">{az.footer}</div>
      </footer>
    </div>
  );
}
