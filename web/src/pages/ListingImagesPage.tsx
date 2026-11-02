import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useRef, useState, type ChangeEvent } from 'react';
import { Link, useParams } from 'react-router';
import { ApiError } from '../api/http';
import { deleteImage, getListing, reorderImages, uploadImage, type Listing, type ListingImage } from '../api/listings';
import { useAuth } from '../auth/AuthContext';
import { Alert } from '../components/Alert';
import { Button } from '../components/Button';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { ErrorRetry } from '../components/ErrorRetry';
import { Loading } from '../components/Loading';
import { az } from '../i18n/az';
import { NotAllowedPage } from './NotAllowedPage';
import { NotFoundPage } from './NotFoundPage';

const MAX_IMAGES = 20;
const MAX_FILE_BYTES = 10 * 1024 * 1024;
const ACCEPTED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

type UploadState = {
  key: number;
  name: string;
  state: 'waiting' | 'uploading' | 'done' | 'failed';
  message: string;
};

function ImageTile({
  image,
  index,
  count,
  busy,
  onMove,
  onDelete,
}: {
  image: ListingImage;
  index: number;
  count: number;
  busy: boolean;
  onMove: (from: number, to: number) => void;
  onDelete: () => void;
}) {
  const text = az.images;
  const small = 'rounded-lg border border-line bg-surface px-2 py-1.5 text-sm font-medium hover:bg-page disabled:cursor-not-allowed disabled:opacity-50';

  return (
    <li data-testid="image-tile" data-image-id={image.id} className="overflow-hidden rounded-xl border border-line bg-surface">
      <div className="relative aspect-[4/3] bg-line">
        <img src={image.thumbnailUrl} alt={text.position(index + 1)} className="h-full w-full object-cover" />
        {index === 0 ? <span className="absolute top-2 left-2 rounded-full bg-accent px-3 py-1 text-xs font-medium text-white">{text.cover}</span> : null}
      </div>
      <div className="flex flex-wrap gap-2 p-3">
        <button type="button" className={small} disabled={busy || index === 0} onClick={() => onMove(index, index - 1)}>
          {text.moveBack}
        </button>
        <button type="button" className={small} disabled={busy || index === count - 1} onClick={() => onMove(index, index + 1)}>
          {text.moveForward}
        </button>
        {index > 0 ? (
          <button type="button" className={small} disabled={busy} onClick={() => onMove(index, 0)}>
            {text.makeCover}
          </button>
        ) : null}
        <button type="button" className={`${small} text-danger`} disabled={busy} onClick={onDelete}>
          {az.common.delete}
        </button>
      </div>
    </li>
  );
}

