# link-share-gfe.ps1 — hardlink build\Share ทุกไฟล์ (ชื่อที่ยังไม่มี) เข้า PF GFE dir แล้ว trigger enable
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\link-share-gfe.log' -Force
$SRC = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Share'
$DST = 'C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience'

Write-Host '=== [1] hardlink ทุกไฟล์จาก build\Share → GFE dir (ไม่แตะไฟล์เดิมที่มีอยู่) ==='
Get-ChildItem $SRC -File | ForEach-Object {
  $t = Join-Path $DST $_.Name
  if (Test-Path $t) { Write-Host ("SKIP (มีอยู่แล้ว): " + $_.Name) }
  else {
    $r = & cmd /c mklink /H "`"$t`"" "`"$($_.FullName)`"" 2>&1
    if ($LASTEXITCODE -eq 0) { Write-Host ("LINK: " + $_.Name) } else { Write-Host ("FAIL: " + $_.Name + " → " + $r) }
  }
}
Write-Host '--- ตรวจ NVIDIA Share.exe ที่ GFE dir ---'
Get-Item (Join-Path $DST 'NVIDIA Share.exe') | Select-Object Length,LinkType,Target | Format-List | Out-String

Write-Host '=== [2] POST /Launch {"launch":true} ==='
try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 10; Write-Host ("Launch → " + $r.StatusCode + " " + $r.Content) } catch { Write-Host ("Launch → " + $_.Exception.Message) }
Start-Sleep -Seconds 10

Write-Host '=== [3] GET /Hotkey/monitor ==='
try { $r = Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/monitor' -UseBasicParsing -TimeoutSec 4; Write-Host ("monitor → " + $r.StatusCode + " " + $r.Content) } catch { Write-Host ("monitor → " + $_.Exception.Message) }

Write-Host '=== [4] process ==='
Get-Process 'NVIDIA Share','NVIDIA Web Helper','nvsphelper64' -ErrorAction SilentlyContinue | Select-Object Id,ProcessName | Format-Table -AutoSize | Out-String
Stop-Transcript
