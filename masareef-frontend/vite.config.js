import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// الـ proxy بيوجّه أي طلب يبدأ بـ /api لسيرفر الباكند
// فمش محتاج تكتب العنوان الكامل في كل استدعاء، وبيحل مشكلة CORS وقت التطوير
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5238',
        changeOrigin: true,
      },
    },
  },
})
