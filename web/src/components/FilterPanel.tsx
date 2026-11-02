import { useQuery } from '@tanstack/react-query';
import { useEffect, useRef, useState, type ChangeEvent } from 'react';
import { getBrands, getCities, getModels } from '../api/catalog';
import { az } from '../i18n/az';
import { activeFilterCount, CURRENCIES, DEFAULT_CURRENCY, FLAGS, isReversed, MULTI_FILTERS, RANGES } from '../search';
import { SelectField } from './SelectField';

const TYPING_DELAY_MS = 500;

const text = az.search;
const { labels, fields } = az.listing;

const MULTI_LABELS: Record<(typeof MULTI_FILTERS)[number], string> = {
  bodyType: fields.bodyType,
  fuelType: fields.fuelType,
  transmission: fields.transmission,
  drivetrain: fields.drivetrain,
  color: fields.color,
  market: fields.market,
};

const RANGE_LABELS = [text.year, text.mileage, text.engine, text.price];

type EditSearch = (edit: (search: URLSearchParams) => void, options?: { replace?: boolean }) => void;

// Keeps what is being typed to itself, and hands it over once typing stops.
function NumberInput({ label, hint, value, typing, onCommit }: { label: string; hint: string; value: string; typing: RegExp; onCommit: (value: string) => void }) {
  const [draft, setDraft] = useState(value);
  const timer = useRef<number | undefined>(undefined);
  const latestCommit = useRef(onCommit);

  useEffect(() => {
    latestCommit.current = onCommit;
  });

  // The address changed from elsewhere, such as the back button. A draft that only differs by a trailing dot is the same value.
  useEffect(() => {
    setDraft((current) => (current.replace(/\.$/, '') === value ? current : value));
    window.clearTimeout(timer.current);
  }, [value]);

  useEffect(() => () => window.clearTimeout(timer.current), []);

  const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
    const next = event.target.value.replace(',', '.');
    if (!typing.test(next)) {
      return;
    }

    setDraft(next);
    window.clearTimeout(timer.current);
    timer.current = window.setTimeout(() => latestCommit.current(next.replace(/\.$/, '')), TYPING_DELAY_MS);
  };

  return (
    <input
      type="text"
      inputMode="decimal"
      aria-label={label}
      placeholder={hint}
      value={draft}
      onChange={handleChange}
      className="w-full min-w-0 rounded-lg border border-line bg-surface px-3 py-2 outline-none focus:ring-2 focus:ring-accent"
    />
  );
}

function CheckBox({ label, checked, onChange }: { label: string; checked: boolean; onChange: () => void }) {
  return (
    <label className="flex items-start gap-2 text-sm">
      <input type="checkbox" checked={checked} onChange={onChange} className="mt-0.5 h-4 w-4 shrink-0 accent-accent" />
      <span className="min-w-0">{label}</span>
    </label>
  );
}

