import { defineConfig } from 'tailwindcss';

export default defineConfig({
  // Tailwind v4 использует @theme из CSS, но IntelliSense требует конфиг
  // Здесь только для VS Code подсказок
  content: [
    './src/**/*.{js,ts,jsx,tsx}',
  ],
  theme: {
    extend: {},
  },
  plugins: [],
});
