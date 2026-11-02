import { useEffect, useRef } from 'react';
import { az } from '../i18n/az';
import { Button } from './Button';

// The browser's own confirm box would show its buttons in the browser's language.
export function ConfirmDialog({ open, text, onConfirm, onCancel }: { open: boolean; text: string; onConfirm: () => void; onCancel: () => void }) {
  const dialog = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    if (open) {
      dialog.current?.showModal();
    } else {
      dialog.current?.close();
    }
  }, [open]);

  return (
    <dialog ref={dialog} onCancel={onCancel} className="m-auto w-[calc(100%-2rem)] max-w-sm rounded-xl border border-line bg-surface p-6 text-ink backdrop:bg-ink/40">
      <p>{text}</p>
      <div className="mt-6 flex justify-end gap-2">
        <Button variant="secondary" onClick={onCancel}>
          {az.common.cancel}
        </Button>
        <Button variant="danger" onClick={onConfirm}>
          {az.common.delete}
        </Button>
      </div>
    </dialog>
  );
}
