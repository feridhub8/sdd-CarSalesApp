import type { UploadResult } from '../images';

export function UploadRows({ uploads }: { uploads: UploadResult[] }) {
  if (uploads.length === 0) {
    return null;
  }

  return (
    <ul className="mt-4 space-y-2">
      {uploads.map((upload) => (
        <li key={upload.key} data-testid="upload-row" data-state={upload.state} className="flex flex-wrap justify-between gap-x-4 gap-y-1 border-t border-line pt-2 text-sm">
          <span className="min-w-0 font-medium break-all">{upload.name}</span>
          <span className={upload.state === 'failed' ? 'text-danger' : upload.state === 'done' ? 'text-success' : 'text-muted'}>{upload.message}</span>
        </li>
      ))}
    </ul>
  );
}
