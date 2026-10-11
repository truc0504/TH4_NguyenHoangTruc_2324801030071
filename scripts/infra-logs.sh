#!/usr/bin/env sh
set -eu
cd "$(dirname "$0")"
docker compose -f infra.yml logs --tail=100 -f
