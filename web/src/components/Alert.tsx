import type { ReactNode } from 'react';

const styles = {
  error: 'border-danger text-danger',
  success: 'border-success text-success',
};

export function Alert({ kind, children }: { kind: 'error' | 'success'; children: ReactNode }) {
  return (
    <div role={kind === 'error' ? 'alert' : 'status'} className={`rounded-lg border bg-surface px-4 py-3 text-sm ${styles[kind]}`}>
      {children}
    </div>
  );
}
