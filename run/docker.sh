#!/usr/bin/env bash
# ============================================
# FamFit — Запуск через Docker Compose
# ============================================
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  FamFit — Docker Compose"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

cd "$ROOT_DIR"

echo ""
echo "🐳 Сборка и запуск контейнеров..."
echo "   PostgreSQL: localhost:5432"
echo "   Backend:    localhost:5000"
echo "   Frontend:   localhost:80"
echo "   Swagger:    http://localhost:5000/swagger"
echo ""

docker-compose up --build -d

echo ""
echo "⏳ Ожидание запуска сервисов..."
sleep 10

echo ""
echo "📋 Статус контейнеров:"
docker-compose ps

echo ""
echo "📝 Логи:"
echo "   Backend:  docker-compose logs -f backend"
echo "   Frontend: docker-compose logs -f frontend"
echo "   Postgres: docker-compose logs -f postgres"
echo ""
echo "🛑 Остановка: docker-compose down"
