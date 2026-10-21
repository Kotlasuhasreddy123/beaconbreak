# Start the web server first using START-GAME.ps1. This adds a temporary public
# HTTPS link. Close this window or press Ctrl+C to stop sharing.
$ErrorActionPreference = 'Stop'
$tunnelPath = Join-Path $PSScriptRoot 'tools\cloudflared.exe'
if (-not (Test-Path -LiteralPath $tunnelPath)) {
    New-Item -ItemType Directory -Path (Join-Path $PSScriptRoot 'tools') -Force | Out-Null
    Invoke-WebRequest -Uri 'https://github.com/cloudflare/cloudflared/releases/download/2026.10.3/cloudflared-windows-amd64.exe' -OutFile $tunnelPath
}
$expected = '76B2805253D0D6FFACB092A7ACFBEE99E7AF6917835C0C5410E23F3EF5052DA2'
if ((Get-FileHash -LiteralPath $tunnelPath -Algorithm SHA256).Hash -ne $expected) {
    throw 'Tunnel client checksum did not match the official release. Stop and inspect the file.'
}
& $tunnelPath tunnel --url http://127.0.0.1:8787 --no-autoupdate
