#!/usr/bin/env bash
# ============================================
# FamFit — Запуск Backend + Frontend одной командой
# Backend запускается в фоне, Frontend — в текущем процессе
# ============================================

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  FamFit — Запуск всего стека"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# --- Backend в фоновом процессе ---
echo "🚀 Запуск Backend (фоновый процесс)..."
echo "   Бэкенд: http://localhost:5000"
echo "   Swagger: http://localhost:5000/swagger"
echo ""

# Запускаем backend в фоне и сохраняем PID
bash "$SCRIPT_DIR/backend.sh" &
BACKEND_PID=$!

# Ждём пока backend поднимется (проверяем /health)
echo "⏳ Ожидание запуска Backend..."
for i in $(seq 1 30); do
  if curl -s http://localhost:5000/health > /dev/null 2>&1; then
    echo "✅ Backend запущен"
    break
  fi
  sleep 1
done

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  Запуск Frontend (текущий процесс)..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# --- Frontend в текущем процессе ---
# При Ctrl+C убиваем backend
cleanup() {
  echo ""
  echo "🛑 Остановка Backend (PID: $BACKEND_PID)..."
  kill $BACKEND_PID 2>/dev/null
  wait $BACKEND_PID 2>/dev/null
  echo "✅ Готово"
  exit 0
}
trap cleanup SIGINT SIGTERM

bash "$SCRIPT_DIR/frontend.sh"
