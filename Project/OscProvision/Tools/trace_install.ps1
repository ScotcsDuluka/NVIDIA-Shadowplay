# Trace GFE setup.exe with ProcMon: capture the ACCESS DENIED moment.
$pm  = 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Tools\procmon\ProcMon64.exe'
$pmc = 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Tools\trace_install.pmc'
$pml = 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Tools\trace_install.pml'
$csv = 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Tools\trace_install.csv'
if (Test-Path $pml) { Remove-Item $pml -Force }
if (Test-Path $csv) { Remove-Item $csv -Force }

# Start capture (backing file, quiet accept of EULA via /AcceptEula)
Start-Process $pm -ArgumentList '/AcceptEula','/Quiet','/Minimized','/BackingFile',"$pml" -Verb RunAs
Start-Sleep -Seconds 6

# Run the installer silently
$setup = 'C:\My Project\GFE\GeForce_Experience_v3.28.0.412\setup.exe'
$p = Start-Process $setup -ArgumentList '-s' -Verb RunAs -PassThru -Wait
Write-Host ("setup ExitCode: 0x" + $p.ExitCode.ToString('X'))

Start-Sleep -Seconds 3
# Stop capture and export ACCESS DENIED rows to CSV
Start-Process $pm -ArgumentList '/Terminate' -Verb RunAs -Wait
Start-Sleep -Seconds 2
& $pm /OpenLog $pml /SaveAs $csv /AcceptEula /Quiet | Out-Null
Start-Sleep -Seconds 5
Write-Host "csv exists: $(Test-Path $csv)"
