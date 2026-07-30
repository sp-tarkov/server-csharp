#!/bin/sh
set -eu

APP_DIR="/opt/spt"
CONFIG="${APP_DIR}/SPT_Data/configs/http.json"
SERVER_BIN="${APP_DIR}/SPT.Server.Linux"

# ---------------------------------------------------------------------------
# Network Configuration
#   SPT_IP            bind address        default 0.0.0.0 (reachable from host)
#   SPT_PORT          listen port         default 6969
#   SPT_BACKEND_IP    advertised to game  default 127.0.0.1 (address the server tells the client to use)
#   SPT_BACKEND_PORT  advertised port     default = SPT_PORT
# ---------------------------------------------------------------------------
SPT_IP="${SPT_IP:-0.0.0.0}"
SPT_PORT="${SPT_PORT:-6969}"
SPT_BACKEND_IP="${SPT_BACKEND_IP:-127.0.0.1}"
SPT_BACKEND_PORT="${SPT_BACKEND_PORT:-$SPT_PORT}"

if [ -f "$CONFIG" ]; then
    tmp="${CONFIG}.tmp"
    if jq \
        --arg ip "$SPT_IP" \
        --argjson port "$SPT_PORT" \
        --arg bip "$SPT_BACKEND_IP" \
        --argjson bport "$SPT_BACKEND_PORT" \
        '.ip = $ip | .port = $port | .backendIp = $bip | .backendPort = $bport' \
        "$CONFIG" > "$tmp"; then
        mv "$tmp" "$CONFIG"
        chmod 0644 "$CONFIG"
        echo "[entrypoint] Updated listen ${SPT_IP}:${SPT_PORT} and backend ${SPT_BACKEND_IP}:${SPT_BACKEND_PORT}"
    else
        rm -f "$tmp"
        echo "[entrypoint] WARNING: failed to rewrite ${CONFIG}" >&2
    fi
else
    echo "[entrypoint] WARNING: ${CONFIG} not found; skipping network config" >&2
fi

# Ensure persistent directories exist on mounted volume.
mkdir -p "${APP_DIR}/user/mods" "${APP_DIR}/user/profiles" "${APP_DIR}/user/logs" "${APP_DIR}/user/certs"

# Point HOME at the persistent volume so keys survive restarts.
export HOME="${APP_DIR}/user"

# Running as root lets us fix ownership of a bind-mounted ./user and then hand off to a non-root user.
if [ "$(id -u)" = "0" ]; then
    PUID="${PUID:-1000}"
    PGID="${PGID:-1000}"
    chown -R "${PUID}:${PGID}" "${APP_DIR}/user"
    echo "[entrypoint] starting server as ${PUID}:${PGID}"
    exec gosu "${PUID}:${PGID}" "$SERVER_BIN" "$@"
else
    echo "[entrypoint] starting server as $(id -u):$(id -g)"
    exec "$SERVER_BIN" "$@"
fi
