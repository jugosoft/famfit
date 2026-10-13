#!/usr/bin/env bash
# ============================================
# FamFit — Запуск Backend (локально)
# ============================================
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  FamFit Backend"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

cd "$ROOT_DIR/src/FamFit.WebApi"

echo ""
echo "📦 Восстановление зависимостей..."
dotnet restore

echo ""
echo "🔨 Сборка проекта..."
dotnet build --no-restore

echo ""
echo "🚀 Запуск сервера..."
echo "   Бэкенд: http://localhost:5000"
echo "   Swagger: http://localhost:5000/swagger"
echo ""

ASPNETCORE_ENVIRONMENT=Development dotnet run --urls "http://localhost:5000"
