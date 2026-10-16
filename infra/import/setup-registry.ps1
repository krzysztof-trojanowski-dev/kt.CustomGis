param([string]$Node = 'desktop-control-plane', [string]$Registry = 'registry')
$ErrorActionPreference = 'Stop'
$nets = docker inspect $Registry --format '{{range $k,$v := .NetworkSettings.Networks}}{{$k}} {{end}}'
if ($nets -notmatch '\bkind\b') { docker network connect kind $Registry }
docker exec $Node mkdir -p "/etc/containerd/certs.d/${Registry}:5000"
$toml = "[host.`"http://${Registry}:5000`"]`ncapabilities = [`"pull`", `"resolve`"]`n"
$toml | docker exec -i $Node sh -c "cat > /etc/containerd/certs.d/${Registry}:5000/hosts.toml"
docker exec $Node cat "/etc/containerd/certs.d/${Registry}:5000/hosts.toml"
