#!/usr/bin/env bash
# End-to-end smoke test against a running stack (docker compose up).
# Usage: ADMIN_PASSWORD=... [WEB=http://localhost:3000] [API=http://localhost:5000] scripts/smoke-test.sh
set -euo pipefail

WEB=${WEB:-http://localhost:3000}
API=${API:-http://localhost:5000}
PW=${ADMIN_PASSWORD:?set ADMIN_PASSWORD}
TITLE="Smoke Test $(date +%s)"
SLUG=$(echo "$TITLE" | tr 'A-Z ' 'a-z-')

fail() { echo "FAIL: $*" >&2; exit 1; }
code() { curl -s -o /dev/null -w '%{http_code}' "$@"; }
expect() { [ "$2" = "$3" ] || fail "$1: expected $2, got $3"; echo "ok   $1"; }

echo "Waiting for API and web..."
for _ in $(seq 1 60); do
  [ "$(code "$API/health")" = 200 ] && [ "$(code "$WEB/")" = 200 ] && break
  sleep 2
done
expect "api health" 200 "$(code "$API/health")"
expect "public home renders" 200 "$(code "$WEB/")"

expect "admin ui requires password" 401 "$(code "$WEB/admin")"
expect "wrong password rejected" 401 "$(code -u me:wrong "$WEB/api/cms/content")"
expect "api rejects missing key" 401 "$(code "$API/api/admin/content")"

AUTH=(-u "me:$PW" -H 'Content-Type: application/json')
expect "admin ui with password" 200 "$(code -u "me:$PW" "$WEB/admin")"

BODY="{\"kind\":\"Post\",\"title\":\"$TITLE\",\"body\":\"# Hello\\n\\n<script>alert(1)</script>\",\"tags\":[\"smoke\"]}"
RESP=$(curl -sf "${AUTH[@]}" -X POST "$WEB/api/cms/content" -d "$BODY") || fail "create via proxy"
ID=$(python3 -c 'import sys,json; print(json.load(sys.stdin)["id"])' <<<"$RESP")
echo "ok   created $ID (database write)"

expect "duplicate slug conflicts" 409 "$(code "${AUTH[@]}" -X POST "$WEB/api/cms/content" -d "$BODY")"
expect "draft is not public" 404 "$(code "$WEB/$SLUG")"
expect "publish" 200 "$(code "${AUTH[@]}" -X POST "$WEB/api/cms/content/$ID/publish")"
expect "published page renders" 200 "$(code "$WEB/$SLUG")"

PAGE=$(curl -s "$WEB/$SLUG")
grep -q "<h1>Hello</h1>" <<<"$PAGE" || fail "markdown not rendered"
grep -q "<script>alert(1)" <<<"$PAGE" && fail "raw script tag leaked into page"
echo "ok   markdown rendered, script escaped"
curl -s "$WEB/" | grep -q "$TITLE" || fail "published item missing from home page"
echo "ok   listed on home page"

SEO=$(curl -sf "${AUTH[@]}" -X POST "$WEB/api/cms/ai/seo" -d "{\"title\":\"$TITLE\",\"body\":\"Some body text.\"}") || fail "ai seo"
grep -q '"slug"' <<<"$SEO" || fail "ai seo response malformed"
echo "ok   ai endpoint"

expect "delete" 204 "$(code "${AUTH[@]}" -X DELETE "$WEB/api/cms/content/$ID")"
echo "Smoke test passed."
