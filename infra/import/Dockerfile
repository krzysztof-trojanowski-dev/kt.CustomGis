FROM debian:bookworm-slim

RUN apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        osm2pgsql postgresql-client ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Build context is the infra directory (see build-image.ps1)
COPY gis.lua /opt/osm/gis.lua
COPY import.sh /opt/osm/import.sh
COPY data2import/ /data/
RUN chmod +x /opt/osm/import.sh

ENTRYPOINT ["/opt/osm/import.sh"]
