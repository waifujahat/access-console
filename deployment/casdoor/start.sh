#!/bin/sh
set -eu

: "${dataSourceName:?PostgreSQL connection is required}"
: "${origin:?Public HTTPS origin is required}"
: "${initAdminPassword:?Initial administrator password is required}"

case "$origin" in
    https://?*) ;;
    *) echo "Casdoor origin must use HTTPS" >&2; exit 1 ;;
esac

case "${origin#https://}" in
    */*|*\?*|*\#*) echo "Casdoor origin must not include a path, query or fragment" >&2; exit 1 ;;
esac

export originFrontend="$origin"
export httpport="${PORT:-8000}"

exec /server
