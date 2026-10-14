#!/usr/bin/env bash
# ============================================
# FamFit - Run Backend + Frontend together
# Backend runs in background, Frontend in foreground
# ============================================

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "=================================================="
echo "  FamFit - Full Stack"
echo "=================================================="
echo ""

# Kill previous process using port 5000
echo "Checking port 5000..."
BACKEND_PID_EXISTING=$(netstat -ano 2>/dev/null | grep ":5000 " | grep "LISTENING" | awk '{print $5}' | head -n 1)
if [ -n "$BACKEND_PID_EXISTING" ] && [ "$BACKEND_PID_EXISTING" != "0" ]; then
  echo "Process found on port 5000 (PID: $BACKEND_PID_EXISTING), stopping..."
  taskkill //PID $BACKEND_PID_EXISTING //F 2>/dev/null
  sleep 1
  echo "Stopped"
else
  echo "Port is free"
fi
echo ""

# Start Backend in background
echo "Starting Backend (background)..."
echo "   Backend:  http://localhost:5000"
echo "   Swagger:  http://localhost:5000/swagger"
echo ""

bash "$SCRIPT_DIR/backend.sh" &
BACKEND_PID=$!

# Wait for backend to be ready
echo "Waiting for Backend..."
for i in $(seq 1 30); do
  if curl -s http://localhost:5000/health > /dev/null 2>&1; then
    echo "Backend is ready"
    break
  fi
  sleep 1
done

echo ""
echo "=================================================="
echo "  Starting Frontend (foreground)"
echo "=================================================="
echo ""

# Cleanup function for Ctrl+C
cleanup() {
  echo ""
  echo "Stopping Backend (PID: $BACKEND_PID)..."
  kill $BACKEND_PID 2>/dev/null
  wait $BACKEND_PID 2>/dev/null
  echo "Done"
  exit 0
}
trap cleanup SIGINT SIGTERM

bash "$SCRIPT_DIR/frontend.sh"
