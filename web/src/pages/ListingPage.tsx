import { useQuery } from '@tanstack/react-query';
import { useEffect, useState } from 'react';
import { useParams } from 'react-router';
import { ApiError } from '../api/http';
import { getListing, type Listing, type ListingImage } from '../api/listings';
import { ErrorRetry } from '../components/ErrorRetry';
import { ImagePlaceholder } from '../components/ImagePlaceholder';
import { Loading } from '../components/Loading';
import { formatDate, formatEngine, formatMileage, formatPrice, label } from '../format';
import { az } from '../i18n/az';
import { NotFoundPage } from './NotFoundPage';

function Gallery({ listing, images }: { listing: Listing; images: ListingImage[] }) {
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const selected = images.find((image) => image.id === selectedId) ?? images[0];
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

export function ListingPage() {
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
  const { fields, labels } = az.listing;
  const engine = formatEngine(item.engineVolume);
  const notes = [item.isCredit && fields.credit, item.isBarter && fields.barter, item.isPainted && fields.painted, item.isDamaged && fields.damaged].filter(
    (note): note is NonNullable<typeof note> & string => Boolean(note),
  );

  return (
    // Phone order: gallery, facts, description. Desktop keeps the description under the gallery.
    <article className="grid items-start gap-6 lg:grid-cols-[3fr_2fr] lg:grid-rows-[auto_1fr] lg:gap-x-8">
      <div className="min-w-0">
        <Gallery listing={item} images={item.images ?? []} />
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
