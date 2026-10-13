#!/usr/bin/env bash
# ============================================
# FamFit — Запуск Frontend с выбором окружения
# ============================================
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  FamFit Frontend — Выбор окружения"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

cd "$ROOT_DIR/src/FamFit.Frontend"

# Определяем окружение из аргумента или спрашиваем
ENV_NAME="${1:-local}"

case "$ENV_NAME" in
  local|dev|development)
    echo ""
    echo "📌 Окружение: Локальное"
    echo "   Backend:  http://localhost:5000"
    echo "   Frontend: http://localhost:5173"
    echo ""
    cp .env.development .env.local
    ;;
  staging|test|remote)
    echo ""
    echo "📌 Окружение: Удалённое (192.168.1.97)"
    echo "   Backend:  http://192.168.1.97"
    echo "   Frontend: http://localhost:5173"
    echo "   CORS:     Обходится через Vite proxy"
    echo ""
    cp .env.staging .env.local
    ;;
  lan|network)
    echo ""
    echo "📌 Окружение: LAN (192.168.1.97 + Origin rewrite)"
    echo "   Backend:  http://192.168.1.97"
    echo "   Frontend: http://localhost:5173"
    echo "   CORS:     changeOrigin + Origin header rewrite"
    echo ""
    cp .env.lan .env.local
    ;;
  production|prod)
    echo ""
    echo "📌 Окружение: Продакшен"
    echo "   Backend:  /api (через Nginx proxy)"
    echo "   Frontend: build для production"
    echo ""
    cp .env.production .env.local
    ;;
  *)
    echo ""
    echo "❌ Неизвестное окружение: $ENV_NAME"
    echo ""
    echo "Доступные окружения:"
    echo "  local/dev/development  — Локальная разработка"
    echo "  staging/test/remote    — Удалённый бэкенд (192.168.1.97)"
    echo "  lan/network            — LAN с Origin rewrite"
    echo "  production/prod        — Продакшен сборка"
    echo ""
    exit 1
    ;;
esac

echo "📦 Установка зависимостей..."
npm install

echo ""
echo "🚀 Запуск dev-сервера..."
echo ""

if [ "$ENV_NAME" = "production" ]; then
  echo "📦 Сборка для production..."
  npm run build
  echo ""
  echo "✅ Сборка завершена. Запустите:"
  echo "   npm run preview"
else
  npm run dev
fi
