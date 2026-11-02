import type { ButtonHTMLAttributes } from 'react';

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: 'primary' | 'secondary' | 'danger';
};

const styles = {
  primary: 'bg-accent text-white hover:bg-accent-strong',
  secondary: 'border border-line bg-surface text-ink hover:bg-page',
  danger: 'border border-danger bg-surface text-danger hover:bg-page',
};

export function Button({ variant = 'primary', className = '', ...button }: ButtonProps) {
  return (
    <button
      className={`rounded-lg px-4 py-2 font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-50 ${styles[variant]} ${className}`}
      {...button}
    />
  );
}
