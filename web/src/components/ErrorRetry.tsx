import { az } from '../i18n/az';
import { Alert } from './Alert';
import { Button } from './Button';

export function ErrorRetry({ onRetry }: { onRetry: () => void }) {
  return (
    <div className="mx-auto mt-8 max-w-md space-y-3 text-center">
      <Alert kind="error">{az.errors.unavailable}</Alert>
      <Button variant="secondary" onClick={onRetry}>
        {az.common.retry}
      </Button>
    </div>
  );
}
