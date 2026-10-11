param([Parameter(Mandatory=$true)][ValidateSet('lab_netmicro_payments','lab_netmicro_jobs','lab_netmicro_policy','lab_netmicro_pricing')][string]$Database)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$backup = Join-Path $repoRoot "artifacts/database-backups/$Database.backup"
if (-not (Test-Path -LiteralPath $backup)) { throw "Backup not found: $backup" }
$composeFile = Join-Path $PSScriptRoot 'infra.yml'
& docker compose -f $composeFile cp $backup "postgres:/tmp/$Database.backup"
if ($LASTEXITCODE -ne 0) { throw 'Copy failed.' }
# Restore into an empty database; no --clean option and no automatic deletion.
& docker compose -f $composeFile exec -T postgres pg_restore -U postgres -d $Database --no-owner --no-acl --exit-on-error "/tmp/$Database.backup"
if ($LASTEXITCODE -ne 0) { throw 'Restore failed. Target database must be empty.' }
