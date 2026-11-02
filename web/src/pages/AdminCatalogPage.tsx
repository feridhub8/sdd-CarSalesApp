import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useState, type FormEvent } from 'react';
import { useSearchParams } from 'react-router';
import {
  createCatalogEntry,
  deleteCatalogEntry,
  getBrands,
  getCatalogWithDeleted,
  restoreCatalogEntry,
  updateCatalogEntry,
  type CatalogEntry,
  type CatalogKind,
} from '../api/catalog';
import { ApiError } from '../api/http';
import { AdminTabs } from '../components/AdminTabs';
import { Alert } from '../components/Alert';
import { Button } from '../components/Button';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { ErrorRetry } from '../components/ErrorRetry';
import { Field } from '../components/Field';
import { Loading } from '../components/Loading';
import { SelectField } from '../components/SelectField';
import { az } from '../i18n/az';

const KINDS: CatalogKind[] = ['Brand', 'Model', 'City'];

// Picks the message for a failed action. The API's own messages are in English and never shown.
function messageFor(failure: unknown, kind: CatalogKind, action: 'save' | 'delete' | 'restore') {
  const errors = az.admin.errors;
  if (!(failure instanceof ApiError)) {
    return az.errors.unavailable;
  }
  if (failure.status === 400) {
    return 'brandId' in failure.fieldErrors ? errors.brandUnavailable : errors.name;
  }
  if (failure.status === 404) {
    return errors.gone;
  }
  if (failure.status === 409) {
    if (action === 'save') {
      return errors.nameTaken;
    }
    if (action === 'restore') {
      return kind === 'Model' ? errors.restoreModel : errors.restoreBrandOrCity;
    }
    return kind === 'Brand' ? errors.brandHasModels : kind === 'Model' ? errors.modelHasListings : errors.cityHasListings;
  }
  return az.errors.unavailable;
}

function CatalogRow({
  kind,
  entry,
  busy,
  onRename,
  onDelete,
  onRestore,
}: {
  kind: CatalogKind;
  entry: CatalogEntry;
  busy: boolean;
  onRename: (name: string) => Promise<boolean>;
  onDelete: () => void;
  onRestore: () => void;
}) {
  const [editing, setEditing] = useState(false);
  const [name, setName] = useState(entry.name);
  const text = az.admin;

  const save = async (event: FormEvent) => {
    event.preventDefault();
    if (await onRename(name)) {
      setEditing(false);
    }
  };

  return (
    <li data-testid="catalog-row" data-name={entry.name} className="flex flex-wrap items-center justify-between gap-3 border-b border-line py-3 last:border-b-0">
      {editing ? (
        <form onSubmit={save} className="flex w-full flex-wrap items-end gap-2">
          <div className="min-w-0 flex-1 basis-48">
            <Field label={text.name} value={name} onChange={(event) => setName(event.target.value)} maxLength={100} autoFocus />
          </div>
          <Button type="submit" disabled={busy}>
            {az.common.save}
          </Button>
          <Button
            type="button"
            variant="secondary"
            onClick={() => {
              setName(entry.name);
              setEditing(false);
            }}
          >
            {az.common.cancel}
          </Button>
        </form>
      ) : (
        <>
          <div className="min-w-0">
            <span className={`font-medium break-words ${entry.isDeleted ? 'text-muted line-through' : ''}`}>{entry.name}</span>
            {kind === 'Model' && entry.brandName ? <span className="ml-2 text-sm text-muted">{entry.brandName}</span> : null}
            {entry.isDeleted ? <span className="ml-2 rounded-full border border-line px-2 py-0.5 text-xs text-muted">{text.deleted}</span> : null}
          </div>
          <div className="flex flex-wrap gap-2">
            {entry.isDeleted ? (
              <Button variant="secondary" disabled={busy} onClick={onRestore}>
                {text.restore}
              </Button>
            ) : (
              <>
                <Button variant="secondary" disabled={busy} onClick={() => setEditing(true)}>
                  {text.rename}
                </Button>
                <Button variant="danger" disabled={busy} onClick={onDelete}>
                  {az.common.delete}
                </Button>
              </>
            )}
          </div>
        </>
      )}
    </li>
  );
}

