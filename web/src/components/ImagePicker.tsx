import { useEffect, useRef, useState, type ChangeEvent } from 'react';
import { az } from '../i18n/az';
import { ACCEPTED_IMAGE_TYPES, MAX_IMAGES, refuseReason } from '../images';
import { Button } from './Button';

export type PickedImage = {
  key: number;
  file: File;
  previewUrl: string;
};

const SMALL = 'rounded-lg border border-line bg-surface px-2 py-1.5 text-sm font-medium hover:bg-page disabled:cursor-not-allowed disabled:opacity-50';

// Holds the chosen files in the browser until the form saves; nothing is sent from here.
export function ImagePicker({ pictures, onChange, error, disabled }: { pictures: PickedImage[]; onChange: (pictures: PickedImage[]) => void; error?: string; disabled: boolean }) {
  const input = useRef<HTMLInputElement>(null);
  const nextKey = useRef(0);
  const latest = useRef(pictures);
  latest.current = pictures;
  const [refused, setRefused] = useState<{ name: string; message: string }[]>([]);
  const text = az.images;

  // Previews hold memory in the browser until released.
  useEffect(
    () => () => {
      latest.current.forEach((picture) => URL.revokeObjectURL(picture.previewUrl));
    },
    [],
  );

  const add = (event: ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(event.target.files ?? []);
    event.target.value = '';

    const next = [...pictures];
    const nextRefused: { name: string; message: string }[] = [];
    for (const file of files) {
      const reason = refuseReason(file, MAX_IMAGES - next.length);
      if (reason) {
        nextRefused.push({ name: file.name, message: reason });
      } else {
        next.push({ key: nextKey.current++, file, previewUrl: URL.createObjectURL(file) });
      }
    }
    setRefused(nextRefused);
    onChange(next);
  };

  const move = (from: number, to: number) => {
    const next = [...pictures];
    const [moved] = next.splice(from, 1);
    next.splice(to, 0, moved);
    onChange(next);
  };

  const remove = (picture: PickedImage) => {
    URL.revokeObjectURL(picture.previewUrl);
    onChange(pictures.filter((item) => item.key !== picture.key));
  };

  return (
    <div>
      <p className="mb-1 block text-sm font-medium">{text.pickerTitle}</p>
      <input ref={input} data-testid="form-image-input" type="file" accept={ACCEPTED_IMAGE_TYPES.join(',')} multiple onChange={add} disabled={disabled} className="sr-only" />
      {pictures.length < MAX_IMAGES ? (
        <Button type="button" variant="secondary" disabled={disabled} onClick={() => input.current?.click()}>
          {text.pick}
        </Button>
      ) : (
        <p className="text-sm text-muted">{text.pickerFull}</p>
      )}
      <p className="mt-2 text-sm text-muted">{text.hint}</p>
      {error ? <p className="mt-1 text-sm text-danger">{error}</p> : null}
      {refused.length > 0 ? (
        <ul className="mt-3 space-y-1">
          {refused.map((item, index) => (
            <li key={index} data-testid="refused-file" className="text-sm text-danger">
              <span className="font-medium break-all">{item.name}</span>: {item.message}
            </li>
          ))}
        </ul>
      ) : null}
      {pictures.length > 0 ? (
        <ul className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
          {pictures.map((picture, index) => (
            <li key={picture.key} data-testid="picked-image" data-name={picture.file.name} className="overflow-hidden rounded-xl border border-line bg-surface">
              <div className="relative aspect-[4/3] bg-line">
                <img src={picture.previewUrl} alt={text.position(index + 1)} className="h-full w-full object-cover" />
                {index === 0 ? <span className="absolute top-2 left-2 rounded-full bg-accent px-3 py-1 text-xs font-medium text-white">{text.cover}</span> : null}
              </div>
              <div className="flex flex-wrap gap-2 p-3">
                <button type="button" className={SMALL} disabled={disabled || index === 0} onClick={() => move(index, index - 1)}>
                  {text.moveBack}
                </button>
                <button type="button" className={SMALL} disabled={disabled || index === pictures.length - 1} onClick={() => move(index, index + 1)}>
                  {text.moveForward}
                </button>
                {index > 0 ? (
                  <button type="button" className={SMALL} disabled={disabled} onClick={() => move(index, 0)}>
                    {text.makeCover}
                  </button>
                ) : null}
                <button type="button" className={`${SMALL} text-danger`} disabled={disabled} onClick={() => remove(picture)}>
                  {text.removePicked}
                </button>
              </div>
            </li>
          ))}
        </ul>
      ) : null}
    </div>
  );
}
