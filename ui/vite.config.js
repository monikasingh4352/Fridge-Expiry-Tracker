import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// The UI runs on http://localhost:5173.
// Any call to /api is forwarded (proxied) to the .NET API on http://localhost:5080,
// so the browser sees one address and no CORS setup is needed for local development.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5080'
    }
  }
})
