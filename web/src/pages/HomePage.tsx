import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router';
import { getListings, LISTINGS_PAGE_SIZE } from '../api/listings';
import { Button } from '../components/Button';
import { ErrorRetry } from '../components/ErrorRetry';
import { FilterPanel } from '../components/FilterPanel';
import { ListingCard } from '../components/ListingCard';
import { Loading } from '../components/Loading';
import { Pagination, readPage } from '../components/Pagination';
import { SelectField } from '../components/SelectField';
import { formatNumber } from '../format';
import { az } from '../i18n/az';
import { activeFilterCount, DEFAULT_SORT, readSearch, searchAddress, SORTS, toApiQuery } from '../search';

// The API accepts pages up to this number.
const MAX_PAGE = 500;

export function HomePage() {
  const [address, setAddress] = useSearchParams();
  const search = readSearch(address);
  const page = readPage(address.get('page'), MAX_PAGE);
  const apiQuery = toApiQuery(search, page);

  // Clearing starts the panel afresh, so a number still being typed does not come back.
  const [panelKey, setPanelKey] = useState(0);

  const listings = useQuery({
    queryKey: ['listings', apiQuery],
    queryFn: () => getListings(apiQuery),
    placeholderData: keepPreviousData,
  });

  useEffect(() => {
    window.scrollTo(0, 0);
  }, [page]);

  const pageCount = Math.min(Math.ceil((listings.data?.totalCount ?? 0) / LISTINGS_PAGE_SIZE), MAX_PAGE);

  // Any change goes back to the first page, since the page is not part of the search.
  // Read from the address itself: two number fields can finish typing before the page renders in between.
  const edit = (change: (next: URLSearchParams) => void, options?: { replace?: boolean }) => {
    const next = readSearch(new URLSearchParams(window.location.search));
    change(next);
    setAddress(next, options);
  };

  const clearFilters = () => {
    const next = new URLSearchParams();
    const sort = search.get('sort');
    if (sort) {
      next.set('sort', sort);
    }

    setAddress(next);
    setPanelKey((key) => key + 1);
  };

  return (
    <section>
      <h1 className="text-2xl font-bold">{az.home.title}</h1>
      <div className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-[17rem_minmax(0,1fr)]">
        <FilterPanel key={panelKey} search={search} onEdit={edit} onClear={clearFilters} />
        <div className="min-w-0">
          <div className="flex flex-wrap items-end justify-between gap-3">
            <p data-testid="result-count" className="min-h-6 font-medium">
              {listings.data ? az.search.count(formatNumber(listings.data.totalCount)) : ''}
            </p>
            <div className="w-full sm:w-56">
              <SelectField
                label={az.search.sort}
                placeholder={null}
                value={search.get('sort') ?? DEFAULT_SORT}
                options={SORTS.map((sort) => ({ value: sort, label: az.search.sorts[sort] }))}
                onChange={(event) =>
                  edit((next) => {
                    if (event.target.value === DEFAULT_SORT) {
                      next.delete('sort');
                    } else {
                      next.set('sort', event.target.value);
                    }
                  })
                }
              />
            </div>
          </div>
          {listings.isPending ? <Loading /> : null}
          {listings.isError ? <ErrorRetry error={listings.error} onRetry={() => listings.refetch()} /> : null}
          {listings.isSuccess && listings.data.totalCount === 0 && activeFilterCount(search) === 0 ? (
            <p className="py-16 text-center text-muted">{az.home.empty}</p>
          ) : null}
          {listings.isSuccess && listings.data.totalCount === 0 && activeFilterCount(search) > 0 ? (
            <div className="py-16 text-center">
              <p className="mb-4 text-muted">{az.search.noMatch}</p>
              <Button variant="secondary" onClick={clearFilters}>
                {az.search.clear}
              </Button>
            </div>
          ) : null}
          {listings.isSuccess && listings.data.totalCount > 0 && listings.data.items.length === 0 ? (
            <p className="py-16 text-center text-muted">
              {az.home.pastEnd}{' '}
              <Link to={searchAddress(search, 1)} className="font-medium text-accent hover:text-accent-strong">
                {az.home.toFirstPage}
              </Link>
            </p>
          ) : null}
          {listings.isSuccess && listings.data.items.length > 0 ? (
            <div aria-busy={listings.isFetching} className={listings.isPlaceholderData ? 'opacity-60' : undefined}>
              <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
                {listings.data.items.map((listing) => (
                  <ListingCard key={listing.id} listing={listing} />
                ))}
              </div>
              <Pagination page={page} pageCount={pageCount} toPage={(target) => searchAddress(search, target)} />
            </div>
          ) : null}
        </div>
      </div>
    </section>
  );
}
