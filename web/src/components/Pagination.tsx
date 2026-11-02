import { Link } from 'react-router';
import { az } from '../i18n/az';

const LINK = 'rounded-lg border border-line bg-surface px-4 py-2 font-medium hover:bg-page';

export function Pagination({ page, pageCount, toPage }: { page: number; pageCount: number; toPage: (page: number) => string }) {
  if (pageCount <= 1) {
    return null;
  }

  return (
    <nav aria-label={az.pagination.label} className="mt-8 flex items-center justify-center gap-3">
      {page > 1 ? (
        <Link to={toPage(page - 1)} className={LINK}>
          {az.pagination.previous}
        </Link>
      ) : null}
      <span className="text-sm text-muted">{az.pagination.position(page, pageCount)}</span>
      {page < pageCount ? (
        <Link to={toPage(page + 1)} className={LINK}>
          {az.pagination.next}
        </Link>
      ) : null}
    </nav>
  );
}

// The page from the address. Anything the API would refuse falls back to a page it accepts.
export function readPage(value: string | null, maxPage: number) {
  const page = Number(value);
  return Number.isInteger(page) && page >= 1 ? Math.min(page, maxPage) : 1;
}
