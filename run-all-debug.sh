#!/usr/bin/env bash
set -e

ROOT="$(cd "$(dirname "$0")" && pwd)"

if [ -f "$ROOT/.env" ]; then
  set -a; source "$ROOT/.env"; set +a
else
  echo "ERROR: .env file not found. Copy .env.example and fill in your secrets." >&2
  exit 1
fi

echo "=== Tasks Management App — Debug Mode ==="
echo "Starting: AuthService (5100/5101), TaskService (5200/5201), Gateway (5000)"
echo ""
echo "Pre-requisites:"
echo "  dotnet ef database update --project Backend/src/AuthService"
echo "  dotnet ef database update --project Backend/src/TaskService"
echo ""

dotnet run --configuration Debug --project "$ROOT/Backend/src/AuthService" &
AUTH_PID=$!

dotnet run --configuration Debug --project "$ROOT/Backend/src/TaskService" &
TASK_PID=$!

dotnet run --configuration Debug --project "$ROOT/Backend/src/Gateway" &
GW_PID=$!

echo "Services started (dotnet run PIDs):"
echo "  AuthService : $AUTH_PID"
echo "  TaskService : $TASK_PID"
echo "  Gateway     : $GW_PID"
echo ""
echo "Attach Rider debugger:"
echo "  Run > Attach to Process (Ctrl+Alt+F5)"
echo "  Look for processes named: AuthService, TaskService, Gateway"
echo ""
echo "Press Ctrl+C to stop all services"

cleanup() {
  echo ""
  echo "Stopping all services..."
  kill $AUTH_PID $TASK_PID $GW_PID 2>/dev/null
  exit 0
}
trap cleanup INT TERM

wait
