# fix-mmf-gfe.ps1 — hardlink MMF IPC libs เข้า GFE dir (ที่ Share ถูก spawn) แล้วรีบูต node ให้ enable ใหม่
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\fix-mmf-gfe.log' -Force
$SRC = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\NvContainer'
$DST = 'C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience'

Write-Host '=== [1] hardlink MMF IPC libs → GFE dir ==='
foreach ($n in 'MessageBus.dll','libprotobuf.dll','Poco.dll','PocoInitializer.dll') {
  $s = Join-Path $SRC $n; $t = Join-Path $DST $n
  if (Test-Path $t) { Write-Host ("SKIP: " + $n) }
  elseif (Test-Path $s) { & cmd /c mklink /H "`"$t`"" "`"$s`"" | Out-Null; Write-Host ("LINK: " + $n + " exit=" + $LASTEXITCODE) }
  else { Write-Host ("NO SOURCE: " + $n) }
}

Write-Host '=== [2] kill node (boot-time enable ใหม่) ==='
Get-Process 'NVIDIA Web Helper' -ErrorAction SilentlyContinue | ForEach-Object {
  Write-Host ("[KILL] NVIDIA Web Helper PID " + $_.Id)
  Stop-Process -Id $_.Id -Force
}
Start-Sleep -Seconds 3
Start-Process -FilePath 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\NvNode\NVIDIA Web Helper.exe' -WorkingDirectory 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\NvNode'
$ok = $false
foreach ($i in 1..40) { Start-Sleep 1; try { if ((Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/openshare' -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) { $ok = $true; break } } catch {} }
Write-Host ("[2] node :59001 → " + $(if ($ok) {'OK (200)'} else {'FAILED'}))

Write-Host '=== [3] รอ container spawn Share (boot-time enable) ==='
$shareUp = $false
foreach ($i in 1..30) { Start-Sleep 2; if (Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue) { $shareUp = $true; break } }
Write-Host ("[3] Share: " + $(if ($shareUp) {'SPAWNED!'} else {'ยังไม่เกิด'}))
Start-Sleep -Seconds 8

Write-Host '=== [4] GET /Hotkey/monitor ==='
try { $r = Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/monitor' -UseBasicParsing -TimeoutSec 4; Write-Host ("monitor → " + $r.StatusCode + " " + $r.Content) } catch { Write-Host ("monitor → " + $_.Exception.Message) }

Write-Host '=== [5] helper ถ้ายังไม่มี ==='
if (-not (Get-Process nvsphelper64 -ErrorAction SilentlyContinue)) {
  Start-Process -FilePath 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\ShadowPlay\nvsphelper64.exe' -WorkingDirectory 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\ShadowPlay'
  Start-Sleep -Seconds 6
}
Write-Host ("[5] helper: " + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count + " · Share: " + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count)
Stop-Transcript
