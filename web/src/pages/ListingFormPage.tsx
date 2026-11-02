import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useEffect, useState, type FormEvent } from 'react';
import { useNavigate, useParams } from 'react-router';
import { getBrands, getCities, getModels } from '../api/catalog';
import { ApiError } from '../api/http';
import { createListing, getListing, updateListing, uploadImage, type Listing, type ListingFields } from '../api/listings';
import { useAuth } from '../auth/AuthContext';
import { Alert } from '../components/Alert';
import { Button } from '../components/Button';
import { ErrorRetry } from '../components/ErrorRetry';
import { Field } from '../components/Field';
import { ImagePicker, type PickedImage } from '../components/ImagePicker';
import { Loading } from '../components/Loading';
import { SelectField } from '../components/SelectField';
import { UploadRows } from '../components/UploadRows';
import { az } from '../i18n/az';
import type { UploadResult } from '../images';
import { NotAllowedPage } from './NotAllowedPage';
import { NotFoundPage } from './NotFoundPage';

// What the form holds: text as typed, turned into numbers only when sent.
type FormValues = {
  brandId: string;
  modelId: string;
  cityId: string;
  year: string;
  mileage: string;
  mileageUnit: string;
  engineVolume: string;
  horsepower: string;
  seats: string;
  vin: string;
  bodyType: string;
  fuelType: string;
  transmission: string;
  drivetrain: string;
  color: string;
  market: string;
  price: string;
  currency: string;
  description: string;
  isCredit: boolean;
  isBarter: boolean;
  isPainted: boolean;
  isDamaged: boolean;
};

const blank: FormValues = {
  brandId: '',
  modelId: '',
  cityId: '',
  year: '',
  mileage: '',
  mileageUnit: 'Km',
  engineVolume: '',
  horsepower: '',
  seats: '',
  vin: '',
  bodyType: '',
  fuelType: '',
  transmission: '',
  drivetrain: '',
  color: '',
  market: '',
  price: '',
  currency: 'AZN',
  description: '',
  isCredit: false,
  isBarter: false,
  isPainted: false,
  isDamaged: false,
};

function fromListing(listing: Listing): FormValues {
  return {
    brandId: listing.brandId,
    modelId: listing.modelId,
    cityId: listing.cityId,
    year: String(listing.year),
    mileage: String(listing.mileage),
    mileageUnit: listing.mileageUnit,
    engineVolume: listing.engineVolume === null ? '' : String(listing.engineVolume),
    horsepower: String(listing.horsepower),
    seats: String(listing.seats),
    vin: listing.vin ?? '',
    bodyType: listing.bodyType,
    fuelType: listing.fuelType,
    transmission: listing.transmission,
    drivetrain: listing.drivetrain,
    color: listing.color,
    market: listing.market,
    price: String(listing.price),
    currency: listing.currency,
    description: listing.description ?? '',
    isCredit: listing.isCredit,
    isBarter: listing.isBarter,
    isPainted: listing.isPainted,
    isDamaged: listing.isDamaged,
  };
}

const REQUIRED: (keyof FormValues)[] = ['brandId', 'modelId', 'cityId', 'year', 'mileage', 'mileageUnit', 'horsepower', 'seats', 'bodyType', 'fuelType', 'transmission', 'drivetrain', 'color', 'market', 'price', 'currency'];

const WHOLE_NUMBERS: (keyof FormValues)[] = ['year', 'mileage', 'horsepower', 'seats'];

// Catches only what could not be sent at all; the server checks the ranges.
function validate(values: FormValues) {
  const errors: Record<string, string> = {};
  const messages = az.listingForm.errors;

  for (const name of REQUIRED) {
    if (values[name] === '') {
      errors[name] = messages[name];
    }
  }
  for (const name of WHOLE_NUMBERS) {
    if (values[name] !== '' && !Number.isInteger(Number(values[name]))) {
      errors[name] = messages[name];
    }
  }
  if (values.fuelType !== 'Electric' && values.engineVolume === '') {
    errors.engineVolume = messages.engineVolume;
  }
  return errors;
}

function toFields(values: FormValues): ListingFields {
  return {
    modelId: values.modelId,
    cityId: values.cityId,
    year: Number(values.year),
    mileage: Number(values.mileage),
    mileageUnit: values.mileageUnit,
    engineVolume: values.fuelType === 'Electric' ? null : Number(values.engineVolume),
    horsepower: Number(values.horsepower),
    seats: Number(values.seats),
    vin: values.vin.trim() || null,
    bodyType: values.bodyType,
    fuelType: values.fuelType,
    transmission: values.transmission,
    drivetrain: values.drivetrain,
    color: values.color,
    market: values.market,
    price: Number(values.price),
    currency: values.currency,
    description: values.description.trim() || null,
    isCredit: values.isCredit,
    isBarter: values.isBarter,
    isPainted: values.isPainted,
    isDamaged: values.isDamaged,
  };
}

function optionsOf(labels: Record<string, string>) {
  return Object.entries(labels).map(([value, label]) => ({ value, label }));
}

