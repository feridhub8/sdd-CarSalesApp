import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

// Local runs on one origin too: the dev server passes API and file paths to the gateway,
// so the refresh token cookie works the same as in Docker.
const gateway = process.env.GATEWAY_URL ?? 'http://localhost:5080';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5081,
    strictPort: true,
    proxy: {
      '/api': gateway,
      '/listing-images-dev': gateway,
    },
  },
});
