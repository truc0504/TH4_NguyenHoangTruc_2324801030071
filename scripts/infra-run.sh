#!/usr/bin/env sh
set -eu
cd "$(dirname "$0")"
docker compose -f infra.yml up -d
