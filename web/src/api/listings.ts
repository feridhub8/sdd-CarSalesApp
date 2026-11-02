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
