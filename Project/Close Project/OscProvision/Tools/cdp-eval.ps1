# cdp-eval.ps1 — evaluate JS in the OSC page via CEF remote debugging (:59099)
# usage: powershell -File cdp-eval.ps1 "<js expression>"
param([Parameter(Mandatory=$true)][string]$Expr)

$ErrorActionPreference = 'Stop'
$list = Invoke-RestMethod 'http://127.0.0.1:59099/json'
$page = $list | Where-Object { $_.type -eq 'page' -and $_.url -like '*3000*' } | Select-Object -First 1
if (-not $page) { $page = $list | Where-Object { $_.type -eq 'page' } | Select-Object -First 1 }
if (-not $page) { Write-Output 'NO PAGE TARGET'; exit 1 }
Write-Output ("target: " + $page.url)

$ws = New-Object System.Net.WebSockets.ClientWebSocket
$ct = New-Object System.Threading.CancellationTokenSource(15000)
$ws.ConnectAsync([Uri]$page.webSocketDebuggerUrl, $ct.Token).Wait()

function Send-Json([object]$o) {
  $bytes = [System.Text.Encoding]::UTF8.GetBytes(($o | ConvertTo-Json -Compress -Depth 6))
  $ws.SendAsync([ArraySegment[byte]]::new($bytes), [System.Net.WebSockets.WebSocketMessageType]::Text, $true, $ct.Token).Wait()
}
function Recv-Json {
  $buf = New-Object byte[] 262144
  $sb = New-Object System.Text.StringBuilder
  do {
    $res = $ws.ReceiveAsync([ArraySegment[byte]]::new($buf), $ct.Token)
    if (-not $res.Wait(15000)) { return $null }
    [void]$sb.Append([System.Text.Encoding]::UTF8.GetString($buf, 0, $res.Result.Count))
  } while ($res.Result.EndOfMessage -eq $false)
  return $sb.ToString() | ConvertFrom-Json
}
# skip events until we get the response with our id
function Recv-Until-Id([int]$id) {
  while ($true) {
    $m = Recv-Json
    if ($null -eq $m) { return $null }
    if ($m.id -eq $id) { return $m }
  }
}

Send-Json @{ id = 1; method = 'Runtime.enable' }
[void](Recv-Until-Id 1)
Send-Json @{ id = 2; method = 'Runtime.evaluate'; params = @{
  expression = $Expr; awaitPromise = $true; returnByValue = $true } }
$r = Recv-Until-Id 2
if ($r.result.result.value -ne $null) { Write-Output ("value: " + ($r.result.result.value | ConvertTo-Json -Compress -Depth 8)) }
elseif ($r.result.result.description) { Write-Output ("desc: " + $r.result.result.description) }
if ($r.result.exceptionDetails) { Write-Output ("EXC: " + ($r.result.exceptionDetails | ConvertTo-Json -Compress -Depth 6)) }
$ws.CloseAsync([System.Net.WebSockets.WebSocketCloseStatus]::NormalClosure, 'done', $ct.Token).Wait()
