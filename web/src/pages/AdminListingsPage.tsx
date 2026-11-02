import { keepPreviousData, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link, useSearchParams } from 'react-router';
import { ApiError } from '../api/http';
import { deleteListing, getListingsWithDeleted, LISTINGS_PAGE_SIZE, restoreListing, type ListingWithDeleted } from '../api/listings';
import { AdminTabs } from '../components/AdminTabs';
import { Alert } from '../components/Alert';
import { Button } from '../components/Button';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { ErrorRetry } from '../components/ErrorRetry';
import { ImagePlaceholder } from '../components/ImagePlaceholder';
import { Loading } from '../components/Loading';
import { Pagination, readPage } from '../components/Pagination';
import { formatPrice } from '../format';
import { az } from '../i18n/az';
import { failureMessage } from '../errors';

const MAX_PAGE = 500;

export function AdminListingsPage() {
  const [params] = useSearchParams();
  const page = readPage(params.get('page'), MAX_PAGE);
  const queryClient = useQueryClient();
  const listings = useQuery({ queryKey: ['admin-listings', page], queryFn: () => getListingsWithDeleted(page), placeholderData: keepPreviousData });
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [deleting, setDeleting] = useState<ListingWithDeleted | null>(null);
  const text = az.admin;

  const refresh = async () => {
    await Promise.all([['admin-listings'], ['listings'], ['my-listings'], ['listing']].map((queryKey) => queryClient.invalidateQueries({ queryKey })));
  };

  const run = async (action: () => Promise<unknown>, onConflict: string) => {
    setBusy(true);
    setError(null);
    try {
      await action();
    } catch (failure) {
      const status = failure instanceof ApiError ? failure.status : 0;
      setError(status === 409 ? onConflict : status === 404 ? text.errors.gone : failureMessage(failure));
    } finally {
      await refresh();
      setBusy(false);
    }
  };

  const pageCount = Math.min(Math.ceil((listings.data?.totalCount ?? 0) / LISTINGS_PAGE_SIZE), MAX_PAGE);

  return (
    <section>
      <AdminTabs />
      <h1 className="text-2xl font-bold">{text.listingsTitle}</h1>
      <p className="mt-1 text-muted">{text.listingsHint}</p>
      {error ? (
        <div className="mt-4">
          <Alert kind="error">{error}</Alert>
        </div>
      ) : null}
      {listings.isPending ? <Loading /> : null}
      {listings.isError ? <ErrorRetry error={listings.error} onRetry={() => listings.refetch()} /> : null}
      {listings.isSuccess && listings.data.items.length === 0 ? <p className="py-16 text-center text-muted">{text.noListings}</p> : null}
      {listings.isSuccess && listings.data.items.length > 0 ? (
        <>
          <ul className="mt-6 space-y-3">
            {listings.data.items.map((listing) => {
              const title = `${listing.brandName} ${listing.modelName}`;
              return (
                <li key={listing.id} data-testid="admin-listing" data-id={listing.id} className="flex flex-wrap items-center gap-4 rounded-xl border border-line bg-surface p-3">
                  <div className="h-20 w-28 flex-none overflow-hidden rounded-lg bg-line">
                    {listing.coverImageUrl ? <img src={listing.coverImageUrl} alt={title} loading="lazy" className="h-full w-full object-cover" /> : <ImagePlaceholder />}
                  </div>
                  <div className="min-w-0 flex-1 basis-40">
                    {listing.isDeleted ? (
                      <p className="font-medium break-words text-muted line-through">{title}</p>
                    ) : (
                      <Link to={`/listings/${listing.id}`} className="font-medium break-words hover:text-accent">
                        {title}
                      </Link>
                    )}
                    <p className="mt-1 text-sm text-muted">
                      {formatPrice(listing.price, listing.currency)}, {listing.year}, {listing.cityName}
                    </p>
                    {listing.isDeleted ? <span className="mt-1 inline-block rounded-full border border-line px-2 py-0.5 text-xs text-muted">{text.deleted}</span> : null}
                  </div>
                  {listing.isDeleted ? (
                    <Button variant="secondary" disabled={busy} onClick={() => run(() => restoreListing(listing.id), text.errors.restoreListing)}>
                      {text.restore}
                    </Button>
                  ) : (
                    <Button variant="danger" disabled={busy} onClick={() => setDeleting(listing)}>
                      {az.common.delete}
                    </Button>
                  )}
                </li>
              );
            })}
          </ul>
          <Pagination page={page} pageCount={pageCount} toPage={(target) => (target === 1 ? '/admin/listings' : `/admin/listings?page=${target}`)} />
        </>
      ) : null}
      <ConfirmDialog
        open={deleting !== null}
        text={text.confirmDeleteListing}
        onCancel={() => setDeleting(null)}
        onConfirm={() => {
          const listing = deleting;
          setDeleting(null);
          if (listing) {
            run(() => deleteListing(listing.id), az.errors.unavailable);
          }
        }}
      />
    </section>
  );
}
