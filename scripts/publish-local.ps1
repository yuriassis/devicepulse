$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$output = if ($args[0]) { $args[0] } else { Join-Path $root "publish/devicepulse" }

Push-Location (Join-Path $root "frontend")
npm install
npm run build
Pop-Location

if (Test-Path $output) { Remove-Item -Recurse -Force $output }
dotnet publish (Join-Path $root "backend/DevicePulse.Api/DevicePulse.Api.csproj") `
    --configuration Release --output $output
Remove-Item -Recurse -Force (Join-Path $output "wwwroot")
Copy-Item -Recurse (Join-Path $root "frontend/dist") (Join-Path $output "wwwroot")
Copy-Item (Join-Path $root "scripts/run-devicepulse.sh") (Join-Path $output "run.sh")
Copy-Item (Join-Path $root "scripts/run-devicepulse.ps1") (Join-Path $output "run.ps1")

$size = [math]::Round((Get-ChildItem $output -Recurse | Measure-Object Length -Sum).Sum / 1MB, 1)
Write-Host "DevicePulse publicado em $output ($size MB)"
