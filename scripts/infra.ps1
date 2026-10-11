param([ValidateSet('up','down','logs','build','status')][string]$Action = 'up')
$ErrorActionPreference = 'Stop'
$composeFile = Join-Path $PSScriptRoot 'infra.yml'
switch ($Action) {
    up { & docker compose -f $composeFile up -d --build }
    down { & docker compose -f $composeFile down }
    logs { & docker compose -f $composeFile logs --tail=100 }
    build { & docker compose -f $composeFile build }
    status { & docker compose -f $composeFile ps }
}
if ($LASTEXITCODE -ne 0) { throw "Infrastructure command failed: $Action" }
