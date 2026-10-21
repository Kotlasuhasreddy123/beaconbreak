$ErrorActionPreference = 'Stop'
$serverFile = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot 'server.py')).Path
$tunnelFile = Join-Path $PSScriptRoot 'tools\cloudflared.exe'
Get-CimInstance Win32_Process | Where-Object {
    ($_.Name -eq 'python.exe' -and $_.CommandLine -like ('*"' + $serverFile + '"*')) -or
    ($_.ExecutablePath -eq $tunnelFile -and $_.CommandLine -like '*127.0.0.1:8787*')
} | ForEach-Object { Stop-Process -Id $_.ProcessId }
Write-Host 'Stopped this project''s web server and public tunnel. No game files were deleted.'
