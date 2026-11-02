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

export type CatalogKind = 'Brand' | 'Model' | 'City';

export type CatalogEntry = {
  id: string;
  name: string;
  brandId?: string;
  brandName?: string;
  isDeleted: boolean;
  deletedAt: string | null;
};

export function getCatalogWithDeleted(kind: CatalogKind) {
  return api<CatalogEntry[]>(`${API}/${kind}/WithDeleted`);
}

export function createCatalogEntry(kind: CatalogKind, name: string, brandId?: string) {
  return api<CatalogEntry>(`${API}/${kind}/Create`, { method: 'POST', body: brandId ? { brandId, name } : { name } });
}

export function updateCatalogEntry(kind: CatalogKind, id: string, name: string, brandId?: string) {
  return api<CatalogEntry>(`${API}/${kind}/Update/${id}`, { method: 'PUT', body: brandId ? { brandId, name } : { name } });
}

export function deleteCatalogEntry(kind: CatalogKind, id: string) {
  return api<void>(`${API}/${kind}/Delete/${id}`, { method: 'DELETE' });
}

export function restoreCatalogEntry(kind: CatalogKind, id: string) {
  return api<void>(`${API}/${kind}/Restore/${id}`, { method: 'PATCH' });
}
