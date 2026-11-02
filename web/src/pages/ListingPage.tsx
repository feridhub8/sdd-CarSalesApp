import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { getSeller } from '../api/auth';
import { ApiError } from '../api/http';
import { deleteImage, deleteListing, getListing, type Listing, type ListingImage } from '../api/listings';
import { useAuth } from '../auth/AuthContext';
import { Alert } from '../components/Alert';
import { Button } from '../components/Button';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { ErrorRetry } from '../components/ErrorRetry';
import { ImagePlaceholder } from '../components/ImagePlaceholder';
import { Loading } from '../components/Loading';
import { formatDate, formatEngine, formatMileage, formatPhone, formatPrice, label } from '../format';
import { az } from '../i18n/az';
import { NotFoundPage } from './NotFoundPage';

function Gallery({ listing, images, canDelete }: { listing: Listing; images: ListingImage[]; canDelete: boolean }) {
  const queryClient = useQueryClient();
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [confirming, setConfirming] = useState(false);
  const [failed, setFailed] = useState(false);
  const selected = images.find((image) => image.id === selectedId) ?? images[0];

  const removeSelected = async () => {
    setConfirming(false);
    setFailed(false);
    if (!selected) {
      return;
    }
    try {
      await deleteImage(selected.id);
      setSelectedId(null);
    } catch {
      setFailed(true);
    } finally {
      await queryClient.invalidateQueries({ queryKey: ['listing', listing.id] });
      await queryClient.invalidateQueries({ queryKey: ['listings'] });
    }
  };
  const name = `${listing.brandName} ${listing.modelName}`;

  return (
    <div>
      <div className="aspect-[4/3] w-full overflow-hidden rounded-xl border border-line bg-line">
        {selected ? <img src={selected.url} alt={name} data-testid="gallery-main" className="h-full w-full object-contain" /> : <ImagePlaceholder />}
      </div>
      {images.length > 1 ? (
        <div className="mt-3 grid grid-cols-4 gap-2 sm:grid-cols-6">
          {images.map((image, index) => (
            <button
              key={image.id}
              type="button"
              onClick={() => setSelectedId(image.id)}
              aria-label={az.listing.showImage(index + 1)}
              aria-current={image.id === selected?.id}
              className={`aspect-[4/3] overflow-hidden rounded-lg border-2 ${image.id === selected?.id ? 'border-accent' : 'border-transparent'}`}
            >
              <img src={image.thumbnailUrl} alt="" className="h-full w-full object-cover" />
            </button>
          ))}
        </div>
      ) : null}
      {canDelete && selected ? (
        <div className="mt-3 space-y-3">
          {failed ? <Alert kind="error">{az.errors.unavailable}</Alert> : null}
          <Button variant="danger" onClick={() => setConfirming(true)}>
            {az.listing.adminDeleteImage}
          </Button>
          <ConfirmDialog open={confirming} text={az.listing.adminConfirmDeleteImage} onConfirm={removeSelected} onCancel={() => setConfirming(false)} />
        </div>
      ) : null}
    </div>
  );
}

function Fact({ name, value }: { name: string; value: string | number }) {
  return (
    <div className="flex justify-between gap-4 border-b border-line py-2 last:border-b-0">
      <dt className="text-muted">{name}</dt>
      <dd className="text-right font-medium">{value}</dd>
    </div>
  );
}

// Asks ms-auth only when pressed, so viewing a listing never depends on that service.
function SellerContact({ ownerId }: { ownerId: string }) {
  const [revealed, setRevealed] = useState(false);
  const seller = useQuery({ queryKey: ['seller', ownerId], queryFn: () => getSeller(ownerId), enabled: revealed });
  const text = az.seller;

  if (!revealed) {
    return (
      <div className="mt-4">
        <Button className="w-full" onClick={() => setRevealed(true)}>
          {text.show}
        </Button>
      </div>
    );
  }
  if (seller.isPending) {
    return <div className="mt-4 text-muted">{az.common.loading}</div>;
  }

  const unavailable = seller.isError && !(seller.error instanceof ApiError && seller.error.status === 404);
  if (unavailable) {
    return (
      <div className="mt-4 space-y-3">
        <Alert kind="error">{az.errors.unavailable}</Alert>
        <Button variant="secondary" className="w-full" onClick={() => seller.refetch()}>
          {az.common.retry}
        </Button>
      </div>
    );
  }

  return (
    <div data-testid="seller-contact" className="mt-4 rounded-lg border border-line p-4">
      {seller.data?.phone ? (
        <>
          <p className="text-sm text-muted">
            {text.title}: <span className="font-medium break-words text-ink">{seller.data.firstName}</span>
          </p>
          <a href={`tel:${seller.data.phone}`} className="mt-1 block text-xl font-bold text-accent hover:text-accent-strong">
            {formatPhone(seller.data.phone)}
          </a>
        </>
      ) : (
        <p className="text-muted">{text.none}</p>
      )}
    </div>
  );
}

function OwnerControls({ id }: { id: string }) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [confirming, setConfirming] = useState(false);
  const [failed, setFailed] = useState(false);
  const linkStyle = 'rounded-lg border border-line bg-surface px-4 py-2 text-center font-medium hover:bg-page';

  const remove = async () => {
    setConfirming(false);
    setFailed(false);
    try {
      await deleteListing(id);
      queryClient.removeQueries({ queryKey: ['listing', id] });
      await queryClient.invalidateQueries({ queryKey: ['listings'] });
      await queryClient.invalidateQueries({ queryKey: ['my-listings'] });
      navigate('/my-listings');
    } catch {
      setFailed(true);
    }
  };

  return (
    <div className="mt-4 space-y-3">
      {failed ? <Alert kind="error">{az.errors.unavailable}</Alert> : null}
      <div className="flex flex-wrap gap-2">
        <Link to={`/listings/${id}/edit`} className={linkStyle}>
          {az.listing.edit}
        </Link>
        <Link to={`/listings/${id}/images`} className={linkStyle}>
          {az.listing.manageImages}
        </Link>
        <Button variant="danger" onClick={() => setConfirming(true)}>
          {az.common.delete}
        </Button>
      </div>
      <ConfirmDialog open={confirming} text={az.listing.confirmDelete} onConfirm={remove} onCancel={() => setConfirming(false)} />
    </div>
  );
}

