import { API, api } from './http';

export type NamedEntry = {
  id: string;
  name: string;
};

export type ModelEntry = NamedEntry & {
  brandId: string;
  brandName: string;
};

export function getBrands() {
  return api<NamedEntry[]>(`${API}/Brand`);
}

export function getCities() {
  return api<NamedEntry[]>(`${API}/City`);
}

export function getModels(brandId: string) {
  return api<ModelEntry[]>(`${API}/Model?brandId=${encodeURIComponent(brandId)}`);
}
