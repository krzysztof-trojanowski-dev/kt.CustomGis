# Infra

## Contents

| Path | Description |
|---|---|
| `postgres.yml` | PostgreSQL + PostGIS (`postgis/postgis:17-3.5`) StatefulSet, ConfigMap with credentials and Service |
| `import/gis.lua` | osm2pgsql flex style (tables `poi`, `roads`, `buildings`, `places`, SRID 4326) |
| `import/import.sh` | Import script run inside the container: extensions, osm2pgsql, indexes |
| `import/Dockerfile` | Import image (`debian:bookworm-slim` + `osm2pgsql` + `psql`), contains the PBF file |
| `import/data2import/` | Folder with the `.osm.pbf` file that is baked into the image |
| `import/build-image.ps1` | Builds the image and pushes it to the local registry |
| `import/setup-registry.ps1` | Makes the local registry reachable from the cluster node |
| `import/import-job.yml` | Kubernetes Job that runs the import |

## Importing OSM data

The import runs as a Kubernetes Job. Run all commands from the repository root.

### Prerequisites
- Docker Desktop with Kubernetes (kind node `desktop-control-plane`)
- Local registry container `registry` (port 5000)
- PostgreSQL deployed: `kubectl apply -f infra\postgres.yml`

### Steps

1. Put the `.osm.pbf` file (e.g. from Geofabrik) into `infra\import\data2import\`.
   If its name differs from `dolnoslaskie-261011.osm.pbf`, change `PBF_FILE` in `infra\import\import-job.yml`.

2. Make the registry reachable from the cluster node (once, and again after the cluster is recreated):
   ```powershell
   .\infra\import\setup-registry.ps1
   ```
   It connects the `registry` container to the `kind` network and configures containerd in the node to pull `registry:5000` over HTTP.

3. Build and push the image (repeat when the PBF, `gis.lua` or `import.sh` changes):
   ```powershell
   .\infra\import\build-image.ps1
   ```
   If you change the tag (`-Tag`), update `image:` in `import-job.yml` accordingly.

4. Run the Job:
   ```powershell
   kubectl delete job osm-import --ignore-not-found
   kubectl apply -f infra\import\import-job.yml
   kubectl logs -f job/osm-import
   ```
   A Job template is immutable, so delete the old Job before applying again.
   The import uses `--create`, so existing `poi`, `roads`, `buildings` and `places` tables are overwritten.

5. Verify:
   ```powershell
   kubectl exec postgres-0 -- psql -U ps_user -d ps_db -c "select count(*) from poi"
   ```

### Resulting tables

| Table | Geometry | Key columns |
|---|---|---|
| `poi` | point | `name`, `category` (amenity/shop/tourism/...), `subtype`, `tags` (jsonb) |
| `roads` | linestring | `name`, `highway`, `ref`, `maxspeed`, `tags` |
| `buildings` | multipolygon | `name`, `building`, `levels`, `tags` |
| `places` | point | `name`, `place`, `population`, `tags` |

All tables have `osm_id` and a `geom` column (EPSG:4326, GiST index created by osm2pgsql).
Trigram indexes on `name` and B-tree indexes on categories are created by `import.sh`.

### Notes
- The warning `database "ps_db" has a collation version mismatch` is harmless.
  To remove it: `ALTER DATABASE ps_db REFRESH COLLATION VERSION;`
- The PBF file is baked into the image (over 180 MB), so rebuilds and pushes take a moment.
- The Job needs about 4 GiB of memory (`limits.memory` in the manifest).
