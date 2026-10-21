$ErrorActionPreference = 'Stop'
$webDirectory = $PSScriptRoot
$pythonPath = Join-Path $webDirectory '.venv\Scripts\python.exe'
if (-not (Test-Path -LiteralPath $pythonPath)) {
    python -m venv (Join-Path $webDirectory '.venv')
    if ($LASTEXITCODE -ne 0) { throw 'Could not create the Python environment.' }
    & $pythonPath -m pip install -r (Join-Path $webDirectory 'requirements.txt')
    if ($LASTEXITCODE -ne 0) { throw 'Could not install the web server dependencies.' }
}
Write-Host 'Open http://localhost:8787 on this computer.'
Write-Host 'On the other device, use this computer''s Wi-Fi address with :8787.'
Write-Host 'Keep this window open. Ctrl+C stops the server.'
& $pythonPath (Join-Path $webDirectory 'server.py')
