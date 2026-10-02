# phase1-clean-flip.ps1 — การทดลองสะอาด: nvcontainer แท้จาก build path (HANDOFF-GENUINE-BUILD-PATH Phase 1)
# ต้องรัน elevated (OWNER รันเอง) — ทุกขั้นบันทึก evidence ลง repo เพื่อให้ agent วิเคราะห์จาก log จริง
# สูตร: phaseB-restore.ps1 + บทเรียน §9/§10 — Set-ItemProperty เท่านั้น (ห้าม sc config) · ล้าง FailureActions ก่อน stop
#   · start ครั้งแรกตาย = start ซ้ำ · flip ครบทุก Watchdog profile ที่มีจริง (ยุค NVIDIA App: AIUserX64 แทน NetworkServiceX64)
#   · แทน string ในค่าเดิม = รักษา flag เฉพาะ profile (RunElevated/-ert/-st) ครบ
# ก่อน flip: backup .reg ทุก key ที่แตะ → Logs\phase1-before\ (roll back ได้เสมอ)
$ErrorActionPreference = 'Continue'
$Root = 'C:\My Project\NVIDIA-Shadowplay'
$B    = Join-Path $Root 'build\NVIDIA ShadowPlay'
$Gen  = Join-Path $B 'NvContainer\genuine'
$Prov = Join-Path $Root 'Project\Overlay OSC\Docs ShadowPlay - Real\OscProvision'
$Ev   = Join-Path $Prov 'Logs\phase1-evidence'

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host '[FAIL] ต้องรันแบบ elevated (Run as Administrator) — จบ'
    exit 2
}
if (-not (Test-Path $Ev)) { New-Item -ItemType Directory -Path $Ev -Force | Out-Null }
Start-Transcript -Path (Join-Path $Ev 'phase1-clean.log') -Force | Out-Null

function Fail([string]$m) { Write-Host ('[FAIL] ' + $m); Stop-Transcript | Out-Null; exit 1 }
function Test-Http200([string]$url) { try { return ((Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) } catch { return $false } }

$svcKey    = 'HKLM:\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem'
$wdKey     = 'HKLM:\SOFTWARE\NVIDIA Corporation\NvContainer\Watchdog'
$PFCont    = 'C:\Program Files\NVIDIA Corporation\NvContainer'

# ---------- [1] หา Payload แท้ (เหมือน start-osc.ps1 [0]) ----------
$payload = Get-ChildItem -Path (Join-Path $Root 'Project\Overlay OSC') -Directory -Recurse -Depth 3 -Filter 'Payload' -ErrorAction SilentlyContinue |
           Where-Object { Test-Path (Join-Path $_.FullName 'NvContainer\nvcontainer.exe') } |
           Select-Object -First 1
if (-not $payload) { Fail 'ไม่พบ Payload แท้ (NvContainer\nvcontainer.exe) — หยุด ไม่มี fallback' }

# ---------- [2] STAGE แท้ → build\NvContainer\genuine\ ----------
Write-Host '=== [2] stage container แท้ (Payload + AIUser จาก PF เฉพาะที่ Payload ไม่มี) ==='
New-Item -ItemType Directory -Path $Gen -Force | Out-Null
foreach ($f in 'nvcontainer.exe','NvContainerTelemetryApi.dll') {
    $src = Join-Path $payload.FullName ('NvContainer\' + $f)
    if (-not (Test-Path $src)) { Fail ('ขาดไฟล์แท้: ' + $src) }
    Copy-Item $src (Join-Path $Gen $f) -Force
}
Copy-Item (Join-Path $payload.FullName 'NvContainer\plugins') (Join-Path $Gen 'plugins') -Recurse -Force
# AIUser (NVIDIA App era) — Payload ไม่มี → stage จาก PF (แหล่งแท้ที่ยังมีอยู่)
if (-not (Test-Path (Join-Path $Gen 'plugins\AIUser'))) {
    if (Test-Path (Join-Path $PFCont 'plugins\AIUser')) {
        Copy-Item (Join-Path $PFCont 'plugins\AIUser') (Join-Path $Gen 'plugins\AIUser') -Recurse -Force
        Write-Host '  staged plugins\AIUser จาก PF (Payload ไม่มี — NVIDIA App era)'
    } else { Write-Host '  ไม่มี AIUser ทั้ง Payload และ PF — AIUserX64 profile จะไม่ถูก flip' }
}
$nc = Get-Item (Join-Path $Gen 'nvcontainer.exe')
Write-Host ('[2] staged nvcontainer.exe ' + [math]::Round($nc.Length / 1KB, 0) + 'KB ที่ ' + $Gen)

# ---------- [3] BACKUP ก่อน flip (roll back เสมอ) ----------
Write-Host '=== [3] backup registry ก่อน flip ==='
$before = Join-Path $Prov 'Logs\phase1-before'
if (-not (Test-Path $before)) { New-Item -ItemType Directory -Path $before -Force | Out-Null }
reg.exe export 'HKLM\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem' (Join-Path $before 'service-NvContainerLocalSystem.reg') /y | Out-Null
reg.exe export 'HKLM\SOFTWARE\NVIDIA Corporation\NvContainer\Watchdog' (Join-Path $before 'Watchdog-all.reg') /y | Out-Null
Write-Host ('[3] backup → ' + $before)

# ---------- [4] ล้าง FailureActions (ก่อน stop — กัน self-heal) ----------
Write-Host '=== [4] ล้าง FailureActions ==='
reg.exe add 'HKLM\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem' /v FailureActions /t REG_BINARY /d 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 /f | Out-Null

# ---------- [5] flip ImagePath (แทน string ในค่าเดิม — รักษา arg ทั้งหมด) ----------
Write-Host '=== [5] flip ImagePath → build (genuine) ==='
$ip  = (Get-ItemProperty $svcKey).ImagePath
$nip = $ip.Replace($PFCont, $Gen)
Set-ItemProperty -Path $svcKey -Name ImagePath -Value $nip -Type ExpandString
Write-Host ('[5] เดิม: ' + $ip)
Write-Host ('[5] ใหม่: ' + $nip)

# ---------- [6] flip Watchdog ทุก profile ที่มีจริง (แทน string — รักษา flag) ----------
Write-Host '=== [6] flip Watchdog profiles → build (genuine) ==='
foreach ($p in (Get-ChildItem $wdKey)) {
    $prop = Get-ItemProperty $p.PSPath
    foreach ($n in 'Folder','Container','Parameters') {
        $v = $prop.$n
        if ($v -and ($v -like ('*' + $PFCont + '*'))) {
            Set-ItemProperty -Path $p.PSPath -Name $n -Value $v.Replace($PFCont, $Gen)
        }
    }
    $chk = Get-ItemProperty $p.PSPath
    Write-Host ('[6] ' + $p.PSChildName + ' → Container=' + $chk.Container)
}

# ---------- [7] stop service + kill container (ประกาศรายตัว) ----------
Write-Host '=== [7] stop service + kill containers ==='
sc.exe stop NvContainerLocalSystem | Out-Null
Start-Sleep -Seconds 5
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host ('[KILL] nvcontainer PID ' + $_.ProcessId + ' (จาก ' + $_.ExecutablePath + ')')
    Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
}
Start-Sleep -Seconds 2

# ---------- [8] start service (ตายรอบแรก = start รอบสอง) ----------
Write-Host '=== [8] start service จาก build ==='
sc.exe start NvContainerLocalSystem | Out-Null
Start-Sleep -Seconds 14
if ((Get-Service NvContainerLocalSystem).Status -ne 'Running') {
    Write-Host '  รอบแรกไม่ขึ้น — start รอบสอง (pattern ปกติ)'
    sc.exe start NvContainerLocalSystem | Out-Null
    Start-Sleep -Seconds 14
}
$st = (Get-Service NvContainerLocalSystem).Status
Write-Host ('[8] service: ' + $st)
if ($st -ne 'Running') { Fail 'service ไม่ขึ้นหลัง flip — ดู NvContainerLocalSystem.log (log dir ตาม ImagePath -f)' }
Start-Sleep -Seconds 10
Write-Host '=== containers ที่เพิ่งบูต (ต้องมาจาก build ทั้งหมด) ==='
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host ('[8] container PID ' + $_.ProcessId + ' จาก ' + $_.ExecutablePath + ' | ' + $_.CommandLine.Substring(0, [Math]::Min(120, $_.CommandLine.Length)))
}

