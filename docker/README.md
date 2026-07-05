# SPT Server - Docker

A slim Linux container image for the SPT server. Built and published to the GitHub Container Registry (GHCR) automatically when a version tag is pushed.

> A quick note... this is not the recommended way to install and use the server. This Docker method is still being tested and developed. If you run into any problems, share them in a Github issue and *do not bother support with it*. You're on your own. You've been warned.

## Versions

```
ghcr.io/sp-tarkov/server-csharp:latest             # newest stable release
ghcr.io/sp-tarkov/server-csharp:4.1.0              # exact stable version
ghcr.io/sp-tarkov/server-csharp:4.1                # latest 4.1.x stable
ghcr.io/sp-tarkov/server-csharp:4                  # latest 4.x stable
ghcr.io/sp-tarkov/server-csharp:edge               # latest bleeding-edge (pre-release)
ghcr.io/sp-tarkov/server-csharp:edge-mods          # latest bleeding-edge + mods (pre-release)
ghcr.io/sp-tarkov/server-csharp:4.1.0-BE-20260704  # a specific pre-release build
```

## Quick Start

```bash
docker run -d --name spt-server \
  -p 6969:6969 \
  -v ./spt-user:/opt/spt/user \
  ghcr.io/sp-tarkov/server-csharp:latest
```

Or with Compose (see `compose.yaml` in the repo root):

```bash
docker compose up -d
```

The server speaks HTTPS with a self-signed certificate it generates on first run into `user/certs/`. Point the SPT launcher at `https://<host>:6969`.

## Persistent Data

Everything the server writes lives under `/opt/spt/user`, exposed as a volume:

| Path | Contents |
|------|----------|
| `user/profiles/` | Player save data |
| `user/mods/` | Server mods |
| `user/certs/` | Generated self-signed cert |
| `user/logs/` | Server request logs |
| `user/credentials/` | Web-panel credentials |

## Configuration

| Variable | Default | Purpose |
|----------|---------|---------|
| `SPT_IP` | `0.0.0.0` | Bind address inside the container. Leave as `0.0.0.0` so the mapped port is reachable. |
| `SPT_PORT` | `6969` | Listen port. |
| `SPT_BACKEND_IP` | `127.0.0.1` | Address advertised to the game client. **Never `0.0.0.0`.** |
| `SPT_BACKEND_PORT` | = `SPT_PORT` | Port advertised to the client. |
| `PUID` / `PGID` | `1000` / `1000` | UID/GID the server runs as. Match your host user so the mounted `user/` data (mods, profiles) stays editable without root. |

## Building Locally

```bash
git lfs pull # Make sure SPT_Data is pulled
docker build -t spt-server:local \
  --build-arg SptVersion=4.1.0 \
  --build-arg SptCommit=$(git rev-parse --short=7 HEAD) \
  --build-arg SptBuildTime=$(date +%s) \
  --build-arg SptBuildType=RELEASE \
  .
```
