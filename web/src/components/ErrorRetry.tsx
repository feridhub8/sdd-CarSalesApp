import { az } from '../i18n/az';
import { Alert } from './Alert';
import { Button } from './Button';
import { failureMessage } from '../errors';

export function ErrorRetry({ error, onRetry }: { error: unknown; onRetry: () => void }) {
  return (
    <div className="mx-auto mt-8 max-w-md space-y-3 text-center">
      <Alert kind="error">{failureMessage(error)}</Alert>
      <Button variant="secondary" onClick={onRetry}>
        {az.common.retry}
      </Button>
    </div>
  );
}
