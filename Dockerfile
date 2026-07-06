# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# Build Stage
# ---------------------------------------------------------------------------
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG TARGETARCH
ARG SptVersion=4.1.0
ARG SptCommit=000000
ARG SptBuildTime=0000000000
ARG SptBuildType=RELEASE

WORKDIR /src
COPY . .

RUN case "${TARGETARCH}" in \
        amd64) RID=linux-x64 ;; \
        arm64) RID=linux-arm64 ;; \
        *) echo "Unsupported TARGETARCH: ${TARGETARCH}" >&2; exit 1 ;; \
    esac \
    && dotnet publish SPTarkov.Server/SPTarkov.Server.csproj \
    --configuration Release \
    --runtime "${RID}" \
    --self-contained false \
    -p:SptVersion="${SptVersion}" \
    -p:SptCommit="${SptCommit}" \
    -p:SptBuildTime="${SptBuildTime}" \
    -p:SptBuildType="${SptBuildType}" \
    --output /app

RUN cp SPTarkov.Server/sptLogger.Development.json /app/

# ---------------------------------------------------------------------------
# Runtime Stage
# ---------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

RUN apt-get update \
    && apt-get install -y --no-install-recommends jq gosu ca-certificates curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /opt/spt
COPY --from=build /app ./
COPY docker/entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh \
    && mkdir -p /opt/spt/user \
    && chown -R 1000:1000 /opt/spt/user

# Environment Defaults
ENV SPT_IP=0.0.0.0 \
    SPT_PORT=6969 \
    SPT_BACKEND_IP=0.0.0.0 \
    PUID=1000 \
    PGID=1000

EXPOSE 6969
VOLUME /opt/spt/user

HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
    CMD curl -fsSk "https://localhost:${SPT_PORT}/health" || exit 1

LABEL org.opencontainers.image.title="SPT Server" \
      org.opencontainers.image.description="Single Player Tarkov Server" \
      org.opencontainers.image.source="https://github.com/sp-tarkov/server-csharp"

# Starts as root to fix bind-mount ownership, then drops to PUID:PGID.
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
