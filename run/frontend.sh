#!/usr/bin/env bash
# ============================================
# FamFit — Запуск Frontend (локально)
# ============================================
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  FamFit Frontend"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

cd "$ROOT_DIR/src/FamFit.Frontend"

echo ""
echo "📦 Установка зависимостей..."
npm install

echo ""
echo "🚀 Запуск dev-сервера..."
echo "   Фронтенд: http://localhost:5173"
echo "   Прокси к бэкенду: http://localhost:5000/api"
echo ""

npm run dev
