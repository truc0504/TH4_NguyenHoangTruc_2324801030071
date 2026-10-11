$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$outputDir = Join-Path $repoRoot 'artifacts/database-backups'
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
$composeFile = Join-Path $PSScriptRoot 'infra.yml'
$databases = @('lab_netmicro_payments','lab_netmicro_jobs','lab_netmicro_policy','lab_netmicro_pricing')
foreach ($database in $databases) {
    & docker compose -f $composeFile exec -T postgres pg_dump -U postgres -Fc --no-owner --no-acl -f "/tmp/$database.backup" $database
    if ($LASTEXITCODE -ne 0) { throw "pg_dump failed: $database" }
    & docker compose -f $composeFile cp "postgres:/tmp/$database.backup" (Join-Path $outputDir "$database.backup")
    if ($LASTEXITCODE -ne 0) { throw "Copy failed: $database" }
    & docker compose -f $composeFile exec -T postgres pg_restore --list "/tmp/$database.backup"
    if ($LASTEXITCODE -ne 0) { throw "Backup verification failed: $database" }
}
Get-ChildItem -LiteralPath $outputDir -Filter '*.backup' | Get-FileHash -Algorithm SHA256 | Format-Table -AutoSize | Out-File (Join-Path $outputDir 'SHA256.txt')
