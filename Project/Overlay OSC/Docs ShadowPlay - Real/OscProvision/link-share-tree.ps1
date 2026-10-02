# link-share-tree.ps1 — hardlink ไฟล์ทั้ง tree ของ build\Share (รวม osc\) เข้า GFE dir (ข้ามไฟล์ที่มีอยู่)
$ErrorActionPreference = 'Continue'
Start-Transcript -Path 'C:\My Project\NVIDIA-Shadowplay\Project\OscProvision\Logs\link-share-tree.log' -Force
$SRC = 'C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Share'
$DST = 'C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience'

$linked = 0; $skipped = 0; $failed = 0
Get-ChildItem $SRC -Recurse -File | ForEach-Object {
  $rel = $_.FullName.Substring($SRC.Length + 1)
  $t = Join-Path $DST $rel
  $tdir = Split-Path $t -Parent
  if (-not (Test-Path $tdir)) { New-Item -ItemType Directory -Force -Path $tdir | Out-Null }
  if (Test-Path $t) { $skipped++; return }
  & cmd /c mklink /H "`"$t`"" "`"$($_.FullName)`"" | Out-Null
  if ($LASTEXITCODE -eq 0) { $linked++ } else { $failed++; Write-Host ("FAIL: " + $rel) }
}
Write-Host ("linked=$linked skipped=$skipped failed=$failed")

Write-Host '=== POST /Launch {"launch":true} ==='
try { $r = Invoke-WebRequest -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch' -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 10; Write-Host ("Launch → " + $r.StatusCode) } catch { Write-Host ("Launch → " + $_.Exception.Message) }
Start-Sleep -Seconds 12

Write-Host '=== GET /Hotkey/monitor ==='
try { $r = Invoke-WebRequest 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/monitor' -UseBasicParsing -TimeoutSec 4; Write-Host ("monitor → " + $r.StatusCode + " " + $r.Content) } catch { Write-Host ("monitor → " + $_.Exception.Message) }

Write-Host '=== process ==='
Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue | Select-Object Id | Format-Table -AutoSize | Out-String
Stop-Transcript
