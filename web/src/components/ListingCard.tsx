import { Link } from 'react-router';
import type { Listing } from '../api/listings';
import { formatPrice } from '../format';
import { ImagePlaceholder } from './ImagePlaceholder';

export function ListingCard({ listing }: { listing: Listing }) {
  return (
    <Link
      to={`/listings/${listing.id}`}
      data-testid="listing-card"
      className="group block overflow-hidden rounded-xl border border-line bg-surface transition-shadow hover:shadow-md"
    >
      <div className="aspect-[4/3] w-full overflow-hidden bg-line">
        {listing.coverImageUrl ? (
          <img src={listing.coverImageUrl} alt={`${listing.brandName} ${listing.modelName}`} loading="lazy" className="h-full w-full object-cover" />
        ) : (
          <ImagePlaceholder />
        )}
      </div>
      <div className="p-4">
        <p className="text-lg font-bold">{formatPrice(listing.price, listing.currency)}</p>
        <h2 className="mt-1 truncate font-medium group-hover:text-accent">
          {listing.brandName} {listing.modelName}
        </h2>
        <p className="mt-1 text-sm text-muted">
          {listing.year}, {listing.cityName}
        </p>
      </div>
    </Link>
  );
}
