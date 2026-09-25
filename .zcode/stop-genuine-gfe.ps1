# Cutover step: stop the genuine GFE stack so port 59001 is freed.
# Ordered: stop the container service first (it supervises nvcontainer),
# then force-close the remaining GFE user processes.
$ErrorActionPreference = 'Continue'

Write-Host '--- NVIDIA services before ---'
Get-Service | Where-Object { $_.Name -like 'Nv*' -or $_.DisplayName -like '*NVIDIA*' } |
    ForEach-Object { Write-Host ($_.Name + ' = ' + $_.Status) }

foreach ($svc in @('NvContainerLocalSystem', 'NVIDIA Overlay')) {
    $s = Get-Service -Name $svc -ErrorAction SilentlyContinue
    if ($s -and $s.Status -eq 'Running') {
        Write-Host ("Stopping service " + $svc + " ...")
        Stop-Service -Name $svc -Force -ErrorAction Continue
    }
}

Start-Sleep -Seconds 3

foreach ($proc in @('NVIDIA Share', 'NVIDIA Web Helper', 'nvcontainer')) {
    $running = Get-Process -Name $proc -ErrorAction SilentlyContinue
    if ($running) {
        Write-Host ("taskkill /F /IM " + $proc + ".exe (" + @($running).Count + " process(es))")
        & taskkill /F /IM ($proc + '.exe') 2>&1 | Out-Null
    }
}

Start-Sleep -Seconds 3

Write-Host '--- after ---'
Get-Service | Where-Object { $_.Name -like 'Nv*' } |
    ForEach-Object { Write-Host ($_.Name + ' = ' + $_.Status) }
$left = Get-Process -Name 'NVIDIA Share', 'NVIDIA Web Helper', 'nvcontainer' -ErrorAction SilentlyContinue
if ($left) { $left | ForEach-Object { Write-Host ('STILL-RUNNING: ' + $_.ProcessName + ' pid=' + $_.Id + ' path=' + $_.Path) } }
else { Write-Host 'ALL-GFE-PROCESSES-CLOSED' }

$port = Get-NetTCPConnection -LocalPort 59001 -State Listen -ErrorAction SilentlyContinue
if ($port) { Write-Host ('PORT-59001-HELD-BY pid=' + ($port | Select-Object -First 1).OwningProcess) }
else { Write-Host 'PORT-59001-FREE' }
