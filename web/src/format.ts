import { az } from './i18n/az';

const currencySigns: Record<string, string> = { AZN: '₼', USD: '$', EUR: '€' };

// Written by hand: browsers ship different locale data, and some fall back to English formats.
function formatNumber(value: number) {
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

// An enum value the site does not know yet shows as it is, rather than as an empty field.
export function label(labels: Record<string, string>, value: string) {
  return labels[value] ?? value;
}
