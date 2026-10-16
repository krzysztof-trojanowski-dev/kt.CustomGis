param([string]$Tag = '1.0')
$ErrorActionPreference = 'Stop'
$cluster = "registry:5000/osm-import:$Tag"
$hostTag = "localhost:5000/osm-import:$Tag"
docker build -t $cluster -t $hostTag $PSScriptRoot
if ($LASTEXITCODE) { throw 'build failed' }
docker push $hostTag
if ($LASTEXITCODE) { throw 'push failed' }
