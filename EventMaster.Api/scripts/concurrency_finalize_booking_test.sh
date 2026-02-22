#!/usr/bin/env bash
set -euo pipefail

# Usage:
# API_BASE=http://localhost:8081 TOKEN="<jwt>" OCCURRENCE_ID=1 ./EventMaster.Api/scripts/concurrency_finalize_booking_test.sh

API_BASE="${API_BASE:-http://localhost:8081}"
TOKEN="${TOKEN:-}"
OCCURRENCE_ID="${OCCURRENCE_ID:-1}"

if [[ -z "$TOKEN" ]]; then
  echo "TOKEN is required"
  exit 1
fi

run_one() {
  local seat="$1"
  curl -sS -X POST "$API_BASE/api/payment/finalize-booking" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "{\"occurrenceId\":$OCCURRENCE_ID,\"quantity\":1,\"seats\":[\"$seat\"],\"nameOnCard\":\"John Doe\",\"cardNumber\":\"12345678901234\",\"exp\":\"12/30\",\"cvv\":\"123\",\"postalCode\":\"A1A1A1\"}" \
    -w "\nHTTP_STATUS:%{http_code}\n"
}

echo "Launching concurrent finalize requests for same seat A1..."
run_one "A1" > /tmp/finalize_1.out &
PID1=$!
run_one "A1" > /tmp/finalize_2.out &
PID2=$!
wait $PID1 || true
wait $PID2 || true

echo "--- Response 1 ---"
cat /tmp/finalize_1.out

echo "--- Response 2 ---"
cat /tmp/finalize_2.out

echo "Expected: one success (200), one conflict (409)."
