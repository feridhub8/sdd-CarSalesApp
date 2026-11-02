import { useId, type SelectHTMLAttributes } from 'react';
import { az } from '../i18n/az';

type SelectFieldProps = SelectHTMLAttributes<HTMLSelectElement> & {
  label: string;
  error?: string;
  options: { value: string; label: string }[];
  // Null leaves out the empty choice, for a field that always has a value.
  placeholder?: string | null;
};

export function SelectField({ label, error, options, placeholder = az.common.choose, ...select }: SelectFieldProps) {
  const id = useId();
  const noteId = `${id}-note`;

  return (
    <div>
      <label htmlFor={id} className="mb-1 block text-sm font-medium">
        {label}
      </label>
      <select
        id={id}
        aria-invalid={error ? true : undefined}
        aria-describedby={error ? noteId : undefined}
        className={`h-[42px] w-full rounded-lg border bg-surface px-3 py-2 outline-none focus:ring-2 focus:ring-accent disabled:opacity-50 ${error ? 'border-danger' : 'border-line'}`}
        {...select}
      >
        {placeholder === null ? null : <option value="">{placeholder}</option>}
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      {error ? (
        <p id={noteId} className="mt-1 text-sm text-danger">
          {error}
        </p>
      ) : null}
    </div>
  );
}
