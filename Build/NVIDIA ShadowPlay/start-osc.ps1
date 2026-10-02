# start-osc.ps1 — บูตสายแท้ (genuine) 6-step idempotent ตาม PATH-MANIFEST §10
# OWNER ORDER 2026-10-03: default = สายแท้เท่านั้น
#   [0] stage binary แท้จาก Project\Overlay OSC\Docs ShadowPlay - Real\...\Payload\ เข้า build ก่อนบูตเสมอ
#   ของแท้หาไม่เจอ = FAIL ชัดเจน (exit 1) — ห้าม fallback ไป shim เงียบ ๆ (shim เฉพาะ -Mode ours)
# ลำดับ §10: [0] stage → [1] service → [2] containers ×3 + agent → [3] node :59001 → [4] helper → [5] Share (attach) → [6] re-arm + รายงาน
# สูตรที่ยึด: Set-ItemProperty เท่านั้น (ไม่ sc config) · helper ต้องหลัง node 200 · re-arm /Launch ทุกรอบ · ห้าม nv-osc=false
param([string]$Mode = 'genuine')

$ErrorActionPreference = 'Continue'
$Root = 'C:\My Project\NVIDIA-Shadowplay'
$B    = Join-Path $Root 'build\NVIDIA ShadowPlay'
$Log  = Join-Path $B 'Logs'
if (-not (Test-Path $Log)) { New-Item -ItemType Directory -Path $Log -Force | Out-Null }
Start-Transcript -Path (Join-Path $Log 'start-osc.log') -Force | Out-Null

