import { LISTINGS_PAGE_SIZE } from './api/listings';
import { az } from './i18n/az';

const { labels } = az.listing;

export const SORTS = ['Newest', 'PriceAsc', 'PriceDesc', 'YearDesc', 'MileageAsc'];
export const CURRENCIES = ['AZN', 'USD', 'EUR'];
export const DEFAULT_SORT = 'Newest';
export const DEFAULT_CURRENCY = 'AZN';

// What a number field accepts while typing. The longest values stay within what the API accepts.
export const NUMBER_TYPING = {
  whole: /^\d{0,9}$/,
  price: /^(\d{1,12}(\.\d{0,2})?)?$/,
  engine: /^(\d{1,2}(\.\d?)?)?$/,
};

export const RANGES = [
  { from: 'yearFrom', to: 'yearTo', typing: NUMBER_TYPING.whole },
  { from: 'mileageFrom', to: 'mileageTo', typing: NUMBER_TYPING.whole },
  { from: 'engineFrom', to: 'engineTo', typing: NUMBER_TYPING.engine },
  { from: 'priceFrom', to: 'priceTo', typing: NUMBER_TYPING.price },
];

export const MULTI_FILTERS = ['bodyType', 'fuelType', 'transmission', 'drivetrain', 'color', 'market'] as const;
export const FLAGS = ['isCredit', 'isBarter', 'notPainted', 'notDamaged', 'hasImages'] as const;

const ID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

// Only the values the site knows, in a fixed order, so a hand-edited address cannot cause an API error
// and two addresses with the same filters share one cached result. The page is kept apart.
export function readSearch(address: URLSearchParams) {
  const search = new URLSearchParams();
  const brandId = address.get('brandId');

  if (brandId && ID.test(brandId)) {
    search.set('brandId', brandId);
    const modelId = address.get('modelId');
    if (modelId && ID.test(modelId)) {
      search.set('modelId', modelId);
    }
  }

  const cityId = address.get('cityId');
  if (cityId && ID.test(cityId)) {
    search.set('cityId', cityId);
  }

  for (const { from, to, typing } of RANGES) {
    for (const name of [from, to]) {
      const value = address.get(name);
      if (value && typing.test(value) && !value.endsWith('.')) {
        search.set(name, value);
      }
    }
  }

  const currency = address.get('priceCurrency');
  if (currency && currency !== DEFAULT_CURRENCY && CURRENCIES.includes(currency)) {
    search.set('priceCurrency', currency);
  }

  for (const name of MULTI_FILTERS) {
    const chosen = address.getAll(name);
    for (const value of Object.keys(labels[name])) {
      if (chosen.includes(value)) {
        search.append(name, value);
      }
    }
  }

  for (const name of FLAGS) {
    if (address.get(name) === 'true') {
      search.set(name, 'true');
    }
  }

  const sort = address.get('sort');
  if (sort && sort !== DEFAULT_SORT && SORTS.includes(sort)) {
    search.set('sort', sort);
  }

  return search;
}

export function isReversed(search: URLSearchParams, range: { from: string; to: string }) {
  const from = search.get(range.from);
  const to = search.get(range.to);
  return from !== null && to !== null && Number(from) > Number(to);
}

// A range counts once, whichever ends it has; each ticked value counts on its own.
export function activeFilterCount(search: URLSearchParams) {
  const ids = ['brandId', 'modelId', 'cityId'].filter((name) => search.has(name)).length;
  const ranges = RANGES.filter(({ from, to }) => search.has(from) || search.has(to)).length;
  const values = MULTI_FILTERS.reduce((count, name) => count + search.getAll(name).length, 0);
  const flags = FLAGS.filter((name) => search.has(name)).length;
  return ids + ranges + values + flags;
}

// The API refuses a reversed range, so it stays out of the search until the fields explain it and it is fixed.
export function toApiQuery(search: URLSearchParams, page: number) {
  const query = new URLSearchParams(search);

  for (const range of RANGES) {
    if (isReversed(search, range)) {
      query.delete(range.from);
      query.delete(range.to);
    }
  }

  if (!query.has('priceFrom') && !query.has('priceTo')) {
    query.delete('priceCurrency');
  }

  query.set('page', String(page));
  query.set('pageSize', String(LISTINGS_PAGE_SIZE));
  return query.toString();
}

// The address of a page of this search.
export function searchAddress(search: URLSearchParams, page: number) {
  const address = new URLSearchParams(search);
  if (page > 1) {
    address.set('page', String(page));
  }

  const text = address.toString();
  return text ? `/?${text}` : '/';
}
