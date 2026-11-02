import { ApiError } from './api/http';
import { az } from './i18n/az';

// The message for a failure that no page handles in its own way.
export function failureMessage(failure: unknown) {
  return failure instanceof ApiError && failure.status === 429 ? az.errors.tooMany : az.errors.unavailable;
}
