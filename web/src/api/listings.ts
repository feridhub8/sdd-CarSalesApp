import { API, api } from './http';

export type ListingImage = {
  id: string;
  url: string;
  thumbnailUrl: string;
  position: number;
};

export type Listing = {
  id: string;
  ownerId: string;
  brandId: string;
  brandName: string;
  modelId: string;
  modelName: string;
  cityId: string;
  cityName: string;
  year: number;
  mileage: number;
  mileageUnit: string;
  engineVolume: number | null;
  horsepower: number;
  seats: number;
  vin: string | null;
  bodyType: string;
  fuelType: string;
  transmission: string;
  drivetrain: string;
  color: string;
  market: string;
  price: number;
  currency: string;
  description: string | null;
  isCredit: boolean;
  isBarter: boolean;
  isPainted: boolean;
  isDamaged: boolean;
  createdAt: string;
  updatedAt: string | null;
  coverImageUrl: string | null;
  images?: ListingImage[];
};

export type Page<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export const LISTINGS_PAGE_SIZE = 20;

export function getListings(page: number) {
  return api<Page<Listing>>(`${API}/Listing?page=${page}&pageSize=${LISTINGS_PAGE_SIZE}`);
}

export function getListing(id: string) {
  return api<Listing>(`${API}/Listing/${encodeURIComponent(id)}`);
}

export function getMyListings(page: number) {
  return api<Page<Listing>>(`${API}/Listing/Mine?page=${page}&pageSize=${LISTINGS_PAGE_SIZE}`);
}

// What a listing form sends; the owner comes from the token.
export type ListingFields = {
  modelId: string;
  cityId: string;
  year: number;
  mileage: number;
  mileageUnit: string;
  engineVolume: number | null;
  horsepower: number;
  seats: number;
  vin: string | null;
  bodyType: string;
  fuelType: string;
  transmission: string;
  drivetrain: string;
  color: string;
  market: string;
  price: number;
  currency: string;
  description: string | null;
  isCredit: boolean;
  isBarter: boolean;
  isPainted: boolean;
  isDamaged: boolean;
};

export function createListing(fields: ListingFields) {
  return api<Listing>(`${API}/Listing/Create`, { method: 'POST', body: fields });
}

export function updateListing(id: string, fields: ListingFields) {
  return api<Listing>(`${API}/Listing/Update/${id}`, { method: 'PUT', body: fields });
}

export function deleteListing(id: string) {
  return api<void>(`${API}/Listing/Delete/${id}`, { method: 'DELETE' });
}

export function uploadImage(listingId: string, file: File) {
  const form = new FormData();
  form.append('file', file);
  return api<ListingImage>(`${API}/ListingImage/Create/${listingId}`, { method: 'POST', body: form });
}

export function reorderImages(listingId: string, imageIds: string[]) {
  return api<ListingImage[]>(`${API}/ListingImage/Reorder/${listingId}`, { method: 'PUT', body: { imageIds } });
}

export function deleteImage(id: string) {
  return api<void>(`${API}/ListingImage/Delete/${id}`, { method: 'DELETE' });
}

export type ListingWithDeleted = Listing & {
  isDeleted: boolean;
  deletedAt: string | null;
};

export function getListingsWithDeleted(page: number) {
  return api<Page<ListingWithDeleted>>(`${API}/Listing/WithDeleted?page=${page}&pageSize=${LISTINGS_PAGE_SIZE}`);
}

export function restoreListing(id: string) {
  return api<void>(`${API}/Listing/Restore/${id}`, { method: 'PATCH' });
}
