import { az } from '../i18n/az';

export function Loading() {
  return (
    <p role="status" className="py-16 text-center text-muted">
      {az.common.loading}
    </p>
  );
}
