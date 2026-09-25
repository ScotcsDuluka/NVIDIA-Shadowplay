$endpoints = @(
    'http://127.0.0.1:59001/HardwareInformation/v.0.1',
    'http://127.0.0.1:59001/HardwareInformation/v.0.2',
    'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/Toggle',
    'http://127.0.0.1:59001/Account/v.1.0/UserToken'
)
foreach ($u in $endpoints) {
    try {
        $r = Invoke-WebRequest -Uri $u -UseBasicParsing -TimeoutSec 8
        Write-Host ($r.StatusCode + ' len=' + $r.Content.Length + '  ' + $u)
    } catch {
        $code = ''
        if ($_.Exception.Response) { $code = [int]$_.Exception.Response.StatusCode }
        Write-Host ($code.ToString() + '  ' + $u)
    }
}
