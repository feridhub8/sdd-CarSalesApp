import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { useEffect } from 'react';
import { Link, useSearchParams } from 'react-router';
import { getListings, LISTINGS_PAGE_SIZE } from '../api/listings';
import { ErrorRetry } from '../components/ErrorRetry';
import { ListingCard } from '../components/ListingCard';
import { Loading } from '../components/Loading';
import { Pagination, readPage } from '../components/Pagination';
import { az } from '../i18n/az';

// The API accepts pages up to this number.
const MAX_PAGE = 500;

export function HomePage() {
  const [params] = useSearchParams();
  const page = readPage(params.get('page'), MAX_PAGE);

  const listings = useQuery({
    queryKey: ['listings', page],
    queryFn: () => getListings(page),
    placeholderData: keepPreviousData,
  });

  useEffect(() => {
    window.scrollTo(0, 0);
  }, [page]);

  const pageCount = Math.min(Math.ceil((listings.data?.totalCount ?? 0) / LISTINGS_PAGE_SIZE), MAX_PAGE);

  return (
    <section>
      <h1 className="text-2xl font-bold">{az.home.title}</h1>
      {listings.isPending ? <Loading /> : null}
      {listings.isError ? <ErrorRetry error={listings.error} onRetry={() => listings.refetch()} /> : null}
      {listings.isSuccess && listings.data.totalCount === 0 ? <p className="py-16 text-center text-muted">{az.home.empty}</p> : null}
      {listings.isSuccess && listings.data.totalCount > 0 && listings.data.items.length === 0 ? (
        <p className="py-16 text-center text-muted">
          {az.home.pastEnd}{' '}
          <Link to="/" className="font-medium text-accent hover:text-accent-strong">
            {az.home.toFirstPage}
          </Link>
        </p>
      ) : null}
      {listings.isSuccess && listings.data.items.length > 0 ? (
        <>
          <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {listings.data.items.map((listing) => (
              <ListingCard key={listing.id} listing={listing} />
            ))}
          </div>
          <Pagination page={page} pageCount={pageCount} toPage={(target) => (target === 1 ? '/' : `/?page=${target}`)} />
        </>
      ) : null}
    </section>
  );
}
