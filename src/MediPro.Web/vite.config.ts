import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  appType: 'spa',
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Match Kestrel on IPv4; "localhost" from Node can hit ::1 while the API listens on 127.0.0.1 only.
      '/api': {
        target: 'http://127.0.0.1:5020',
        changeOrigin: true,
      },
    },
  },
  preview: {
    port: 4173,
    proxy: {
      '/api': {
        target: 'http://127.0.0.1:5020',
        changeOrigin: true,
      },
    },
  },
})
