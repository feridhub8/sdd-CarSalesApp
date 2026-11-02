import { Link } from 'react-router';
import { az } from '../i18n/az';

export function NotFoundPage() {
  return (
    <section className="py-16 text-center">
      <h1 className="text-2xl font-bold">{az.notFound.title}</h1>
      <p className="mt-2 text-muted">{az.notFound.text}</p>
      <Link to="/" className="mt-6 inline-block font-medium text-accent hover:text-accent-strong">
        {az.notFound.back}
      </Link>
    </section>
  );
}
