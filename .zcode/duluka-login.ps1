# Login to the Duluka Server and inject the session into NvNode.
$ErrorActionPreference = 'Continue'
$server = 'http://192.168.1.254:5115'
$nvnode = 'http://127.0.0.1:59001'

# stable per-machine device key
$deviceKey = ($env:COMPUTERNAME + '-intel-standalone') | ForEach-Object {
    $md5 = [System.Security.Cryptography.MD5]::Create()
    ([System.Security.Cryptography.MD5]::Create().ComputeHash([Text.Encoding]::UTF8.GetBytes($_)) | ForEach-Object { $_.ToString('x2') }) -join ''
}
$deviceKey = [System.Security.Cryptography.MD5]::Create().ComputeHash([Text.Encoding]::UTF8.GetBytes($env:COMPUTERNAME + '-intel-standalone'))
$deviceKeyHex = ($deviceKey | ForEach-Object { $_.ToString('x2') }) -join ''

Write-Host '--- [1] login ---'
$loginBody = @{
    username   = 'ScotcsDuluka'
    password   = 'ScotcsDuluka'
    deviceKey  = $deviceKeyHex
    deviceName = $env:COMPUTERNAME
} | ConvertTo-Json
try {
    $r = Invoke-WebRequest -Uri ($server + '/v1/auth/login') -Method POST -ContentType 'application/json' -Body $loginBody -UseBasicParsing -TimeoutSec 15
    Write-Host ('LOGIN: ' + $r.StatusCode)
    Write-Host ('RAW: ' + $r.Content.Substring(0, [Math]::Min(500, $r.Content.Length)))
    $json = $r.Content | ConvertFrom-Json
    $sess = $json.data
    if (-not $sess) { $sess = $json }
    $token = $sess.sessionToken
    $accountId = $sess.accountId
    $username = $sess.username
    if (-not $token) { Write-Host 'NO-TOKEN-IN-RESPONSE'; exit 1 }
    Write-Host ('TOKEN-OK len=' + $token.Length + ' account=' + $accountId)
} catch {
    Write-Host ('LOGIN-ERR: ' + $_.Exception.Message)
    if ($_.ErrorDetails -and $_.ErrorDetails.Message) { Write-Host ('DETAIL: ' + $_.ErrorDetails.Message.Substring(0, [Math]::Min(300, $_.ErrorDetails.Message.Length))) }
    exit 1
}

Write-Host '--- [2] inject into NvNode (login bridge) ---'
$guestInfo = @{
    userId      = [string]$accountId
    displayName = $username
    deviceId    = $sess.deviceId
    userToken   = $token
    duluka      = $true
}
$injectBody = @{
    userToken = $token
    userInfo  = ($guestInfo | ConvertTo-Json -Depth 4 -Compress)
} | ConvertTo-Json -Depth 4 -Compress
try {
    $r2 = Invoke-WebRequest -Uri ($nvnode + '/Account/v.1.0/UserToken') -Method POST -ContentType 'application/json' -Body $injectBody -UseBasicParsing -TimeoutSec 10
    Write-Host ('INJECT: ' + $r2.StatusCode)
} catch {
    Write-Host ('INJECT-ERR: ' + $_.Exception.Message)
    exit 1
}

Write-Host '--- [3] verify UserToken ---'
try {
    $r3 = Invoke-WebRequest -Uri ($nvnode + '/Account/v.1.0/UserToken') -UseBasicParsing -TimeoutSec 10
    Write-Host ('VERIFY: ' + $r3.StatusCode + '  ' + $r3.Content.Substring(0, [Math]::Min(300, $r3.Content.Length)))
} catch {
    Write-Host ('VERIFY-ERR: ' + $_.Exception.Message)
}
