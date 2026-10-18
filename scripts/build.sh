#!/usr/bin/env sh
set -eu
cd "$(dirname "$0")/.."
dotnet build InsuranceMicroservices.sln -c Release -m:1
dotnet test InsuranceMicroservices.sln -c Release --no-build -m:1
cd Web
npm ci --no-audit --no-fund
NODE_OPTIONS=--openssl-legacy-provider npm run build
