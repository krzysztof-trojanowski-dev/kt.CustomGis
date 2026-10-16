#!/bin/bash
set -euo pipefail

# PBF_FILE, PGHOST, PGUSER, PGPASSWORD, PGDATABASE are provided as environment variables.
: "${PBF_FILE:?PBF_FILE is required}"
CACHE_MB="${CACHE_MB:-1500}"

psql -v ON_ERROR_STOP=1 -c "CREATE EXTENSION IF NOT EXISTS postgis; CREATE EXTENSION IF NOT EXISTS hstore; CREATE EXTENSION IF NOT EXISTS pg_trgm;"

osm2pgsql --create --slim --output=flex --style=/opt/osm/gis.lua \
    -H "$PGHOST" -U "$PGUSER" -d "$PGDATABASE" --cache "$CACHE_MB" "$PBF_FILE"

psql -v ON_ERROR_STOP=1 <<'SQL'
CREATE INDEX IF NOT EXISTS poi_name_trgm ON poi USING gin (name gin_trgm_ops);
CREATE INDEX IF NOT EXISTS poi_category_idx ON poi (category, subtype);
CREATE INDEX IF NOT EXISTS poi_tags_gin ON poi USING gin (tags);
CREATE INDEX IF NOT EXISTS roads_name_trgm ON roads USING gin (name gin_trgm_ops);
CREATE INDEX IF NOT EXISTS roads_highway_idx ON roads (highway);
CREATE INDEX IF NOT EXISTS buildings_building_idx ON buildings (building);
CREATE INDEX IF NOT EXISTS places_name_trgm ON places USING gin (name gin_trgm_ops);
CREATE INDEX IF NOT EXISTS places_place_idx ON places (place);
ANALYZE;
SQL
