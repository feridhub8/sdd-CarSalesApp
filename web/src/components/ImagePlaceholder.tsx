import { az } from '../i18n/az';

export function ImagePlaceholder() {
  return (
    <div className="flex h-full w-full flex-col items-center justify-center gap-2 bg-line text-muted">
      <svg viewBox="0 0 24 24" aria-hidden="true" className="h-8 w-8 fill-none stroke-current stroke-[1.5]">
        <path d="M4 17v-3l2-5a2 2 0 0 1 1.9-1.3h8.2A2 2 0 0 1 18 9l2 5v3a1 1 0 0 1-1 1h-1a2 2 0 0 1-4 0h-4a2 2 0 0 1-4 0H5a1 1 0 0 1-1-1Zm3-3h10" />
      </svg>
      <span className="text-sm">{az.listing.noImage}</span>
    </div>
  );
}
