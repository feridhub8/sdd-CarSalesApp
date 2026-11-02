import { az } from './i18n/az';

export const MAX_IMAGES = 20;
export const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const MAX_FILE_BYTES = 10 * 1024 * 1024;

// Why a file cannot be sent, or null when it can. The server still checks the real content.
export function refuseReason(file: File, freePlaces: number) {
  if (!ACCEPTED_IMAGE_TYPES.includes(file.type)) {
    return az.images.status.wrongType;
  }
  if (file.size > MAX_FILE_BYTES) {
    return az.images.status.tooLarge;
  }
  return freePlaces <= 0 ? az.images.status.limit : null;
}

// One file's result, shown in a row on the form and on the image page.
export type UploadResult = {
  key: number;
  name: string;
  state: 'waiting' | 'uploading' | 'done' | 'failed';
  message: string;
};
