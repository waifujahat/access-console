#!/bin/sh
set -eu

: "${dataSourceName:?PostgreSQL connection is required}"
: "${origin:?Public HTTPS origin is required}"
: "${initAdminPassword:?Initial administrator password is required}"
: "${REDIS_HOST:?Redis TLS host is required}"
: "${REDIS_PASSWORD:?Redis password is required}"

case "$origin" in
    https://?*) ;;
    *) echo "Casdoor origin must use HTTPS" >&2; exit 1 ;;
esac

case "${origin#https://}" in
    */*|*\?*|*\#*) echo "Casdoor origin must not include a path, query or fragment" >&2; exit 1 ;;
esac

export originFrontend="$origin"
export httpport="${PORT:-8000}"

case "$REDIS_HOST" in
    *[!a-zA-Z0-9.-]*) echo "Invalid Redis host" >&2; exit 1 ;;
esac

redis_port="${REDIS_PORT:-6379}"
case "$redis_port" in
    ''|*[!0-9]*) echo "Invalid Redis port" >&2; exit 1 ;;
esac
if [ "$redis_port" -lt 1 ] || [ "$redis_port" -gt 65535 ]; then
    echo "Invalid Redis port" >&2
    exit 1
fi

case "$REDIS_PASSWORD" in
    *[!a-zA-Z0-9._+/=-]*) echo "Unsupported Redis password characters" >&2; exit 1 ;;
esac

# Casdoor's Redis session adapter uses TCP. Keep plaintext on loopback only;
# stunnel verifies the remote certificate and encrypts the external connection.
umask 077
config=$(mktemp /tmp/casdoor-redis.XXXXXX)
printf '%s\n' \
    'foreground = yes' \
    'pid =' \
    '[redis]' \
    'client = yes' \
    'accept = 127.0.0.1:6380' \
    "connect = $REDIS_HOST:$redis_port" \
    "sni = $REDIS_HOST" \
    'verifyChain = yes' \
    "checkHost = $REDIS_HOST" \
    'CAfile = /etc/ssl/certs/ca-certificates.crt' > "$config"

stunnel "$config" &
tunnel_pid=$!
trap 'kill "$tunnel_pid" 2>/dev/null || true' EXIT

# Confirm authentication and connectivity before starting Casdoor. No local
# session fallback is allowed and the password is never passed as an argument.
attempt=0
while [ "$attempt" -lt 10 ]; do
    # Use a protocol-aware client which waits for AUTH before sending PING.
    if response=$(VALKEYCLI_AUTH="$REDIS_PASSWORD" timeout 5 valkey-cli \
        -h 127.0.0.1 -p 6380 --raw PING 2>/dev/null) && [ "$response" = PONG ]; then
        export redisEndpoint="127.0.0.1:6380,10,$REDIS_PASSWORD,0"
        echo "Redis TLS session store ready"
        exec /server
    fi
    kill -0 "$tunnel_pid" 2>/dev/null || break
    attempt=$((attempt + 1))
    sleep 1
done

echo "Redis TLS authentication or connection failed" >&2
exit 1
