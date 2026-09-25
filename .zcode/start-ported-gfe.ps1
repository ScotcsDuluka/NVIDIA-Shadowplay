# Restore the ported GFE 3.28 stack (the working osc on this machine).
$ErrorActionPreference = 'Continue'
Write-Host 'Starting NvContainerLocalSystem ...'
Start-Service -Name 'NvContainerLocalSystem' -ErrorAction Continue
Start-Sleep -Seconds 4
Get-Service | Where-Object { $_.Name -like 'Nv*' } | ForEach-Object { Write-Host ($_.Name + ' = ' + $_.Status) }
$nc = Get-Process -Name nvcontainer -ErrorAction SilentlyContinue
if ($nc) { $nc | ForEach-Object { Write-Host ('nvcontainer alive pid=' + $_.Id) } } else { Write-Host 'nvcontainer NOT-RUNNING' }
