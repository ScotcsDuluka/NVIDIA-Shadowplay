# stub-listener.ps1 — Duluka Account STUB (probe phase)
#
# Listens on 127.0.0.1:59870 (the jarvis.server the Duluka mod points to)
# and LOGS every request (method, path, headers, body) so we learn the
# EXACT jarvis API shapes the osc/GFE layer calls — the contract the real
# Duluka server must implement.
#
# Responds: 200 with an empty JSON object (harmless placeholder).
#
# Run:  powershell -File stub-listener.ps1
# Stop: Ctrl+C. Log: duluka-stub-capture.log (same folder).

$ErrorActionPreference = 'Stop'
$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add('http://127.0.0.1:59870/')
$listener.Start()
$logPath = Join-Path $PSScriptRoot 'duluka-stub-capture.log'
Write-Output "Duluka stub listening on http://127.0.0.1:59870/ — logging to $logPath"

while ($listener.IsListening) {
    $ctx = $listener.GetContext()
    $req = $ctx.Request
    $body = ''
    if ($req.HasEntityBody) {
        $reader = New-Object IO.StreamReader($req.InputStream, $req.ContentEncoding)
        $body = $reader.ReadToEnd()
    }
    $entry = "[{0}] {1} {2}`n  Cookie: {3}`n  Body: {4}" -f `
        (Get-Date -Format 'HH:mm:ss.fff'), $req.HttpMethod, $req.Url.PathAndQuery,
        $req.Headers['X_LOCAL_SECURITY_COOKIE'], $body
    Add-Content -Path $logPath -Value $entry
    Write-Output $entry

    $res = $ctx.Response
    $res.ContentType = 'application/json; charset=utf-8'
    $bytes = [Text.Encoding]::UTF8.GetBytes('{}')
    $res.ContentLength64 = $bytes.Length
    $res.OutputStream.Write($bytes, 0, $bytes.Length)
    $res.OutputStream.Close()
}
