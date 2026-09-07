# run-server.ps1 — launch Duluka.Server from ANY working directory.
#
# Safe to run from anywhere: the server pins its content root and database
# path to the EXE location (Program.cs C/1, Database.cs) — the CWD never
# matters. This script may also be invoked via run-server.cmd (double-click).
#
# Client secret source, in order:
#   1. an existing DULUKA_GitHub__ClientSecret environment variable
#   2. the local, git-ignored file Duluka\.server-secret (first line)
#   3. a one-time prompt — the value is then saved to that file
# The secret never touches any tracked file.

$ErrorActionPreference = 'Stop'

$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $here 'Duluka.Server\Duluka.Server.csproj'
$exe = Join-Path $here 'Duluka.Server\bin\Release\net10.0\Duluka.Server.exe'
$secretFile = Join-Path $here '.server-secret'

if (-not (Test-Path $exe)) {
    Write-Host "Server exe not found — building (Release)…"
    dotnet build $project -c Release --nologo
    if ($LASTEXITCODE -ne 0) { throw "Build failed." }
}

if ([string]::IsNullOrWhiteSpace($env:DULUKA_GitHub__ClientSecret)) {
    if (Test-Path $secretFile) {
        $env:DULUKA_GitHub__ClientSecret = (Get-Content $secretFile -TotalCount 1).Trim()
        Write-Host "Client secret loaded from $secretFile"
    }
    else {
        Write-Host "GitHub client secret is not set yet."
        Write-Host "(GitHub > Settings > Developer settings > GitHub Apps > 'Duluka Shadow' > Generate a client secret)"
        $sec = Read-Host 'Paste the client secret'
        if ([string]::IsNullOrWhiteSpace($sec)) { throw "No secret entered — server not started." }
        Set-Content -Path $secretFile -Value $sec -Encoding ascii
        $env:DULUKA_GitHub__ClientSecret = $sec
        Write-Host "Saved to $secretFile (git-ignored — it will not be committed)."
    }
}

Write-Host "Starting Duluka.Server on http://localhost:5000 … (Ctrl+C to stop)"
& $exe @args
exit $LASTEXITCODE
