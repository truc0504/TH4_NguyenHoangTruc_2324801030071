param([switch]$SkipBlazor)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$logDir = Join-Path $repoRoot 'artifacts/runtime'
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$services = @{
    AuthService = 6060; ChatService = 4099; ProductService = 5030
    PricingService = 5041; OfferService = 5045; PolicyService = 5050
    PolicySearchService = 5065; PaymentService = 5070
    DocumentService = 5080; DashboardService = 5035
    AgentPortalApiGateway = 8099
}
$started = @()
foreach ($service in $services.Keys) {
    $port = $services[$service]
    if (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue) {
        Write-Host "Already listening on $port; keeping existing process."
        continue
    }
    $serviceDir = Join-Path $repoRoot $service
    $dll = Join-Path $serviceDir "bin/Release/net8.0/$service.dll"
    if (-not (Test-Path -LiteralPath $dll)) { throw "Build Release first: $dll" }
    $arguments = '"' + $dll + '" --urls http://127.0.0.1:' + $port
    $process = Start-Process -FilePath 'dotnet' -ArgumentList $arguments -WorkingDirectory $serviceDir -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $logDir "$service.log") -RedirectStandardError (Join-Path $logDir "$service.error.log")
    $started += [pscustomobject]@{ service = $service; port = $port; processId = $process.Id }
}
if (-not $SkipBlazor) {
    if (-not (Get-NetTCPConnection -LocalPort 5081 -State Listen -ErrorAction SilentlyContinue)) {
        $process = Start-Process -FilePath 'dotnet' -ArgumentList 'run --project BlazorWasmClient/BlazorWasmClient.csproj --configuration Release --no-build --no-restore --urls http://127.0.0.1:5081' -WorkingDirectory $repoRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $logDir 'BlazorWasmClient.log') -RedirectStandardError (Join-Path $logDir 'BlazorWasmClient.error.log')
        $started += [pscustomobject]@{ service = 'BlazorWasmClient'; port = 5081; processId = $process.Id }
    }
}
$processFile = Join-Path $logDir 'processes.json'
$previous = if (Test-Path -LiteralPath $processFile) { @(Get-Content -LiteralPath $processFile -Raw | ConvertFrom-Json) } else { @() }
@($previous + $started) | ConvertTo-Json | Set-Content -LiteralPath $processFile -Encoding UTF8
$started | Format-Table -AutoSize
