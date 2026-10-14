#!/usr/bin/env bash
# ============================================
# FamFit - Run Frontend (local)
# ============================================
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "=================================================="
echo "  FamFit Frontend"
echo "=================================================="

cd "$ROOT_DIR/src/FamFit.Frontend"

echo ""
echo "[1/2] Installing dependencies..."
npm install

echo ""
echo "[2/2] Starting dev server..."
echo "   Frontend: http://localhost:5173"
echo "   Proxy:    http://localhost:5000/api"
echo ""

npm run dev
