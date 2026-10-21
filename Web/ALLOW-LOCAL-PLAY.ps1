# Run in an Administrator PowerShell. This permits only local-subnet devices,
# only TCP 8787, and only this project's Python executable.
$ErrorActionPreference = 'Stop'
$venvPython = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '.venv\Scripts\python.exe')).Path
$serverPython = & $venvPython -c "import sys; print(sys._base_executable)"
$ruleName = 'Beaconbreak local play TCP 8787'
if (-not (Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -DisplayName $ruleName -Direction Inbound -Action Allow `
        -Protocol TCP -LocalPort 8787 -RemoteAddress LocalSubnet -Program $serverPython -Profile Any
}
Write-Host 'Local-network play is allowed. To undo: Remove-NetFirewallRule -DisplayName "Beaconbreak local play TCP 8787"'