export function FilterPanel({ search, onEdit, onClear }: { search: URLSearchParams; onEdit: EditSearch; onClear: () => void }) {
  const [open, setOpen] = useState(false);
  const brandId = search.get('brandId') ?? '';

  const brands = useQuery({ queryKey: ['brands'], queryFn: getBrands });
  const cities = useQuery({ queryKey: ['cities'], queryFn: getCities });
  const models = useQuery({ queryKey: ['models', brandId], queryFn: () => getModels(brandId), enabled: brandId !== '' });

  const active = activeFilterCount(search);
  const toOptions = (entries: { id: string; name: string }[] | undefined) => (entries ?? []).map((entry) => ({ value: entry.id, label: entry.name }));

  const setOrDelete = (name: string, value: string) => (next: URLSearchParams) => {
    if (value) {
      next.set(name, value);
    } else {
      next.delete(name);
    }
  };

  const toggleValue = (name: string, value: string) => () =>
    onEdit((next) => {
      if (next.has(name, value)) {
        next.delete(name, value);
      } else {
        next.append(name, value);
      }
    });

  return (
    <aside className="min-w-0">
      <div className="flex min-h-10 items-center justify-between gap-3">
        <h2 className="hidden font-bold lg:block">{text.filters}</h2>
        <button
          type="button"
          aria-expanded={open}
          onClick={() => setOpen((current) => !current)}
          className="rounded-lg border border-line bg-surface px-4 py-2 font-medium hover:bg-page lg:hidden"
        >
          {text.filters}
          {active > 0 ? (
            <span data-testid="active-filters" className="ml-2 rounded-full bg-accent px-2 py-0.5 text-sm text-white">
              {active}
            </span>
          ) : null}
        </button>
        {active > 0 ? (
          <button type="button" onClick={onClear} className="text-sm font-medium text-accent hover:text-accent-strong">
            {text.clear}
          </button>
        ) : null}
      </div>

      <div data-testid="filter-panel" className={`mt-3 space-y-5 rounded-xl border border-line bg-surface p-4 ${open ? '' : 'hidden lg:block'}`}>
        <div className="space-y-3">
          <SelectField
            label={az.listingForm.brand}
            placeholder={text.all}
            value={brandId}
            error={brands.isError ? az.errors.unavailable : undefined}
            options={toOptions(brands.data)}
            onChange={(event) =>
              onEdit((next) => {
                setOrDelete('brandId', event.target.value)(next);
                next.delete('modelId');
              })
            }
          />
          <SelectField
            label={az.listingForm.model}
            placeholder={brandId ? text.all : az.listingForm.chooseBrandFirst}
            disabled={!brandId}
            value={search.get('modelId') ?? ''}
            error={models.isError ? az.errors.unavailable : undefined}
            options={toOptions(models.data)}
            onChange={(event) => onEdit(setOrDelete('modelId', event.target.value))}
          />
          <SelectField
            label={fields.city}
            placeholder={text.all}
            value={search.get('cityId') ?? ''}
            error={cities.isError ? az.errors.unavailable : undefined}
            options={toOptions(cities.data)}
            onChange={(event) => onEdit(setOrDelete('cityId', event.target.value))}
          />
        </div>

        {RANGES.map((range, index) => (
          <fieldset key={range.from}>
            <legend className="mb-1 text-sm font-medium">{RANGE_LABELS[index]}</legend>
            <div className="grid grid-cols-2 gap-2">
              <NumberInput
                label={`${RANGE_LABELS[index]}: ${text.from}`}
                hint={text.from}
                value={search.get(range.from) ?? ''}
                typing={range.typing}
                onCommit={(value) => onEdit(setOrDelete(range.from, value), { replace: true })}
              />
              <NumberInput
                label={`${RANGE_LABELS[index]}: ${text.to}`}
                hint={text.to}
                value={search.get(range.to) ?? ''}
                typing={range.typing}
                onCommit={(value) => onEdit(setOrDelete(range.to, value), { replace: true })}
              />
            </div>
            {range.from === 'priceFrom' ? (
              <div className="mt-2 space-y-2">
                <SelectField
                  label={az.listingForm.currency}
                  placeholder={null}
                  value={search.get('priceCurrency') ?? DEFAULT_CURRENCY}
                  options={CURRENCIES.map((currency) => ({ value: currency, label: currency }))}
                  onChange={(event) => onEdit(setOrDelete('priceCurrency', event.target.value === DEFAULT_CURRENCY ? '' : event.target.value))}
                />
                <p className="text-sm text-muted">{text.rateNote}</p>
              </div>
            ) : null}
            {isReversed(search, range) ? (
              <p role="alert" className="mt-1 text-sm text-danger">
                {text.reversed}
              </p>
            ) : null}
          </fieldset>
        ))}

        {MULTI_FILTERS.map((name) => (
          <fieldset key={name}>
            <legend className="mb-2 text-sm font-medium">{MULTI_LABELS[name]}</legend>
            <div className="grid grid-cols-2 gap-x-2 gap-y-2">
              {Object.entries(labels[name]).map(([value, label]) => (
                <CheckBox key={value} label={label} checked={search.has(name, value)} onChange={toggleValue(name, value)} />
              ))}
            </div>
          </fieldset>
        ))}

        <fieldset>
          <legend className="mb-2 text-sm font-medium">{text.other}</legend>
          <div className="space-y-2">
            {FLAGS.map((name) => (
              <CheckBox
                key={name}
                label={text.flags[name]}
                checked={search.has(name)}
                onChange={() => onEdit(setOrDelete(name, search.has(name) ? '' : 'true'))}
              />
            ))}
          </div>
        </fieldset>
      </div>
    </aside>
  );
}
