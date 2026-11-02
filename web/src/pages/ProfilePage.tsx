import { useState, type FormEvent } from 'react';
import { ApiError } from '../api/http';
import { useAuth } from '../auth/AuthContext';
import { Alert } from '../components/Alert';
import { Button } from '../components/Button';
import { Field } from '../components/Field';
import { formatPhone, normalizePhone } from '../format';
import { az } from '../i18n/az';

export function ProfilePage() {
  const { auth, savePhone } = useAuth();
  const user = auth.status === 'signedIn' ? auth.user : null;
  const [phone, setPhone] = useState(user?.phone ? formatPhone(user.phone) : '');
  const [phoneError, setPhoneError] = useState<string | undefined>();
  const [formError, setFormError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);
  const [busy, setBusy] = useState(false);
  const text = az.profile;

  // RequireAuth lets only signed-in people reach this page.
  if (!user) {
    return null;
  }

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    setPhoneError(undefined);
    setFormError(null);
    setSaved(false);

    const normalized = normalizePhone(phone);
    if (!normalized) {
      setPhoneError(az.phone.invalid);
      return;
    }

    setBusy(true);
    try {
      await savePhone(normalized);
      setPhone(formatPhone(normalized));
      setSaved(true);
    } catch (failure) {
      if (failure instanceof ApiError && failure.status === 400) {
        setPhoneError(az.phone.invalid);
      } else {
        setFormError(az.errors.unavailable);
      }
    } finally {
      setBusy(false);
    }
  };

  const facts = [
    [text.username, user.username],
    [text.email, user.email],
    [text.firstName, user.firstName],
    [text.lastName, user.lastName],
  ];

  return (
    <section className="mx-auto max-w-md rounded-xl border border-line bg-surface p-6">
      <h1 className="text-2xl font-bold">{text.title}</h1>
      <dl className="mt-4 text-sm">
        {facts.map(([name, value]) => (
          <div key={name} className="flex justify-between gap-4 border-b border-line py-2">
            <dt className="text-muted">{name}</dt>
            <dd className="min-w-0 text-right font-medium break-words">{value}</dd>
          </div>
        ))}
      </dl>
      <form onSubmit={submit} noValidate className="mt-6 space-y-4">
        <Field label={az.phone.label} type="tel" inputMode="tel" autoComplete="tel" value={phone} onChange={(event) => setPhone(event.target.value)} error={phoneError} hint={az.phone.hint} />
        {saved ? <Alert kind="success">{text.saved}</Alert> : null}
        {formError ? <Alert kind="error">{formError}</Alert> : null}
        <Button type="submit" disabled={busy} className="w-full">
          {busy ? az.common.wait : az.common.save}
        </Button>
      </form>
    </section>
  );
}