function ImageManager({ listing }: { listing: Listing }) {
  const queryClient = useQueryClient();
  const input = useRef<HTMLInputElement>(null);
  const nextKey = useRef(0);
  const [uploads, setUploads] = useState<UploadState[]>([]);
  const [uploading, setUploading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [actionFailed, setActionFailed] = useState(false);
  const [deleting, setDeleting] = useState<ListingImage | null>(null);

  const text = az.images;
  const images = listing.images ?? [];
  const busy = uploading || saving;

  const refresh = async () => {
    await queryClient.invalidateQueries({ queryKey: ['listing', listing.id] });
    await queryClient.invalidateQueries({ queryKey: ['listings'] });
    await queryClient.invalidateQueries({ queryKey: ['my-listings'] });
  };

  const addFiles = async (event: ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(event.target.files ?? []);
    event.target.value = '';
    setActionFailed(false);

    // Refused here without sending: the wrong type, too large, or past the limit.
    let free = MAX_IMAGES - images.length;
    const planned = files.map((file) => {
      const key = nextKey.current++;
      if (!ACCEPTED_TYPES.includes(file.type)) {
        return { file, upload: { key, name: file.name, state: 'failed', message: text.status.wrongType } as UploadState };
      }
      if (file.size > MAX_FILE_BYTES) {
        return { file, upload: { key, name: file.name, state: 'failed', message: text.status.tooLarge } as UploadState };
      }
      if (free <= 0) {
        return { file, upload: { key, name: file.name, state: 'failed', message: text.status.limit } as UploadState };
      }
      free--;
      return { file, upload: { key, name: file.name, state: 'waiting', message: text.status.waiting } as UploadState };
    });
    setUploads(planned.map((item) => item.upload));

    const update = (key: number, state: UploadState['state'], message: string) => {
      setUploads((current) => current.map((item) => (item.key === key ? { ...item, state, message } : item)));
    };

    // One at a time, so the images land in the order selected.
    setUploading(true);
    for (const { file, upload } of planned) {
      if (upload.state !== 'waiting') {
        continue;
      }
      update(upload.key, 'uploading', text.status.uploading);
      try {
        await uploadImage(listing.id, file);
        update(upload.key, 'done', text.status.done);
        await refresh();
      } catch (failure) {
        const status = failure instanceof ApiError ? failure.status : 0;
        update(upload.key, 'failed', status === 400 || status === 413 ? text.status.rejected : status === 409 ? text.status.limit : text.status.unavailable);
      }
    }
    setUploading(false);
  };

  const move = async (from: number, to: number) => {
    const ordered = [...images];
    const [moved] = ordered.splice(from, 1);
    ordered.splice(to, 0, moved);

    setSaving(true);
    setActionFailed(false);
    try {
      await reorderImages(listing.id, ordered.map((image) => image.id));
      await refresh();
    } catch {
      setActionFailed(true);
      await refresh();
    } finally {
      setSaving(false);
    }
  };

  const remove = async () => {
    const image = deleting;
    setDeleting(null);
    if (!image) {
      return;
    }

    setSaving(true);
    setActionFailed(false);
    try {
      await deleteImage(image.id);
    } catch {
      setActionFailed(true);
    } finally {
      await refresh();
      setSaving(false);
    }
  };

  return (
    <section>
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="min-w-0">
          <h1 className="text-2xl font-bold">{text.title}</h1>
          <p className="mt-1 break-words text-muted">
            {listing.brandName} {listing.modelName}
          </p>
        </div>
        <Link to={`/listings/${listing.id}`} className="rounded-lg border border-line bg-surface px-4 py-2 font-medium hover:bg-page">
          {text.toListing}
        </Link>
      </div>

      <div className="mt-6 rounded-xl border border-line bg-surface p-5">
        {images.length >= MAX_IMAGES ? (
          <Alert kind="error">{text.limitReached}</Alert>
        ) : (
          <>
            <input ref={input} data-testid="image-input" type="file" accept={ACCEPTED_TYPES.join(',')} multiple onChange={addFiles} disabled={busy} className="sr-only" />
            <Button disabled={busy} onClick={() => input.current?.click()}>
              {text.add}
            </Button>
            <p className="mt-2 text-sm text-muted">{text.hint}</p>
          </>
        )}
        {uploads.length > 0 ? (
          <ul className="mt-4 space-y-2">
            {uploads.map((upload) => (
              <li key={upload.key} data-testid="upload-row" data-state={upload.state} className="flex flex-wrap justify-between gap-x-4 gap-y-1 border-t border-line pt-2 text-sm">
                <span className="min-w-0 font-medium break-all">{upload.name}</span>
                <span className={upload.state === 'failed' ? 'text-danger' : upload.state === 'done' ? 'text-success' : 'text-muted'}>{upload.message}</span>
              </li>
            ))}
          </ul>
        ) : null}
      </div>

      {actionFailed ? (
        <div className="mt-4">
          <Alert kind="error">{az.errors.unavailable}</Alert>
        </div>
      ) : null}

      {images.length === 0 ? (
        <p className="py-12 text-center text-muted">{text.empty}</p>
      ) : (
        <ul className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {images.map((image, index) => (
            <ImageTile key={image.id} image={image} index={index} count={images.length} busy={busy} onMove={move} onDelete={() => setDeleting(image)} />
          ))}
        </ul>
      )}

      <ConfirmDialog open={deleting !== null} text={text.confirmDelete} onConfirm={remove} onCancel={() => setDeleting(null)} />
    </section>
  );
}

export function ListingImagesPage() {
  const { id = '' } = useParams();
  const { auth } = useAuth();
  const listing = useQuery({ queryKey: ['listing', id], queryFn: () => getListing(id) });

  if (listing.isPending) {
    return <Loading />;
  }
  if (listing.isError) {
    const status = listing.error instanceof ApiError ? listing.error.status : 0;
    return status === 404 || status === 400 ? <NotFoundPage /> : <ErrorRetry onRetry={() => listing.refetch()} />;
  }
  if (auth.status !== 'signedIn' || auth.user.id !== listing.data.ownerId) {
    return <NotAllowedPage />;
  }
  return <ImageManager listing={listing.data} />;
}
