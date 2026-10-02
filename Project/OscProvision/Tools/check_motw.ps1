$files = @(
  'C:\My Project\GFE\GeForce_Experience_v3.28.0.412.exe',
  'C:\My Project\GFE\GeForce_Experience_v3.28.0.412\setup.exe'
)
foreach ($f in $files) {
  $item = Get-Item $f
  Write-Host ("== " + $f)
  $streams = $item.GetStreams() | Where-Object { $_.Stream -ne ':$DATA' -and $_.Stream -ne 'DATA' }
  if ($streams) { $streams | Format-Table Stream, Length -AutoSize } else { Write-Host "  no MotW" }
  Write-Host ("  Zone: " + (Get-ItemProperty -Path ("Microsoft.PowerShell.Core\FileSystem::" + $f) -Stream Zone.Identifier -ErrorAction SilentlyContinue))
}
# Smart App Control state: 0=Off 1=On 2=Eval
$sac = Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Control\CI\Policy' -ErrorAction SilentlyContinue
Write-Host ("SmartAppControl VerifiedAndReputablePolicyState: " + $sac.VerifiedAndReputablePolicyState)
