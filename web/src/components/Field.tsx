import { useId, type InputHTMLAttributes } from 'react';

type FieldProps = InputHTMLAttributes<HTMLInputElement> & {
  label: string;
  error?: string;
  hint?: string;
};

export function Field({ label, error, hint, ...input }: FieldProps) {
  const id = useId();
  const noteId = `${id}-note`;

  return (
    <div>
      <label htmlFor={id} className="mb-1 block text-sm font-medium">
        {label}
      </label>
      <input
        id={id}
        aria-invalid={error ? true : undefined}
        aria-describedby={error || hint ? noteId : undefined}
        className={`w-full rounded-lg border bg-surface px-3 py-2 outline-none focus:ring-2 focus:ring-accent ${error ? 'border-danger' : 'border-line'}`}
        {...input}
      />
      {error ? (
        <p id={noteId} className="mt-1 text-sm text-danger">
          {error}
        </p>
      ) : hint ? (
        <p id={noteId} className="mt-1 text-sm text-muted">
          {hint}
        </p>
      ) : null}
    </div>
  );
}
