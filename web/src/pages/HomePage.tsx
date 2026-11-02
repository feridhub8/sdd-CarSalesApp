import { az } from '../i18n/az';

export function HomePage() {
  return (
    <section>
      <h1 className="text-2xl font-bold">{az.home.title}</h1>
      <p className="mt-2 text-muted">{az.home.comingSoon}</p>
    </section>
  );
}
