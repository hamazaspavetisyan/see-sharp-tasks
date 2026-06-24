#!/usr/bin/env bash
set -e

ROOT="$(cd "$(dirname "$0")" && pwd)"

echo "=== Tasks Management App ==="
echo "Starting: AuthService (5100/5101), TaskService (5200/5201), Gateway (5000), Frontend (4200)"
echo ""
echo "Pre-requisites:"
echo "  dotnet user-secrets set 'Jwt:Secret' '<your-32-char-secret>' --project src/AuthService"
echo "  dotnet user-secrets set 'Jwt:Secret' '<your-32-char-secret>' --project src/TaskService"
echo "  dotnet user-secrets set 'Jwt:Secret' '<your-32-char-secret>' --project src/Gateway"
echo "  dotnet ef database update --project src/AuthService"
echo "  dotnet ef database update --project src/TaskService"
echo ""

# Start each service in background
dotnet run --project "$ROOT/src/AuthService" &
AUTH_PID=$!

dotnet run --project "$ROOT/src/TaskService" &
TASK_PID=$!

dotnet run --project "$ROOT/src/Gateway" &
GW_PID=$!

cd "$ROOT/Frontend" && ng serve --port 4200 &
FE_PID=$!

echo "PIDs: AuthService=$AUTH_PID, TaskService=$TASK_PID, Gateway=$GW_PID, Frontend=$FE_PID"
echo ""
echo "Press Ctrl+C to stop all services"

cleanup() {
  echo ""
  echo "Stopping all services..."
  kill $AUTH_PID $TASK_PID $GW_PID $FE_PID 2>/dev/null
  exit 0
}
trap cleanup INT TERM

wait
