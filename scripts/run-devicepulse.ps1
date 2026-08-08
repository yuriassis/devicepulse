$ErrorActionPreference = "Stop"
$appDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $appDirectory
New-Item -ItemType Directory -Force -Path "data" | Out-Null

if (-not $env:Database__Provider) { $env:Database__Provider = "Sqlite" }
if (-not $env:ConnectionStrings__DevicePulse) {
    $env:ConnectionStrings__DevicePulse = "Data Source=$appDirectory/data/devicepulse.db"
}
if (-not $env:ASPNETCORE_URLS) { $env:ASPNETCORE_URLS = "http://localhost:5000" }

dotnet DevicePulse.Api.dll
