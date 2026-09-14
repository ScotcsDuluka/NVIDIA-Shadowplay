# cdp-test.ps1 — one-shot Page.captureScreenshot via CDP WebSocket.
# Probes whether the browser responds to captureScreenshot.
param(
    [string]$Port = "9224"
)
Add-Type -AssemblyName System.Net.Http

$list = (Invoke-WebRequest -Uri "http://127.0.0.1:$Port/json/list" -UseBasicParsing -TimeoutSec 5).Content
$start = $list.IndexOf('[')
$arr = $list.Substring($start) | ConvertFrom-Json
$target = $arr | Where-Object { $_.type -eq 'page' -and $_.url -match 'index.html' } | Select-Object -First 1
if (-not $target) { Write-Host "NO PAGE TARGET"; exit 1 }
Write-Host ("target: " + $target.url)

$ws = New-Object System.Net.WebSockets.ClientWebSocket
$ct = [System.Threading.CancellationToken]::None
$ws.ConnectAsync([Uri]$target.webSocketDebuggerUrl, $ct).Wait(5000) | Out-Null
if ($ws.State -ne [System.Net.WebSockets.WebSocketState]::Open) { Write-Host "WS CONNECT FAILED: $($ws.State)"; exit 1 }
Write-Host "ws connected"

$req = '{"id":1,"method":"Page.captureScreenshot","params":{"format":"png"}}'
$bytes = [System.Text.Encoding]::UTF8.GetBytes($req)
$ws.SendAsync([ArraySegment[byte]]::new($bytes), 'Text', $true, $ct).Wait(5000) | Out-Null
Write-Host "captureScreenshot sent"

$buf = New-Object byte[] (4 * 1024 * 1024)
$ms = New-Object System.IO.MemoryStream
while ($true) {
    $seg = [ArraySegment[byte]]::new($buf)
    $res = $ws.ReceiveAsync($seg, $ct).Result
    $ms.Write($buf, 0, $res.Count)
    if ($res.EndOfMessage) {
        $txt = [System.Text.Encoding]::UTF8.GetString($ms.ToArray())
        if ($txt -match '"id":1') {
            if ($txt.Length -gt 100000) {
                Write-Host ("GOT SCREENSHOT: " + $txt.Length + " bytes")
            } else {
                Write-Host ("RESPONSE: " + $txt.Substring(0, [Math]::Min(300, $txt.Length)))
            }
            break
        }
        $ms = New-Object System.IO.MemoryStream
    }
}
$ws.Dispose()
