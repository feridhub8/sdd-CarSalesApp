import { useQuery } from '@tanstack/react-query';
import { API, api } from '../api/http';
import { Button } from '../components/Button';
import { Alert } from '../components/Alert';
import { Loading } from '../components/Loading';
import { az } from '../i18n/az';

type Page = { totalCount: number };

export function MyListingsPage() {
  const mine = useQuery({
    queryKey: ['my-listings'],
    queryFn: () => api<Page>(`${API}/Listing/Mine`),
  });

  return (
    <section>
      <h1 className="text-2xl font-bold">{az.myListings.title}</h1>
      {mine.isPending ? <Loading /> : null}
      {mine.isError ? (
        <div className="mt-4 space-y-3">
          <Alert kind="error">{az.errors.unavailable}</Alert>
          <Button variant="secondary" onClick={() => mine.refetch()}>
            {az.common.retry}
          </Button>
        </div>
      ) : null}
      {mine.isSuccess ? <p className="mt-2 text-muted">{az.myListings.count(mine.data.totalCount)}</p> : null}
    </section>
  );
}
