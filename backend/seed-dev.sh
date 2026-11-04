#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export ASPNETCORE_ENVIRONMENT=Development

dotnet run --project "$root/ProfileApi" --no-launch-profile -- --seed
dotnet run --project "$root/QueueApi" --no-launch-profile -- --seed