function Fail([string]$msg) { Write-Host ('[FAIL] ' + $msg); Stop-Transcript | Out-Null; exit 1 }
function Test-Http200([string]$url) { try { return ((Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) } catch { return $false } }
function Same-File([string]$a, [string]$b) {
    if (-not ((Test-Path $a -PathType Leaf) -and (Test-Path $b -PathType Leaf))) { return $false }
    return ((Get-FileHash $a -Algorithm MD5 -ErrorAction SilentlyContinue).Hash -eq (Get-FileHash $b -Algorithm MD5 -ErrorAction SilentlyContinue).Hash)
}
# Copy-Genuine: idempotent — ข้ามถ้าเนื้อไฟล์ตรง (มีอยู่แล้ว หรือถูก lock โดย container ที่รันจากมัน) · copy ผิดพลาดอื่น = fail ชัด
function Copy-Genuine([string]$src, [string]$dst) {
    if (Test-Path $dst -PathType Leaf) {
        if (Same-File $src $dst) { Write-Host ('  ข้าม (เนื้อไฟล์ตรง): ' + (Split-Path $dst -Leaf)); return }
        try { Copy-Item $src $dst -Force } catch {
            if (Same-File $src $dst) { Write-Host ('  ข้าม (ล็อกแต่เนื้อไฟล์ตรง): ' + (Split-Path $dst -Leaf)); return }
            Fail ('copy ไม่สำเร็จ: ' + $src + ' → ' + $dst + ' — ' + $_.Exception.Message)
        }
    } else {
        try { Copy-Item $src $dst -Force } catch { Fail ('copy ไม่สำเร็จ: ' + $src + ' → ' + $dst + ' — ' + $_.Exception.Message) }
    }
}
# Copy-Genuine-Tree: ทีละไฟล์ — จัดการ leaf/dir ชนกัน + ไฟล์ล็อกที่เนื้อตรง (ข้าม)
function Copy-Genuine-Tree([string]$src, [string]$dst) {
    if (-not (Test-Path $dst)) { New-Item -ItemType Directory -Path $dst -Force | Out-Null }
    foreach ($child in (Get-ChildItem $src -Force)) {
        $dstChild = Join-Path $dst $child.Name
        if ($child.PSIsContainer) {
            if (Test-Path $dstChild -PathType Leaf) { Remove-Item $dstChild -Force }
            Copy-Genuine-Tree $child.FullName $dstChild
        } else {
            if (Test-Path $dstChild -PathType Container) { Remove-Item $dstChild -Recurse -Force }
            Copy-Genuine $child.FullName $dstChild
        }
    }
}

# Share แท้ใน build (ทั้งสองโหมดใช้ตัวเดียวกัน)
$ShareExe   = Join-Path $B 'Overlay OSC\NVIDIA Share\NVIDIA Share.exe'
$ShareWd    = Split-Path $ShareExe
$NodeProbe  = 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/openshare'
$LaunchUrl  = 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch'

# ---------- [0] STAGE ของแท้ (genuine mode เท่านั้น) ----------
if ($Mode -eq 'genuine') {
    Write-Host '=== [0] stage binary แท้จาก OscProvision\Payload → build ==='
    $payload = Get-ChildItem -Path (Join-Path $Root 'Project\Overlay OSC') -Directory -Recurse -Depth 3 -Filter 'Payload' -ErrorAction SilentlyContinue |
               Where-Object { Test-Path (Join-Path $_.FullName 'NvContainer\nvcontainer.exe') } |
               Select-Object -First 1
    if (-not $payload) { Fail ('ไม่พบ Payload ของแท้ (ต้องมี NvContainer\nvcontainer.exe) ภายใต้ ' + (Join-Path $Root 'Project\Overlay OSC') + ' — หยุดบูต ไม่มี fallback') }

    # 0a) container แท้ → build\NvContainer\genuine\ (แยกจาก NvContainer.exe C# ของเรา — ชื่อไฟล์ชนกันบน NTFS)
    $genuine = Join-Path $B 'NvContainer\genuine'
    New-Item -ItemType Directory -Path $genuine -Force | Out-Null
    foreach ($f in 'nvcontainer.exe','NvContainerTelemetryApi.dll') {
        $src = Join-Path $payload.FullName ('NvContainer\' + $f)
        if (-not (Test-Path $src)) { Fail ('ขาดไฟล์ container แท้: ' + $src) }
        Copy-Genuine $src (Join-Path $genuine $f)
    }
    $plugSrc = Join-Path $payload.FullName 'NvContainer\plugins'
    if (-not (Test-Path $plugSrc)) { Fail ('ขาด plugins แท้: ' + $plugSrc) }
    Copy-Genuine-Tree $plugSrc (Join-Path $genuine 'plugins')

    # 0b) node แท้ → build\NvNode\ (Web Helper แท้ ~28MB — shim .NET = 0.6MB, ขนาดต้องเกิน 20MB)
    $nodeDst = Join-Path $B 'NvNode'
    $nhPath  = Join-Path $nodeDst 'NVIDIA Web Helper.exe'
    $needNode = $true
    if (Test-Path $nhPath) { if ((Get-Item $nhPath).Length -ge 20MB) { $needNode = $false } }
    if ($needNode) {
        if (-not (Test-Path (Join-Path $payload.FullName 'NvNode\NVIDIA Web Helper.exe'))) { Fail ('ขาด node แท้: ' + (Join-Path $payload.FullName 'NvNode\NVIDIA Web Helper.exe')) }
        Copy-Genuine-Tree (Join-Path $payload.FullName 'NvNode') $nodeDst
    }
    if (-not (Test-Path $nhPath)) { Fail ('stage node ไม่สำเร็จ: ' + $nhPath) }
    if ((Get-Item $nhPath).Length -lt 20MB) { Fail ('node ที่ stage ไม่ใช่ตัวแท้ (' + [math]::Round((Get-Item $nhPath).Length / 1MB, 1) + 'MB — น่าจะเป็น shim) — หยุดบูต') }

    # 0c) helper แท้ → build\ShadowPlay\ (nvsphelper64.exe + ไลบรารีข้างเคียง)
    $spDst = Join-Path $B 'ShadowPlay'
    if (-not (Test-Path (Join-Path $spDst 'nvsphelper64.exe'))) {
        if (-not (Test-Path (Join-Path $payload.FullName 'ShadowPlay\nvsphelper64.exe'))) { Fail ('ขาด helper แท้: ' + (Join-Path $payload.FullName 'ShadowPlay\nvsphelper64.exe')) }
        Copy-Genuine-Tree (Join-Path $payload.FullName 'ShadowPlay') $spDst
    }
    if (-not (Test-Path (Join-Path $spDst 'nvsphelper64.exe'))) { Fail ('stage helper ไม่สำเร็จ: ' + (Join-Path $spDst 'nvsphelper64.exe')) }

    # 0d) Share แท้ + Share.json ห้าม nv-osc=false (สคริปต์ไม่แก้ค่าแทน — fail ให้แก้มือ)
    if (-not (Test-Path $ShareExe)) { Fail ('ไม่พบ Share แท้ใน build: ' + $ShareExe) }
    $shareJson = Get-Content (Join-Path $ShareWd 'NVIDIA Share.json') -Raw
    if ($shareJson -match 'nv-osc=false') { Fail ('Share.json มี nv-osc=false — ห้ามบูต (แก้เป็น true ด้วยมือก่อน ปิด native OSC ทั้งสาย)') }

    Write-Host ('[0] stage OK · container=' + $genuine)
    Write-Host ('[0]          · node=' + $nodeDst)
    Write-Host ('[0]          · helper=' + $spDst)
    $NodeExe   = $nhPath
    $NodeWd    = $nodeDst
    $HelperExe = Join-Path $spDst 'nvsphelper64.exe'
}
else {
    Write-Host '=== [0] OURS MODE (-Mode ours — shim lane §17, ไม่ stage ของแท้) ==='
    $NodeExe   = Join-Path $B 'Overlay OSC\NvNode\NVIDIA Web Helper.exe'
    $NodeWd    = Join-Path $B 'Overlay OSC\NvNode'
    $HelperExe = Join-Path $B 'NvContainer\nvsphelper64.exe'
    if (-not (Test-Path $NodeExe))   { Fail ('ไม่พบ node (ours): ' + $NodeExe) }
    if (-not (Test-Path $HelperExe)) { Fail ('ไม่พบ helper (ours): ' + $HelperExe) }
}

# ---------- [1] service NvContainerLocalSystem (RUNNING = ไม่แตะ) ----------
Write-Host '=== [1] service NvContainerLocalSystem (RUNNING = ไม่แตะ) ==='
$svc = Get-Service NvContainerLocalSystem -ErrorAction SilentlyContinue
if (-not $svc) { Fail 'ไม่มี service NvContainerLocalSystem — restore registry ก่อน (PATH-MANIFEST §3.1)' }
if ($svc.Status -ne 'Running') {
    Start-Service NvContainerLocalSystem
    Start-Sleep -Seconds 10
    if ((Get-Service NvContainerLocalSystem).Status -ne 'Running') {
        Write-Host '  รอบแรกไม่ขึ้น — start รอบสอง (pattern ปกติของ container แท้)'
        Start-Service NvContainerLocalSystem
        Start-Sleep -Seconds 10
    }
}
$svc = Get-Service NvContainerLocalSystem
Write-Host ('[1] service: ' + $svc.Status)
$ip = (Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem' -ErrorAction SilentlyContinue).ImagePath
Write-Host ('[1] ImagePath: ' + $ip)

# ---------- [2] containers + agent (ตรวจ CommandLine ไม่ใช่แค่ชื่อ process) ----------
Write-Host '=== [2] containers + agent (เช็ค CommandLine: SPUser / plugins\User) ==='
$deadline = (Get-Date).AddSeconds(60)
$containers = @()
while ((Get-Date) -lt $deadline) {
    $containers = @(Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" -ErrorAction SilentlyContinue)
    $spUser = @($containers | Where-Object { $_.CommandLine -match 'SPUser' })
    $agent  = @($containers | Where-Object { $_.CommandLine -match 'plugins\\User' })
    if (($spUser.Count -ge 1) -and ($agent.Count -ge 1)) { break }
    Start-Sleep -Seconds 3
}
if ($containers.Count -eq 0) { Write-Host '[2] ยังไม่เห็น container ใด (Watchdog จะปลุกเอง — ไปต่อได้ รายงานท้ายจะเช็คซ้ำ)' }
foreach ($c in $containers) { Write-Host ('[2] container PID ' + $c.ProcessId + ' จาก ' + $c.ExecutablePath) }

# ---------- [3] node :59001 → 200 (ตรวจ path — กัน node จากที่อื่นตอบแทน) ----------
Write-Host '=== [3] node :59001 → 200 (path ต้องเป็นตัวของโหมดนี้เท่านั้น) ==='
$good = $false
if (Test-Http200 $NodeProbe) {
    $wp   = @(Get-CimInstance Win32_Process -Filter "Name='NVIDIA Web Helper.exe'" -ErrorAction SilentlyContinue)
    $ours = @($wp | Where-Object { $_.ExecutablePath -ieq $NodeExe })
    if ($ours.Count -ge 1) {
        $good = $true
        Write-Host ('[3] node อยู่แล้ว (PID ' + $ours[0].ProcessId + ' — path ตรง)')
    } else {
        Write-Host '[3] :59001 ตอบแต่ไม่ใช่ node จาก path ที่กำหนด — kill แล้วสตาร์ตตัวที่ถูกต้อง'
        Get-Process 'NVIDIA Web Helper' -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Seconds 3
    }
}
if (-not $good) {
    Start-Process -FilePath $NodeExe -WorkingDirectory $NodeWd
    $ok = $false
    foreach ($i in 1..40) {
        Start-Sleep -Seconds 1
        if (Test-Http200 $NodeProbe) { $ok = $true; break }
    }
    if (-not $ok) { Fail 'node ไม่ตอบ 200 ที่ :59001 ภายใน 40 วิ — หยุดบูต (ดู nvnode.log / NvNode\Logs)' }
    Write-Host '[3] node :59001 → 200'
}

# ---------- [4] helper (ต้องหลัง node 200 เท่านั้น — สตาร์ตก่อน = ตายใน ~20 วิ HELPR 8116) ----------
Write-Host '=== [4] helper (หลัง node 200 เท่านั้น) ==='
$hp    = @(Get-CimInstance Win32_Process -Filter "Name='nvsphelper64.exe'" -ErrorAction SilentlyContinue)
$hpOur = @($hp | Where-Object { $_.ExecutablePath -ieq $HelperExe })
if ($hpOur.Count -eq 0) {
    if ($hp.Count -gt 0) {
        Write-Host '  helper ตัวเกิน path อื่นรันอยู่ — kill แล้วเริ่มตัวที่ถูกต้อง'
        Get-Process nvsphelper64 -ErrorAction SilentlyContinue | Stop-Process -Force
        Start-Sleep -Seconds 2
    }
    Start-Process -FilePath $HelperExe -WorkingDirectory (Split-Path $HelperExe)
    Start-Sleep -Seconds 5
}
Write-Host ('[4] helper: ' + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count + ' ตัว (' + $HelperExe + ')')

# ---------- [5] Share (attach mode — Share ยืนรอ container เข้าเกาะ) ----------
Write-Host '=== [5] Share (attach mode) ==='
if (-not (Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue)) {
    Start-Process -FilePath $ShareExe -WorkingDirectory $ShareWd
    Start-Sleep -Seconds 10
}
Start-Sleep -Seconds 20
$shareCount = @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count
Write-Host ('[5] Share: ' + $shareCount + ' ตัว (attach mode ×2 = ปกติ — spawn mode จะ crash 0xc0000005)')

# ---------- [6] re-arm POST /Launch + รายงาน + tail gate ----------
Write-Host '=== [6] re-arm POST /Launch (ทุกรอบหลัง container cycle) ==='
try {
    $r = Invoke-WebRequest -Uri $LaunchUrl -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 15
    Write-Host ('[6] Launch → ' + $r.StatusCode)
} catch { Write-Host ('[6] Launch → ' + $_.Exception.Message) }
Start-Sleep -Seconds 8

$containersFinal = @(Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" -ErrorAction SilentlyContinue)
Write-Host '=== รายงานสรุป ==='
Write-Host ('service: ' + (Get-Service NvContainerLocalSystem).Status +
            ' · containers: ' + $containersFinal.Count +
            ' · node 200: ' + (Test-Http200 $NodeProbe) +
            ' · Share: ' + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count +
            ' · helper: ' + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count)
foreach ($c in $containersFinal) { Write-Host ('  container PID ' + $c.ProcessId + ' จาก ' + $c.ExecutablePath) }
$cc = 'C:\ProgramData\NVIDIA\CaptureCore.log'
if (Test-Path $cc) {
    Write-Host '--- CaptureCore.log (tail gate) ---'
    Get-Content $cc -Tail 5
}
Write-Host '=== จบบูต — กด Alt+Z เพื่อเปิด Overlay ==='
Stop-Transcript | Out-Null
