#!/usr/bin/env sh
set -eu

ROOT=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
OUTPUT=${1:-"$ROOT/publish/devicepulse"}

cd "$ROOT/frontend"
npm install
npm run build

rm -rf "$OUTPUT"
dotnet publish "$ROOT/backend/DevicePulse.Api/DevicePulse.Api.csproj" \
  --configuration Release \
  --output "$OUTPUT"
rm -rf "$OUTPUT/wwwroot"
cp -R "$ROOT/frontend/dist" "$OUTPUT/wwwroot"
cp "$ROOT/scripts/run-devicepulse.sh" "$OUTPUT/run.sh"
cp "$ROOT/scripts/run-devicepulse.ps1" "$OUTPUT/run.ps1"
chmod +x "$OUTPUT/run.sh"

printf 'DevicePulse publicado em %s (%s)\n' "$OUTPUT" "$(du -sh "$OUTPUT" | cut -f1)"