function CatalogSection({ kind }: { kind: CatalogKind }) {
  const queryClient = useQueryClient();
  const entries = useQuery({ queryKey: ['admin-catalog', kind], queryFn: () => getCatalogWithDeleted(kind) });
  const brands = useQuery({ queryKey: ['brands'], queryFn: getBrands, enabled: kind === 'Model' });

  const [name, setName] = useState('');
  const [brandId, setBrandId] = useState('');
  const [filter, setFilter] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [deleting, setDeleting] = useState<CatalogEntry | null>(null);
  const text = az.admin;

  // Everything that shows catalog entries refreshes, including the listing form's lists.
  const refresh = async () => {
    await Promise.all(
      [['admin-catalog'], ['brands'], ['cities'], ['models'], ['listings'], ['my-listings'], ['admin-listings']].map((queryKey) => queryClient.invalidateQueries({ queryKey })),
    );
  };

  const run = async (action: () => Promise<unknown>, type: 'save' | 'delete' | 'restore') => {
    setBusy(true);
    setError(null);
    try {
      await action();
      await refresh();
      return true;
    } catch (failure) {
      setError(messageFor(failure, kind, type));
      await refresh();
      return false;
    } finally {
      setBusy(false);
    }
  };

  const add = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);
    if (kind === 'Model' && !brandId) {
      setError(text.errors.chooseBrand);
      return;
    }
    if (!name.trim()) {
      setError(text.errors.name);
      return;
    }
    if (await run(() => createCatalogEntry(kind, name.trim(), kind === 'Model' ? brandId : undefined), 'save')) {
      setName('');
    }
  };

  if (entries.isPending) {
    return <Loading />;
  }
  if (entries.isError) {
    return <ErrorRetry onRetry={() => entries.refetch()} />;
  }

  const brandOptions = (brands.data ?? []).map((brand) => ({ value: brand.id, label: brand.name }));
  const visible = kind === 'Model' && filter ? entries.data.filter((entry) => entry.brandId === filter) : entries.data;

  return (
    <div className="rounded-xl border border-line bg-surface p-5">
      <form onSubmit={add} noValidate className="flex flex-wrap items-end gap-3">
        {kind === 'Model' ? (
          <div className="min-w-0 flex-1 basis-48">
            <SelectField label={text.brand} value={brandId} onChange={(event) => setBrandId(event.target.value)} options={brandOptions} />
          </div>
        ) : null}
        <div className="min-w-0 flex-1 basis-48">
          <Field label={text.name} value={name} onChange={(event) => setName(event.target.value)} maxLength={100} />
        </div>
        <Button type="submit" disabled={busy}>
          {text.add}
        </Button>
      </form>

      {error ? (
        <div className="mt-4">
          <Alert kind="error">{error}</Alert>
        </div>
      ) : null}

      {kind === 'Model' ? (
        <div className="mt-4 max-w-xs">
          <SelectField label={text.filterByBrand} placeholder={text.allBrands} value={filter} onChange={(event) => setFilter(event.target.value)} options={brandOptions} />
        </div>
      ) : null}

      {visible.length === 0 ? (
        <p className="py-8 text-center text-muted">{text.empty}</p>
      ) : (
        <ul className="mt-4">
          {visible.map((entry) => (
            <CatalogRow
              key={entry.id}
              kind={kind}
              entry={entry}
              busy={busy}
              onRename={(next) => run(() => updateCatalogEntry(kind, entry.id, next.trim(), kind === 'Model' ? entry.brandId : undefined), 'save')}
              onDelete={() => setDeleting(entry)}
              onRestore={() => run(() => restoreCatalogEntry(kind, entry.id), 'restore')}
            />
          ))}
        </ul>
      )}

      <ConfirmDialog
        open={deleting !== null}
        text={deleting ? text.confirmDelete(deleting.name) : ''}
        onCancel={() => setDeleting(null)}
        onConfirm={() => {
          const entry = deleting;
          setDeleting(null);
          if (entry) {
            run(() => deleteCatalogEntry(kind, entry.id), 'delete');
          }
        }}
      />
    </div>
  );
}

export function AdminCatalogPage() {
  const [params, setParams] = useSearchParams();
  const requested = params.get('kind') as CatalogKind | null;
  const kind = requested && KINDS.includes(requested) ? requested : 'Brand';

  return (
    <section>
      <AdminTabs />
      <h1 className="text-2xl font-bold">{az.admin.catalogTitle}</h1>
      <div className="my-4 flex flex-wrap gap-2">
        {KINDS.map((item) => (
          <Button key={item} variant={item === kind ? 'primary' : 'secondary'} onClick={() => setParams({ kind: item })}>
            {az.admin.kinds[item]}
          </Button>
        ))}
      </div>
      {/* A new section for each kind, so a half-typed name or an old error does not carry over. */}
      <CatalogSection key={kind} kind={kind} />
    </section>
  );
}
