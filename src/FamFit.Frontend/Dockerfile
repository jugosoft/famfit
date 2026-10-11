# ============================================
# Этап 1: Сборка (Node 22)
# ============================================
FROM node:22-alpine AS build
WORKDIR /app

# Копируем package.json и package-lock.json
COPY package*.json ./

# Устанавливаем зависимости
RUN npm ci --only=production && npm cache clean --force

# Копируем исходный код
COPY . .

# Собираем приложение
RUN npm run build

# ============================================
# Этап 2: Nginx (production)
# ============================================
FROM nginx:alpine AS production
WORKDIR /usr/share/nginx/html

# Копируем собранные файлы
COPY --from=build /app/dist .

# Копируем конфиг Nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Открываем порт 80
EXPOSE 80

# Пользователь nginx
RUN chown -R nginx:nginx /usr/share/nginx/html
RUN chmod -R 755 /usr/share/nginx/html

CMD ["nginx", "-g", "daemon off;"]
