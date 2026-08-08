#!/usr/bin/env sh
set -eu

APP_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
cd "$APP_DIR"
mkdir -p data

export Database__Provider=${Database__Provider:-Sqlite}
export ConnectionStrings__DevicePulse=${ConnectionStrings__DevicePulse:-"Data Source=$APP_DIR/data/devicepulse.db"}
export ASPNETCORE_URLS=${ASPNETCORE_URLS:-http://localhost:5000}

exec dotnet DevicePulse.Api.dll
