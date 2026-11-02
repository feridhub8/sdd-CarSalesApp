import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router';
import { register, type Registration } from '../api/auth';
import { ApiError } from '../api/http';
import { Alert } from '../components/Alert';
import { Button } from '../components/Button';
import { Field } from '../components/Field';
import { az } from '../i18n/az';
import { failureMessage } from '../errors';

type Errors = Partial<Record<keyof Registration, string>>;

const empty: Registration = { username: '', email: '', password: '', firstName: '', lastName: '' };

// Repeats the server's rules, so most mistakes show without a request. The server stays the authority.
function validate(values: Registration): Errors {
  const errors: Errors = {};
  const { fields } = az.register;

  if (!/^[a-zA-Z0-9._-]{3,30}$/.test(values.username)) {
    errors.username = fields.username.invalid;
  }
  if (!/^[^\s@]+@[^\s@]+$/.test(values.email)) {
    errors.email = fields.email.invalid;
  }
  if (values.password.length < 8 || !/\p{Lu}/u.test(values.password) || !/\p{Ll}/u.test(values.password) || !/\p{Nd}/u.test(values.password) || !/[^\p{L}\p{Nd}]/u.test(values.password)) {
    errors.password = fields.password.invalid;
  }
  if (!values.firstName.trim()) {
    errors.firstName = fields.firstName.invalid;
  }
  if (!values.lastName.trim()) {
    errors.lastName = fields.lastName.invalid;
  }
  return errors;
}

export function RegisterPage() {
  const navigate = useNavigate();
  const [values, setValues] = useState(empty);
  const [errors, setErrors] = useState<Errors>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const { fields } = az.register;

  const set = (name: keyof Registration) => (event: { target: { value: string } }) => {
    setValues((current) => ({ ...current, [name]: event.target.value }));
  };

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    setFormError(null);

    const found = validate(values);
    setErrors(found);
    if (Object.keys(found).length > 0) {
      return;
    }

    setBusy(true);
    try {
      await register(values);
      navigate('/login?registered=1');
    } catch (failure) {
      setValues((current) => ({ ...current, password: '' }));
      if (failure instanceof ApiError && failure.status === 409) {
        // The conflict names the field in English; only the matching Azerbaijani message is shown.
        const detail = failure.detail?.toLowerCase() ?? '';
        if (detail.includes('username')) {
          setErrors({ username: fields.username.taken });
        } else if (detail.includes('email')) {
          setErrors({ email: fields.email.taken });
        } else {
          setFormError(az.register.taken);
        }
      } else if (failure instanceof ApiError && failure.status === 400) {
        const next: Errors = {};
        for (const name of Object.keys(failure.fieldErrors)) {
          if (name in fields) {
            next[name as keyof Registration] = fields[name as keyof Registration].invalid;
          }
        }
        setErrors(next);
        if (Object.keys(next).length === 0) {
          setFormError(az.errors.invalid);
        }
      } else {
        setFormError(failureMessage(failure));
      }
    } finally {
      setBusy(false);
    }
  };

  return (
    <section className="mx-auto max-w-md rounded-xl border border-line bg-surface p-6">
      <h1 className="text-2xl font-bold">{az.register.title}</h1>
      {formError ? (
        <div className="mt-4">
          <Alert kind="error">{formError}</Alert>
        </div>
      ) : null}
      <form onSubmit={submit} noValidate className="mt-6 space-y-4">
        <Field label={fields.username.label} value={values.username} onChange={set('username')} error={errors.username} autoComplete="username" />
        <Field label={fields.email.label} type="email" value={values.email} onChange={set('email')} error={errors.email} autoComplete="email" />
        <Field label={fields.password.label} type="password" value={values.password} onChange={set('password')} error={errors.password} hint={fields.password.hint} autoComplete="new-password" />
        <Field label={fields.firstName.label} value={values.firstName} onChange={set('firstName')} error={errors.firstName} autoComplete="given-name" />
        <Field label={fields.lastName.label} value={values.lastName} onChange={set('lastName')} error={errors.lastName} autoComplete="family-name" />
        <Button type="submit" disabled={busy} className="w-full">
          {busy ? az.common.wait : az.register.submit}
        </Button>
      </form>
      <p className="mt-4 text-center text-sm text-muted">
        {az.register.hasAccount}{' '}
        <Link to="/login" className="font-medium text-accent hover:text-accent-strong">
          {az.register.toLogin}
        </Link>
      </p>
    </section>
  );
}
