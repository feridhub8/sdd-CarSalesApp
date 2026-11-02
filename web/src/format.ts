import { az } from './i18n/az';

const currencySigns: Record<string, string> = { AZN: '₼', USD: '$', EUR: '€' };

// Written by hand: browsers ship different locale data, and some fall back to English formats.
export function formatNumber(value: number) {
  return Math.round(value).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
}

export function formatPrice(price: number, currency: string) {
  return `${formatNumber(price)} ${currencySigns[currency] ?? currency}`;
}

export function formatMileage(mileage: number, unit: string) {
  return `${formatNumber(mileage)} ${unit === 'Mi' ? az.units.miles : az.units.kilometers}`;
}

export function formatEngine(volume: number | null) {
  return volume === null ? null : `${volume.toFixed(1)} ${az.units.litres}`;
}

export function formatDate(date: string) {
  const value = new Date(date);
  return `${value.getDate()} ${az.months[value.getMonth()]} ${value.getFullYear()}`;
}

// +994501234567 as +994 50 123 45 67. Anything else shows as it is.
export function formatPhone(phone: string) {
  const parts = /^\+994(\d{2})(\d{3})(\d{2})(\d{2})$/.exec(phone);
  return parts ? `+994 ${parts[1]} ${parts[2]} ${parts[3]} ${parts[4]}` : phone;
}

// The stored form of a number, or null when it is not valid. Repeats the API's rule so most mistakes show
// without a request; the API stays the authority.
export function normalizePhone(value: string) {
  const match = /^(?:\+994|994|0)([0-9]{9})$/.exec(value.replace(/[\s\-()]/g, ''));
  return match ? `+994${match[1]}` : null;
}

// An enum value the site does not know yet shows as it is, rather than as an empty field.
export function label(labels: Record<string, string>, value: string) {
  return labels[value] ?? value;
}
