import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { Link, useSearchParams } from 'react-router';
import { getMyListings, LISTINGS_PAGE_SIZE } from '../api/listings';
import { ErrorRetry } from '../components/ErrorRetry';
import { ListingCard } from '../components/ListingCard';
import { Loading } from '../components/Loading';
import { Pagination, readPage } from '../components/Pagination';
import { az } from '../i18n/az';

const MAX_PAGE = 500;

export function MyListingsPage() {
  const [params] = useSearchParams();
  const page = readPage(params.get('page'), MAX_PAGE);
  const mine = useQuery({ queryKey: ['my-listings', page], queryFn: () => getMyListings(page), placeholderData: keepPreviousData });
  const pageCount = Math.min(Math.ceil((mine.data?.totalCount ?? 0) / LISTINGS_PAGE_SIZE), MAX_PAGE);

  return (
    <section>
      <h1 className="text-2xl font-bold">{az.myListings.title}</h1>
      {mine.isPending ? <Loading /> : null}
      {mine.isError ? <ErrorRetry error={mine.error} onRetry={() => mine.refetch()} /> : null}
      {mine.isSuccess && mine.data.items.length === 0 ? (
        <div className="py-16 text-center">
          <p className="text-muted">{az.myListings.empty}</p>
          <Link to="/listings/new" className="mt-4 inline-block rounded-lg bg-accent px-4 py-2 font-medium text-white hover:bg-accent-strong">
            {az.myListings.create}
          </Link>
        </div>
      ) : null}
      {mine.isSuccess && mine.data.items.length > 0 ? (
        <>
          <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {mine.data.items.map((listing) => (
              <ListingCard key={listing.id} listing={listing} />
            ))}
          </div>
          <Pagination page={page} pageCount={pageCount} toPage={(target) => (target === 1 ? '/my-listings' : `/my-listings?page=${target}`)} />
        </>
      ) : null}
    </section>
  );
}