// An admin who is not the owner can remove a listing or its images, but cannot edit them.
function AdminControls({ id }: { id: string }) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [confirming, setConfirming] = useState(false);
  const [failed, setFailed] = useState(false);

  const remove = async () => {
    setConfirming(false);
    setFailed(false);
    try {
      await deleteListing(id);
      queryClient.removeQueries({ queryKey: ['listing', id] });
      await queryClient.invalidateQueries({ queryKey: ['listings'] });
      await queryClient.invalidateQueries({ queryKey: ['admin-listings'] });
      navigate('/admin/listings');
    } catch {
      setFailed(true);
    }
  };

  return (
    <div className="mt-4 space-y-3">
      {failed ? <Alert kind="error">{az.errors.unavailable}</Alert> : null}
      <Button variant="danger" onClick={() => setConfirming(true)}>
        {az.listing.adminDeleteListing}
      </Button>
      <ConfirmDialog open={confirming} text={az.listing.adminConfirmDelete} onConfirm={remove} onCancel={() => setConfirming(false)} />
    </div>
  );
}

export function ListingPage() {
  const { auth } = useAuth();
  const { id = '' } = useParams();
  const listing = useQuery({ queryKey: ['listing', id], queryFn: () => getListing(id) });

  const title = listing.data ? `${listing.data.brandName} ${listing.data.modelName}` : null;
  useEffect(() => {
    document.title = title ? `${title} | ${az.brand}` : az.brand;
    return () => {
      document.title = az.brand;
    };
  }, [title]);

  if (listing.isPending) {
    return <Loading />;
  }
  if (listing.isError) {
    const status = listing.error instanceof ApiError ? listing.error.status : 0;
    // An address that is not a valid id cannot name a listing either.
    return status === 404 || status === 400 ? <NotFoundPage /> : <ErrorRetry onRetry={() => listing.refetch()} />;
  }

  const item = listing.data;
  const isOwner = auth.status === 'signedIn' && auth.user.id === item.ownerId;
  const isOtherAdmin = auth.status === 'signedIn' && auth.isAdmin && !isOwner;
  const { fields, labels } = az.listing;
  const engine = formatEngine(item.engineVolume);
  const notes = [item.isCredit && fields.credit, item.isBarter && fields.barter, item.isPainted && fields.painted, item.isDamaged && fields.damaged].filter(
    (note): note is NonNullable<typeof note> & string => Boolean(note),
  );

  return (
    // Phone order: gallery, facts, description. Desktop keeps the description under the gallery.
    <article className="grid items-start gap-6 lg:grid-cols-[3fr_2fr] lg:grid-rows-[auto_1fr] lg:gap-x-8">
      <div className="min-w-0">
        <Gallery listing={item} images={item.images ?? []} canDelete={isOtherAdmin} />
      </div>
      <div className="min-w-0 lg:row-span-2">
        <div className="rounded-xl border border-line bg-surface p-5">
          <h1 className="text-2xl font-bold break-words">{title}</h1>
          <p className="mt-2 text-3xl font-bold text-accent">{formatPrice(item.price, item.currency)}</p>
          {notes.length > 0 ? (
            <ul className="mt-3 flex flex-wrap gap-2">
              {notes.map((note) => (
                <li key={note} className="rounded-full border border-line px-3 py-1 text-sm">
                  {note}
                </li>
              ))}
            </ul>
          ) : null}
          <SellerContact ownerId={item.ownerId} />
          <dl className="mt-4 text-sm">
            <Fact name={fields.city} value={item.cityName} />
            <Fact name={fields.year} value={item.year} />
            <Fact name={fields.mileage} value={formatMileage(item.mileage, item.mileageUnit)} />
            {engine ? <Fact name={fields.engine} value={engine} /> : null}
            <Fact name={fields.horsepower} value={item.horsepower} />
            <Fact name={fields.seats} value={item.seats} />
            <Fact name={fields.bodyType} value={label(labels.bodyType, item.bodyType)} />
            <Fact name={fields.fuelType} value={label(labels.fuelType, item.fuelType)} />
            <Fact name={fields.transmission} value={label(labels.transmission, item.transmission)} />
            <Fact name={fields.drivetrain} value={label(labels.drivetrain, item.drivetrain)} />
            <Fact name={fields.color} value={label(labels.color, item.color)} />
            <Fact name={fields.market} value={label(labels.market, item.market)} />
            {item.vin ? <Fact name={fields.vin} value={item.vin} /> : null}
            <Fact name={fields.createdAt} value={formatDate(item.createdAt)} />
          </dl>
          {isOwner ? <OwnerControls id={item.id} /> : null}
          {isOtherAdmin ? <AdminControls id={item.id} /> : null}
        </div>
      </div>
      {item.description ? (
        <section className="min-w-0 rounded-xl border border-line bg-surface p-5">
          <h2 className="font-bold">{fields.description}</h2>
          {/* Shown as text: anything a seller typed, including markup, stays text. */}
          <p data-testid="description" className="mt-2 break-words whitespace-pre-line">
            {item.description}
          </p>
        </section>
      ) : null}
    </article>
  );
}
