import { useState, type FormEvent } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router';
import { ApiError } from '../api/http';
import { useAuth } from '../auth/AuthContext';
import { safeNext } from '../auth/RequireAuth';
import { Alert } from '../components/Alert';
import { Button } from '../components/Button';
import { Field } from '../components/Field';
import { az } from '../i18n/az';
import { failureMessage } from '../errors';

export function LoginPage() {
  const { signIn } = useAuth();
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const [login, setLogin] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);
    if (!login.trim() || !password) {
      setError(az.login.missing);
      return;
    }

    setBusy(true);
    try {
      await signIn(login.trim(), password);
      navigate(safeNext(params.get('next')), { replace: true });
    } catch (failure) {
      const status = failure instanceof ApiError ? failure.status : 0;
      // One general message: it must not say which of the two was wrong.
      const refused = status === 401 || status === 400;
      setError(refused ? az.login.wrong : failureMessage(failure));
      // Only refused credentials are worth retyping; a limit or an outage says nothing about the password.
      if (refused) {
        setPassword('');
      }
    } finally {
      setBusy(false);
    }
  };

  return (
    <section className="mx-auto max-w-md rounded-xl border border-line bg-surface p-6">
      <h1 className="text-2xl font-bold">{az.login.title}</h1>
      {params.has('registered') ? (
        <div className="mt-4">
          <Alert kind="success">{az.login.registered}</Alert>
        </div>
      ) : null}
      {error ? (
        <div className="mt-4">
          <Alert kind="error">{error}</Alert>
        </div>
      ) : null}
      <form onSubmit={submit} noValidate className="mt-6 space-y-4">
        <Field label={az.login.login} value={login} onChange={(event) => setLogin(event.target.value)} autoComplete="username" />
        <Field label={az.login.password} type="password" value={password} onChange={(event) => setPassword(event.target.value)} autoComplete="current-password" />
        <Button type="submit" disabled={busy} className="w-full">
          {busy ? az.common.wait : az.login.submit}
        </Button>
      </form>
      <p className="mt-4 text-center text-sm text-muted">
        {az.login.noAccount}{' '}
        <Link to="/register" className="font-medium text-accent hover:text-accent-strong">
          {az.login.toRegister}
        </Link>
      </p>
    </section>
  );
}
