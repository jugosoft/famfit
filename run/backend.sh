#!/usr/bin/env bash
# ============================================
# FamFit - Run Backend (local)
# ============================================
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "=================================================="
echo "  FamFit Backend"
echo "=================================================="

cd "$ROOT_DIR/src/FamFit.WebApi"

echo ""
echo "[1/3] Restoring dependencies..."
dotnet restore

echo ""
echo "[2/3] Building project..."
dotnet build --no-restore

echo ""
echo "[3/3] Applying database migrations..."
dotnet ef database update --project "../FamFit.Infrastructure/FamFit.Infrastructure.csproj"

echo ""
echo "Starting server..."
echo "   Backend:  http://localhost:5000"
echo "   Swagger:  http://localhost:5000/swagger"
echo ""

ASPNETCORE_ENVIRONMENT=Development dotnet run --urls "http://localhost:5000"