const CURRENCIES = [
  { value: 'AZN', label: 'AZN' },
  { value: 'USD', label: 'USD' },
  { value: 'EUR', label: 'EUR' },
];

function ListingForm({ listing }: { listing?: Listing }) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [values, setValues] = useState<FormValues>(listing ? fromListing(listing) : blank);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [pictures, setPictures] = useState<PickedImage[]>([]);
  const [uploads, setUploads] = useState<UploadResult[]>([]);

  const brands = useQuery({ queryKey: ['brands'], queryFn: getBrands });
  const cities = useQuery({ queryKey: ['cities'], queryFn: getCities });
  const models = useQuery({ queryKey: ['models', values.brandId], queryFn: () => getModels(values.brandId), enabled: values.brandId !== '' });

  const { fields, labels } = az.listing;
  const form = az.listingForm;

  const set = (name: keyof FormValues) => (event: { target: { value: string } }) => {
    const value = event.target.value;
    setValues((current) => ({
      ...current,
      [name]: value,
      // A model belongs to one brand, so changing the brand clears it.
      ...(name === 'brandId' ? { modelId: '' } : {}),
      ...(name === 'fuelType' && value === 'Electric' ? { engineVolume: '' } : {}),
    }));
  };

  const toggle = (name: 'isCredit' | 'isBarter' | 'isPainted' | 'isDamaged') => (event: { target: { checked: boolean } }) => {
    setValues((current) => ({ ...current, [name]: event.target.checked }));
  };

  // One at a time, so the images land in the order shown.
  const uploadPictures = async (listingId: string) => {
    const status = az.images.status;
    let results: UploadResult[] = pictures.map((picture) => ({ key: picture.key, name: picture.file.name, state: 'waiting', message: status.waiting }));
    const update = (key: number, state: UploadResult['state'], message: string) => {
      results = results.map((result) => (result.key === key ? { ...result, state, message } : result));
      setUploads(results);
    };

    setUploads(results);
    for (const picture of pictures) {
      update(picture.key, 'uploading', status.uploading);
      try {
        await uploadImage(listingId, picture.file);
        update(picture.key, 'done', status.done);
      } catch (failure) {
        const code = failure instanceof ApiError ? failure.status : 0;
        update(picture.key, 'failed', code === 400 || code === 413 ? status.rejected : code === 409 ? status.limit : status.unavailable);
      }
    }
    return results;
  };

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    setFormError(null);

    const found = validate(values);
    // A new listing starts with its images; an existing one manages them on its image page.
    if (!listing && pictures.length === 0) {
      found.pictures = az.images.required;
    }
    setErrors(found);
    if (Object.keys(found).length > 0) {
      setFormError(az.errors.invalid);
      return;
    }

    setBusy(true);
    try {
      const saved = listing ? await updateListing(listing.id, toFields(values)) : await createListing(toFields(values));
      const results = listing ? [] : await uploadPictures(saved.id);
      if (listing) {
        queryClient.setQueryData(['listing', saved.id], saved);
      } else {
        queryClient.removeQueries({ queryKey: ['listing', saved.id] });
      }
      await queryClient.invalidateQueries({ queryKey: ['listings'] });
      await queryClient.invalidateQueries({ queryKey: ['my-listings'] });

      // The listing exists now, so the form must not stay open: saving again would create another one.
      if (results.some((result) => result.state === 'failed')) {
        navigate(`/listings/${saved.id}/images`, { state: { uploads: results } });
      } else {
        navigate(`/listings/${saved.id}`);
      }
    } catch (failure) {
      if (failure instanceof ApiError && failure.status === 400) {
        const next: Record<string, string> = {};
        for (const key of Object.keys(failure.fieldErrors)) {
          // A value the API cannot read at all is reported on a path such as "$.year".
          const name = key.replace(/^\$\./, '');
          if (form.errors[name]) {
            next[name] = form.errors[name];
          }
        }
        setErrors(next);
        setFormError(az.errors.invalid);
      } else {
        setFormError(az.errors.unavailable);
      }
    } finally {
      setBusy(false);
    }
  };

  if (brands.isPending || cities.isPending) {
    return <Loading />;
  }
  if (brands.isError || cities.isError) {
    return (
      <ErrorRetry
        onRetry={() => {
          brands.refetch();
          cities.refetch();
        }}
      />
    );
  }

  const toOptions = (entries: { id: string; name: string }[]) => entries.map((entry) => ({ value: entry.id, label: entry.name }));
  const notes = [
    ['isCredit', fields.credit],
    ['isBarter', fields.barter],
    ['isPainted', fields.painted],
    ['isDamaged', fields.damaged],
  ] as const;

  return (
    <section className="mx-auto max-w-3xl rounded-xl border border-line bg-surface p-6">
      <h1 className="text-2xl font-bold">{listing ? form.editTitle : form.newTitle}</h1>
      {brands.data.length === 0 || cities.data.length === 0 ? (
        <div className="mt-4">
          <Alert kind="error">{form.emptyCatalog}</Alert>
        </div>
      ) : null}
      <form onSubmit={submit} noValidate className="mt-6 space-y-6">
        <div className="grid gap-4 sm:grid-cols-2">
          <SelectField label={form.brand} value={values.brandId} onChange={set('brandId')} error={errors.brandId} options={toOptions(brands.data)} />
          <SelectField
            label={form.model}
            value={values.modelId}
            onChange={set('modelId')}
            error={errors.modelId}
            disabled={values.brandId === ''}
            placeholder={values.brandId === '' ? form.chooseBrandFirst : az.common.choose}
            options={toOptions(models.data ?? [])}
          />
          <SelectField label={fields.city} value={values.cityId} onChange={set('cityId')} error={errors.cityId} options={toOptions(cities.data)} />
          <Field label={fields.year} type="number" inputMode="numeric" value={values.year} onChange={set('year')} error={errors.year} />
          <Field label={fields.mileage} type="number" inputMode="numeric" value={values.mileage} onChange={set('mileage')} error={errors.mileage} />
          <SelectField label={form.mileageUnit} value={values.mileageUnit} onChange={set('mileageUnit')} error={errors.mileageUnit} options={optionsOf(form.units)} />
          <SelectField label={fields.fuelType} value={values.fuelType} onChange={set('fuelType')} error={errors.fuelType} options={optionsOf(labels.fuelType)} />
          {values.fuelType !== 'Electric' ? <Field label={form.engineVolume} type="number" step="0.1" inputMode="decimal" value={values.engineVolume} onChange={set('engineVolume')} error={errors.engineVolume} /> : null}
          <Field label={fields.horsepower} type="number" inputMode="numeric" value={values.horsepower} onChange={set('horsepower')} error={errors.horsepower} />
          <Field label={fields.seats} type="number" inputMode="numeric" value={values.seats} onChange={set('seats')} error={errors.seats} />
          <SelectField label={fields.bodyType} value={values.bodyType} onChange={set('bodyType')} error={errors.bodyType} options={optionsOf(labels.bodyType)} />
          <SelectField label={fields.transmission} value={values.transmission} onChange={set('transmission')} error={errors.transmission} options={optionsOf(labels.transmission)} />
          <SelectField label={fields.drivetrain} value={values.drivetrain} onChange={set('drivetrain')} error={errors.drivetrain} options={optionsOf(labels.drivetrain)} />
          <SelectField label={fields.color} value={values.color} onChange={set('color')} error={errors.color} options={optionsOf(labels.color)} />
          <SelectField label={fields.market} value={values.market} onChange={set('market')} error={errors.market} options={optionsOf(labels.market)} />
          <Field label={fields.vin} value={values.vin} onChange={set('vin')} error={errors.vin} maxLength={17} autoCapitalize="characters" />
          <Field label={form.price} type="number" inputMode="decimal" value={values.price} onChange={set('price')} error={errors.price} />
          <SelectField label={form.currency} value={values.currency} onChange={set('currency')} error={errors.currency} options={CURRENCIES} />
        </div>
        <div>
          <label htmlFor="listing-description" className="mb-1 block text-sm font-medium">
            {fields.description}
          </label>
          <textarea
            id="listing-description"
            rows={5}
            maxLength={3000}
            value={values.description}
            onChange={set('description')}
            className={`w-full rounded-lg border bg-surface px-3 py-2 outline-none focus:ring-2 focus:ring-accent ${errors.description ? 'border-danger' : 'border-line'}`}
          />
          {errors.description ? <p className="mt-1 text-sm text-danger">{errors.description}</p> : null}
        </div>
        <div className="grid gap-2 sm:grid-cols-2">
          {notes.map(([name, text]) => (
            <label key={name} className="flex items-center gap-2">
              <input type="checkbox" checked={values[name]} onChange={toggle(name)} className="h-4 w-4 accent-accent" />
              {text}
            </label>
          ))}
        </div>
        {listing ? null : <ImagePicker pictures={pictures} onChange={setPictures} error={errors.pictures} disabled={busy} />}
        <UploadRows uploads={uploads} />
        {formError ? <Alert kind="error">{formError}</Alert> : null}
        <Button type="submit" disabled={busy} className="w-full sm:w-auto">
          {busy ? az.common.wait : az.common.save}
        </Button>
      </form>
    </section>
  );
}

function EditListing({ id }: { id: string }) {
  const { auth } = useAuth();
  const listing = useQuery({ queryKey: ['listing', id], queryFn: () => getListing(id) });

  useEffect(() => {
    window.scrollTo(0, 0);
  }, []);

  if (listing.isPending) {
    return <Loading />;
  }
  if (listing.isError) {
    const status = listing.error instanceof ApiError ? listing.error.status : 0;
    return status === 404 || status === 400 ? <NotFoundPage /> : <ErrorRetry onRetry={() => listing.refetch()} />;
  }
  // Only the owner edits; the API refuses anyone else, admins included.
  if (auth.status !== 'signedIn' || auth.user.id !== listing.data.ownerId) {
    return <NotAllowedPage />;
  }
  return <ListingForm listing={listing.data} />;
}

export function ListingFormPage() {
  const { id } = useParams();
  return id ? <EditListing id={id} /> : <ListingForm />;
}
