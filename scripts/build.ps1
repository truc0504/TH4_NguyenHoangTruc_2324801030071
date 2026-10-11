param([switch]$SkipTests, [switch]$SkipWeb)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$artifactDir = Join-Path $repoRoot 'artifacts'
New-Item -ItemType Directory -Force -Path $artifactDir | Out-Null
Push-Location $repoRoot
try {
    & dotnet build InsuranceMicroservices.sln -c Release -m:1 -v minimal 2>&1 | Tee-Object -FilePath (Join-Path $artifactDir 'build-dotnet.log')
    if ($LASTEXITCODE -ne 0) { throw 'Solution build failed.' }
    if (-not $SkipTests) {
        & dotnet test InsuranceMicroservices.sln -c Release --no-build -m:1 --logger trx --results-directory artifacts/test-results 2>&1 | Tee-Object -FilePath (Join-Path $artifactDir 'test.log')
        if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
    }
    if (-not $SkipWeb) {
        Push-Location Web
        try {
            & npm.cmd ci --no-audit --no-fund --loglevel=error
            if ($LASTEXITCODE -ne 0) { throw 'npm ci failed.' }
            $previousNodeOptions = $env:NODE_OPTIONS
            $env:NODE_OPTIONS = '--openssl-legacy-provider'
            try {
                & npm.cmd run build 2>&1 | Tee-Object -FilePath (Join-Path $artifactDir 'build-web.log')
                if ($LASTEXITCODE -ne 0) { throw 'Vue build failed.' }
            } finally { $env:NODE_OPTIONS = $previousNodeOptions }
        } finally { Pop-Location }
    }
} finally { Pop-Location }