# ---------- [9] บูต user layer ด้วย start-osc.ps1 (stage node/helper แท้ + boot + re-arm) ----------
Write-Host '=== [9] เรียก start-osc.ps1 -Mode genuine ==='
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $B 'start-osc.ps1') -Mode genuine

# ---------- [10] เก็บหลักฐาน: grep "Unknown executable path" + copy logs ----------
Write-Host '=== [10] เก็บหลักฐาน (grep Unknown executable path + copy logs) ==='
Start-Sleep -Seconds 20
$logDirs = @(
    'C:\ProgramData\NVIDIA',
    'C:\ProgramData\NVIDIA Corporation\NVIDIA App\NvContainer',
    (Join-Path $env:LOCALAPPDATA 'NVIDIA Corporation\NvNode'),
    (Join-Path $B 'Logs')
)
$hits = New-Object System.Collections.Generic.List[string]
foreach ($d in $logDirs) {
    if (-not (Test-Path $d)) { continue }
    Get-ChildItem $d -Filter '*.log' -ErrorAction SilentlyContinue | ForEach-Object {
        $m = Select-String -Path $_.FullName -Pattern 'Unknown executable path' -SimpleMatch -ErrorAction SilentlyContinue
        foreach ($x in $m) { $hits.Add(($_.FullName + ':' + $x.LineNumber + ': ' + $x.Line)) }
        # สำเนา log เต็มเข้า repo (เฉพาะไฟล์ < 20MB)
        if ($_.Length -lt 20MB) {
            Copy-Item $_.FullName (Join-Path $Ev ('log-' + $_.Name)) -Force -ErrorAction SilentlyContinue
        }
    }
}
$summary = @()
$summary += ('เวลา: ' + (Get-Date))
$summary += ('ImagePath: ' + (Get-ItemProperty $svcKey).ImagePath)
$summary += ('service: ' + (Get-Service NvContainerLocalSystem).Status)
$summary += ('node :59001 200: ' + (Test-Http200 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/openshare'))
$summary += ('Share ตัว: ' + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + ' · helper ตัว: ' + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count)
$summary += ('container ตัว: ' + @(Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" -ErrorAction SilentlyContinue).Count)
$summary += '--- containers จาก path ใด ---'
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" -ErrorAction SilentlyContinue | ForEach-Object { $summary += ('  PID ' + $_.ProcessId + ' → ' + $_.ExecutablePath) }
$summary += ('--- บรรทัด "Unknown executable path": ' + $hits.Count + ' บรรทัด ---')
$hits | ForEach-Object { $summary += ('  ' + $_) }
$summary | Set-Content -Path (Join-Path $Ev 'phase1-RESULT.txt') -Encoding UTF8
$summary | ForEach-Object { Write-Host $_ }
Write-Host ('[10] evidence → ' + $Ev)
Stop-Transcript | Out-Null
