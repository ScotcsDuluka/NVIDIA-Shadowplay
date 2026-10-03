# start-osc.ps1 — บูตสายแท้ (genuine) 6-step idempotent ตาม PATH-MANIFEST §10
# OWNER ORDER 2026-10-03: default = สายแท้เท่านั้น
#   [0] stage binary แท้จาก Project\Overlay OSC\Docs ShadowPlay - Real\...\Payload\ เข้า build ก่อนบูตเสมอ
#   ของแท้หาไม่เจอ = FAIL ชัดเจน (exit 1) — ห้าม fallback ไป shim เงียบ ๆ (shim เฉพาะ -Mode ours)
# ลำดับ §10: [0] stage → [1] service → [2] containers ×3 + agent → [3] node :59001 → [4] helper → [5] Share (attach) → [6] re-arm + รายงาน
# สูตรที่ยึด: Set-ItemProperty เท่านั้น (ไม่ sc config) · helper ต้องหลัง node 200 · re-arm /Launch ทุกรอบ · ห้าม nv-osc=false
param([string]$Mode = 'genuine', [switch]$RecoverSpUser)

# $RecoverSpUser = recovery procedure แยก (input Phase 2): kill เฉพาะ SPUser container แล้วปล่อย Watchdog respawn
# (FACT A4: SPUser-only respawn ปลอดภัย — Share รอด · ตัวฆ่า Share = full service restart เท่านั้น ห้ามใช้ตอน recovery)
if ($RecoverSpUser) {
    Write-Host '=== RECOVERY: kill เฉพาะ SPUser container (Watchdog respawn เอง) ==='
    Get-CimInstance Win32_Process | Where-Object { $_.Name -eq "nvcontainer.exe" -and $_.CommandLine -match "SPUser" } | ForEach-Object {
        Write-Host ("[KILL] SPUser PID " + $_.ProcessId)
        Stop-Process -Id $_.ProcessId -Force
    }
    foreach ($i in 1..20) {
        Start-Sleep -Seconds 2
        $sp = Get-CimInstance Win32_Process | Where-Object { $_.Name -eq "nvcontainer.exe" -and $_.CommandLine -match "SPUser" }
        if ($sp) { Write-Host ("SPUser respawn: PID " + $sp.ProcessId); break }
    }
    exit 0
}

$ErrorActionPreference = 'Continue'
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
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
# Copy-Genuine: idempotent — ข้ามถ้าเนื้อไฟล์ตรง (มีอยู่แล้ว หรือถูก lock โดย container ที่รันจากมัน)
# ล็อกแต่เนื้อไม่ตรง = เตือน + ข้าม (runtime วางไว้เองและกำลังใช้ — แทนไม่ได้ตอนนี้) · ไฟล์หายจริง = fail ชัด
function Copy-Genuine([string]$src, [string]$dst) {
    if (Test-Path $dst -PathType Leaf) {
        if (Same-File $src $dst) { Write-Host ('  ข้าม (เนื้อไฟล์ตรง): ' + (Split-Path $dst -Leaf)); return }
        try { Copy-Item $src $dst -Force } catch {
            if (Same-File $src $dst) { Write-Host ('  ข้าม (ล็อกแต่เนื้อไฟล์ตรง): ' + (Split-Path $dst -Leaf)); return }
            Write-Host ('  ⚠ ล็อกโดย process ที่รันอยู่ — ข้าม (ใช้ของที่มีอยู่): ' + (Split-Path $dst -Leaf) + ' — ' + $_.Exception.Message)
            return
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
    # ข้ามได้เมื่อ exe แท้ + node_modules อยู่ครบ (บั๊กเดิม: Copy-Item wildcard flatten node_modules ลง root — node ตายทันที)
    if ((Test-Path $nhPath) -and ((Get-Item $nhPath).Length -ge 20MB) -and (Test-Path (Join-Path $nodeDst 'node_modules'))) { $needNode = $false }
    if ($needNode) {
        if (-not (Test-Path (Join-Path $payload.FullName 'NvNode\NVIDIA Web Helper.exe'))) { Fail ('ขาด node แท้: ' + (Join-Path $payload.FullName 'NvNode\NVIDIA Web Helper.exe')) }
        Copy-Genuine-Tree (Join-Path $payload.FullName 'NvNode') $nodeDst
    }
    if (-not (Test-Path $nhPath)) { Fail ('stage node ไม่สำเร็จ: ' + $nhPath) }
    if ((Get-Item $nhPath).Length -lt 20MB) { Fail ('node ที่ stage ไม่ใช่ตัวแท้ (' + [math]::Round((Get-Item $nhPath).Length / 1MB, 1) + 'MB — น่าจะเป็น shim) — หยุดบูต') }
    # MessageBus.dll ใน payload node tree = x64 (ผิด arch สำหรับ node x86 — ทำ bridge fail 193) — บังคับใช้ตัว x86 จาก NvContainerX86Dlls
    $mb86 = Join-Path $payload.FullName 'NvContainerX86Dlls\MessageBus.dll'
    if (-not (Test-Path $mb86)) { Fail ('ขาด MessageBus x86: ' + $mb86) }
    Copy-Genuine $mb86 (Join-Path $nodeDst 'MessageBus.dll')

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

    # 0e) PF anchor restore (§7.1 + ต้นไม้ node ที่ node แท้ hardcode หา) — idempotent + manifest hash ไว้เทียบทุกบูต
    # FACT §18: node แท้ resolve script root = SHGetFolderPath(PF_x86) + "\NVIDIA Corporation\NvNode\index.js" (compiled-in ไม่มี override)
    Write-Host '=== [0e] PF anchor restore (node tree + §7.1 mandatory) ==='
    if (-not $isAdmin) { Fail '[0e] ต้องเขียน Program Files — รัน start-osc แบบ elevated (Run as Administrator)' }
    $gfeExe = 'C:\My Project\GFE\GeForce_Experience_v3.28.0.412'
    $pf86 = 'C:\Program Files (x86)\NVIDIA Corporation'
    $pf64 = 'C:\Program Files\NVIDIA Corporation'
    $anchorJobs = @(
        @{ Key = 'PF(x86)\NvNode (node tree)';            Src = (Join-Path $payload.FullName 'NvNode');                         Dst = (Join-Path $pf86 'NvNode');                Tree = $true },
        @{ Key = 'PF\NvBackend (agent home — §2 map)';    Src = (Join-Path $payload.FullName 'NvBackend');                      Dst = (Join-Path $pf64 'NvBackend');             Tree = $true },
        @{ Key = 'PF(x86)\NvStreamSrv (node bridge)';     Src = (Join-Path $gfeExe 'GFExperience.NvStreamSrv\x86\server');      Dst = (Join-Path $pf86 'NvStreamSrv');           Tree = $true },
        @{ Key = 'PF\NvStreamSrv (x64 server — §3.2)';    Src = (Join-Path $gfeExe 'GFExperience.NvStreamSrv\amd64\server');    Dst = (Join-Path $pf64 'NvStreamSrv');           Tree = $true },
        @{ Key = 'PF(x86)\NvContainer (x86 dlls §3.3)';   Src = (Join-Path $payload.FullName 'NvContainerX86Dlls');             Dst = (Join-Path $pf86 'NvContainer');           Tree = $true },
        @{ Key = 'PF\NvContainer\MessageBus (x64)';       Src = (Join-Path $payload.FullName 'NvContainer\MessageBus.dll');     Dst = (Join-Path $pf64 'NvContainer');           Tree = $false },
        @{ Key = 'PF\NvContainer\libprotobuf (x64)';      Src = (Join-Path $payload.FullName 'NvContainer\libprotobuf.dll');    Dst = (Join-Path $pf64 'NvContainer');           Tree = $false },
        @{ Key = 'PF\NvContainer\libcrypto (x64)';        Src = (Join-Path $payload.FullName 'NvContainer\libcrypto-1_1.dll');  Dst = (Join-Path $pf64 'NvContainer');           Tree = $false },
        @{ Key = 'PF\NvContainer\libssl (x64)';           Src = (Join-Path $payload.FullName 'NvContainer\libssl-1_1.dll');     Dst = (Join-Path $pf64 'NvContainer');           Tree = $false },
        @{ Key = 'PF\NvContainer\Poco (x64)';             Src = (Join-Path $payload.FullName 'NvContainer\Poco.dll');           Dst = (Join-Path $pf64 'NvContainer');           Tree = $false },
        @{ Key = 'PF\NvContainer\PocoInit (x64)';         Src = (Join-Path $payload.FullName 'NvContainer\PocoInitializer.dll'); Dst = (Join-Path $pf64 'NvContainer');          Tree = $false },
        @{ Key = 'PF\GFE\dependencies\CrimsonUtil';       Src = (Join-Path $gfeExe 'GFExperience\dependencies\CrimsonUtil.dll'); Dst = (Join-Path $pf64 'NVIDIA GeForce Experience\dependencies'); Tree = $false },
        @{ Key = 'ProgramData piplConfig (§6 seed)';      Src = (Join-Path $payload.FullName 'ProgramDataSeeds\NvNode\piplConfig.json'); Dst = (Join-Path $env:ProgramData 'NVIDIA Corporation\NvNode'); Tree = $false },
        @{ Key = 'PF(x86)\NvTelemetry (API32/Bridge32)';  Src = (Join-Path $gfeExe 'NvTelemetry');                              Dst = (Join-Path $pf86 'NvTelemetry');           Tree = $true },
        @{ Key = 'PF\NvTelemetry (API64/Bridge64)';       Src = (Join-Path $gfeExe 'NvTelemetry');                              Dst = (Join-Path $pf64 'NvTelemetry');           Tree = $true },
        @{ Key = 'PF\ShadowPlay (helper anchor — full set)'; Src = (Join-Path $payload.FullName 'ShadowPlay');                  Dst = (Join-Path $pf64 'ShadowPlay');            Tree = $true },
        @{ Key = 'PF(x86)\ShadowPlay (x86 set — node symlink A7)'; Src = (Join-Path $gfeExe 'ShadowPlay');                        Dst = (Join-Path $pf86 'ShadowPlay');            Tree = $true },
        @{ Key = 'PF(x86)\Update Core\NvBackendAPI32';    Src = (Join-Path $gfeExe 'NvBackend\NvBackendAPI32.dll');             Dst = (Join-Path $pf86 'Update Core');           Tree = $false },
        @{ Key = 'PF(x86)\Update Core\NvTmRep';           Src = (Join-Path $payload.FullName 'NvBackend\NvTmRep.exe');          Dst = (Join-Path $pf86 'Update Core');           Tree = $false },
        @{ Key = 'PF(x86)\Update Core\NvSHIM';            Src = (Join-Path $payload.FullName 'NvBackend\NvSHIM.exe');           Dst = (Join-Path $pf86 'Update Core');           Tree = $false },
        @{ Key = 'PF(x86)\Update Core\AppOntology';       Src = (Join-Path $payload.FullName 'NvBackend\ApplicationOntology.7z'); Dst = (Join-Path $pf86 'Update Core');         Tree = $false },
        @{ Key = 'PF(x86)\Update Core\FeatureWhitelist';  Src = (Join-Path $payload.FullName 'NvBackend\FeatureWhitelist.json'); Dst = (Join-Path $pf86 'Update Core');          Tree = $false },
        @{ Key = 'PF\Update Core\NvBackendAPI64';         Src = (Join-Path $gfeExe 'NvBackend\NvBackendAPI64.dll');             Dst = (Join-Path $pf64 'Update Core');           Tree = $false },
        @{ Key = 'PF\NvDriverUpdateCheck64';              Src = (Join-Path $gfeExe 'NvBackend\NvDriverUpdateCheck64.dll');      Dst = (Join-Path $pf64 'NvDriverUpdateCheck');   Tree = $false },
        @{ Key = 'PF(x86)\ShadowPlay\nvspapi';            Src = (Join-Path $gfeExe 'ShadowPlay\nvspapi.dll');                   Dst = (Join-Path $pf86 'ShadowPlay');            Tree = $false },
        @{ Key = 'PF(x86)\ShadowPlay\ipccommon';          Src = (Join-Path $gfeExe 'ShadowPlay\ipccommon.dll');                 Dst = (Join-Path $pf86 'ShadowPlay');            Tree = $false },
        @{ Key = 'PF\ShadowPlay\nvspapi64';               Src = (Join-Path $payload.FullName 'ShadowPlay\nvspapi64.dll');       Dst = (Join-Path $pf64 'ShadowPlay');            Tree = $false },
        @{ Key = 'PF\ShadowPlay\ipccommon64';             Src = (Join-Path $payload.FullName 'ShadowPlay\ipccommon64.dll');     Dst = (Join-Path $pf64 'ShadowPlay');            Tree = $false }
    )
    $restored = 0; $verified = 0
    foreach ($job in $anchorJobs) {
        if (-not (Test-Path $job.Src)) { Fail ('[0e] แหล่งของแท้หาย: ' + $job.Src) }
        if ($job.Tree) {
            $idxDst = Join-Path $job.Dst 'index.js'
            if (-not ((Test-Path $idxDst) -and (Test-Path (Join-Path $job.Dst 'node_modules')))) {
                Write-Host ('  restore tree → ' + $job.Dst)
                Copy-Genuine-Tree $job.Src $job.Dst
                $restored++
            } else { $verified++ }
        } else {
            $dstFile = Join-Path $job.Dst (Split-Path $job.Src -Leaf)
            if (-not (Test-Path $dstFile)) {
                New-Item -ItemType Directory -Path $job.Dst -Force | Out-Null
                Copy-Genuine $job.Src $dstFile
                Write-Host ('  restore ' + $job.Key)
                $restored++
            } else { $verified++ }
        }
    }
    # manifest ไว้เทียบ (จำนวนไฟล์ + hash) — NVIDIA App ลบซ้ำ = เทียบเจอทันที
    # MessageBus.dll ใน node tree ฝั่ง PF(x86) ก็เป็น x64 ปนมาเหมือนกัน — บังคับ x86 เช่นเดียวกับ build\NvNode
    Copy-Genuine $mb86 (Join-Path $pf86 'NvNode\MessageBus.dll')
    $manifestPath = Join-Path $Log 'pf-anchor-manifest.txt'
    $entries = @()
    foreach ($job in $anchorJobs) {
        if ($job.Tree) {
            Get-ChildItem $job.Dst -Recurse -File | ForEach-Object { $entries += ((Get-FileHash $_.FullName -Algorithm MD5).Hash + '  ' + $_.FullName) }
        } else {
            $dstFile = Join-Path $job.Dst (Split-Path $job.Src -Leaf)
            $entries += ((Get-FileHash $dstFile -Algorithm MD5).Hash + '  ' + $dstFile)
        }
    }
    if (Test-Path $manifestPath) {
        $prevCount = @(Get-Content $manifestPath).Count
        if ($prevCount -ne $entries.Count) { Write-Host ('  ⚠ จำนวนไฟล์ anchor เปลี่ยน: ' + $prevCount + ' → ' + $entries.Count) }
    }
    $entries | Set-Content -Path $manifestPath -Encoding UTF8
    Write-Host ('[0e] PF anchor: ' + $entries.Count + ' ไฟล์ (restore ' + $restored + ' · มีอยู่แล้ว ' + $verified + ') · manifest → ' + $manifestPath)

    # 0f) GFE dir hardlink (§11 proven — COscProcMgr หา OSC exe ที่ Program Files ตรง ๆ ไม่ใช่ FullPath registry)
    Write-Host '=== [0f] hardlink build Share tree → PF GFE dir ==='
    $gfeDir = Join-Path $pf64 'NVIDIA GeForce Experience'
    $linked = 0; $skipped = 0; $failed = 0
    Get-ChildItem $ShareWd -Recurse -File | ForEach-Object {
        $rel = $_.FullName.Substring($ShareWd.Length + 1)
        $t = Join-Path $gfeDir $rel
        $tdir = Split-Path $t -Parent
        if (-not (Test-Path $tdir)) { New-Item -ItemType Directory -Force -Path $tdir | Out-Null }
        if (Test-Path $t) { $skipped++; return }
        cmd /c mklink /H "`"$t`"" "`"$($_.FullName)`"" | Out-Null
        if ($LASTEXITCODE -eq 0) { $linked++ } else { $failed++; Write-Host ('  FAIL: ' + $rel) }
    }
    Write-Host ('[0f] GFE hardlink: linked=' + $linked + ' skipped=' + $skipped + ' failed=' + $failed)

    # 0g) runtime data dirs (§6 — สร้างล่วงหน้า idempotent ก่อน service start — ทุก plugin resolve path ตอน init ไม่ retry)
    Write-Host '=== [0g] runtime data dirs (§6) ==='
    $runtimeDirs = @(
        (Join-Path $env:ProgramData 'NVIDIA Corporation\NvNode'),
        (Join-Path $env:ProgramData 'NVIDIA Corporation\ShadowPlay'),
        (Join-Path $env:ProgramData 'NVIDIA'),
        (Join-Path $env:LOCALAPPDATA 'NVIDIA Corporation\NvNode'),
        (Join-Path $env:LOCALAPPDATA 'NVIDIA Corporation\NVIDIA Share')
    )
    foreach ($d in $runtimeDirs) {
        if (-not (Test-Path $d)) { New-Item -ItemType Directory -Path $d -Force | Out-Null; Write-Host ('  สร้าง: ' + $d) }
    }
    Write-Host ('[0g] runtime dirs ok (' + $runtimeDirs.Count + ')')

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

# ---------- [1] A1 Light-era order: หยุด service ก่อน (Share ต้องมีชีวิตก่อน container init — §11) ----------
Write-Host '=== [1] หยุด service (A1: Share ต้องมาก่อน container init) ==='
$svc = Get-Service NvContainerLocalSystem -ErrorAction SilentlyContinue
if (-not $svc) { Fail 'ไม่มี service NvContainerLocalSystem — restore registry ก่อน (PATH-MANIFEST §3.1)' }
if (-not $isAdmin) { Fail '[1] ต้อง stop/start service — รัน start-osc แบบ elevated (Run as Administrator)' }
reg.exe add 'HKLM\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem' /v FailureActions /t REG_BINARY /d 0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 /f | Out-Null
if ($svc.Status -eq 'Running') {
    sc.exe stop NvContainerLocalSystem | Out-Null
    Start-Sleep -Seconds 5
}
Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host ('[KILL] nvcontainer PID ' + $_.ProcessId)
    Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
}
Start-Sleep -Seconds 2
Write-Host '[1] service หยุดแล้ว (จะสตาร์ตหลัง Share ยืนรอ — ขั้น [2])'
$ip = (Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem' -ErrorAction SilentlyContinue).ImagePath
Write-Host ('[1] ImagePath: ' + $ip)

# ---------- [1b] registry กุญแจ (สูตรพิสูจน์แล้วเดิม — NVIDIA App ไม่ใช้ค่าเหล่านี้ การฟื้นไม่กระทบมัน) ----------
if ($Mode -eq 'genuine') {
    Write-Host '=== [1b] registry: Global\NvNode + GFExperience\FullPath (idempotent) ==='
    if (-not $isAdmin) { Fail '[1b] ต้องเขียน HKLM — รัน start-osc แบบ elevated (Run as Administrator)' }
    $regJobs = @(
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\Global\NvNode';              N = 'port';           T = 'DWord';  V = 59001 },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\Global\NvNode';              N = 'disableSecurity'; T = 'DWord'; V = 1 },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\NvNode';  N = 'port';           T = 'DWord';  V = 59001 },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\NvNode';  N = 'disableSecurity'; T = 'DWord'; V = 1 },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\Global\GFExperience';        N = 'FullPath';       T = 'String'; V = $ShareExe },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\GFExperience'; N = 'FullPath'; T = 'String'; V = $ShareExe },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\Global\GFExperience';        N = 'Version';        T = 'String'; V = '3.28.0.412' },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\Global\GFExperience';        N = 'Installed';      T = 'DWord';  V = 1 },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\Global\GFExperience';        N = 'GFEBundledNGXVersion'; T = 'String'; V = '' },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\GFExperience'; N = 'Version';  T = 'String'; V = '3.28.0.412' },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\GFExperience'; N = 'Installed'; T = 'DWord'; V = 1 },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\GFExperience'; N = 'Architecture'; T = 'String'; V = 'x64' },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\Global\ShadowPlay\NVSPCAPS'; N = 'IsShadowPlayEnabled'; T = 'DWord'; V = 1 },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\Global\ShadowPlay\NVSPCAPS'; N = 'IsShadowPlayEnabledUser'; T = 'DWord'; V = 1 },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\ShadowPlay\NVSPCAPS'; N = 'IsShadowPlayEnabled'; T = 'DWord'; V = 1 },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\ShadowPlay\NVSPCAPS'; N = 'IsShadowPlayEnabledUser'; T = 'DWord'; V = 1 },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'MessageBus.dll'; T = 'String'; V = 'C:\Program Files\NVIDIA Corporation\NvContainer\MessageBus.dll' },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'libprotobuf.dll'; T = 'String'; V = 'C:\Program Files\NVIDIA Corporation\NvContainer\libprotobuf.dll' },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'libcrypto-1_1.dll'; T = 'String'; V = 'C:\Program Files\NVIDIA Corporation\NvContainer\libcrypto-1_1.dll' },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'libssl-1_1.dll'; T = 'String'; V = 'C:\Program Files\NVIDIA Corporation\NvContainer\libssl-1_1.dll' },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'Poco.dll'; T = 'String'; V = 'C:\Program Files\NVIDIA Corporation\NvContainer\Poco.dll' },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'PocoInitializer.dll'; T = 'String'; V = 'C:\Program Files\NVIDIA Corporation\NvContainer\PocoInitializer.dll' },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'NvStreamBase.dll'; T = 'String'; V = 'C:\Program Files\NVIDIA Corporation\NvStreamSrv\NvStreamBase.dll' },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'RtspServer.dll'; T = 'String'; V = 'C:\Program Files\NVIDIA Corporation\NvStreamSrv\RtspServer.dll' },
        @{ P = 'HKLM:\SOFTWARE\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'CrimsonUtil.dll'; T = 'String'; V = 'C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\dependencies\CrimsonUtil.dll' },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'MessageBus.dll'; T = 'String'; V = 'C:\Program Files (x86)\NVIDIA Corporation\NvContainer\MessageBus.dll' },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'libprotobuf.dll'; T = 'String'; V = 'C:\Program Files (x86)\NVIDIA Corporation\NvContainer\libprotobuf.dll' },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'libcrypto-1_1.dll'; T = 'String'; V = 'C:\Program Files (x86)\NVIDIA Corporation\NvContainer\libcrypto-1_1.dll' },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'libssl-1_1.dll'; T = 'String'; V = 'C:\Program Files (x86)\NVIDIA Corporation\NvContainer\libssl-1_1.dll' },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'Poco.dll'; T = 'String'; V = 'C:\Program Files (x86)\NVIDIA Corporation\NvContainer\Poco.dll' },
        @{ P = 'HKLM:\SOFTWARE\WOW6432Node\NVIDIA Corporation\NvContainer\ModuleMap'; N = 'PocoInitializer.dll'; T = 'String'; V = 'C:\Program Files (x86)\NVIDIA Corporation\NvContainer\PocoInitializer.dll' }
    )
    foreach ($r in $regJobs) {
        if (-not (Test-Path $r.P)) { New-Item -Path $r.P -Force | Out-Null }
        $cur = (Get-ItemProperty -Path $r.P -ErrorAction SilentlyContinue).($r.N)
        if ($cur -ne $r.V) {
            Set-ItemProperty -Path $r.P -Name $r.N -Value $r.V -Type $r.T
            if ($null -ne $cur) { $note = ' (เดิม: ' + $cur + ')' } else { $note = ' (สร้างใหม่)' }
            Write-Host ('  set ' + $r.P + ' → ' + $r.N + '=' + $r.V + $note)
        }
    }
    Write-Host '[1b] registry ok'
}

# ---------- [1c] Share attach mode (ยืนรอ ก่อน container init — Light era: "Share มีชีวิตก่อน container init") ----------
if ($Mode -eq 'genuine') {
    Write-Host '=== [1c] Share attach mode (ยืนรอก่อน service) ==='
    if (-not (Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue)) {
        Start-Process -FilePath $ShareExe -WorkingDirectory $ShareWd -WindowStyle Hidden
        Start-Sleep -Seconds 12
    }
    Write-Host ('[1c] Share: ' + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count + ' ตัว (ยืนรอ container init — Light era)')
}

# ---------- [2] start service (Share พร้อมแล้ว) + รอ SPUser container spawn ----------
Write-Host '=== [2] start service + รอ SPUser container ==='
sc.exe start NvContainerLocalSystem | Out-Null
Start-Sleep -Seconds 14
if ((Get-Service NvContainerLocalSystem).Status -ne 'Running') {
    Write-Host '  รอบแรกไม่ขึ้น — start รอบสอง (pattern ปกติของ container แท้)'
    sc.exe start NvContainerLocalSystem | Out-Null
    Start-Sleep -Seconds 14
}
$svc = Get-Service NvContainerLocalSystem
Write-Host ('[2] service: ' + $svc.Status)
if ($svc.Status -ne 'Running') { Fail '[2] service ไม่ขึ้น — ดู log ตาม -f ใน ImagePath' }
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

# ---------- [5] Share สด attach (ลำดับถาวรจาก Launch-200: node up → Share สด → Launch ครั้งเดียว) ----------
Write-Host '=== [5] Share สด (attach — ลำดับถาวร Launch-200) ==='
$zombies = @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue)
if ($zombies.Count -gt 0) {
    Write-Host ('[5] kill zombie Share ×' + $zombies.Count)
    $zombies | ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }
    Start-Sleep -Seconds 4
}
Start-Process -FilePath $ShareExe -WorkingDirectory $ShareWd -WindowStyle Hidden
Start-Sleep -Seconds 15
$shareCount = @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count
Write-Host ('[5] Share: ' + $shareCount + ' ตัว (สด — พร้อม attach)')

# ---------- [6] re-arm POST /Launch + GUARD (fail 2 ครั้ง = หยุดรายงาน ไม่วน kill) + หมุด attach ----------
Write-Host '=== [6] re-arm POST /Launch (guard: fail 2 ครั้ง = หยุด) ==='
$guardFile = Join-Path $Log 'launch-fail-count.txt'
$failCount = 0
if (Test-Path $guardFile) { [void][int]::TryParse((Get-Content $guardFile -ErrorAction SilentlyContinue), [ref]$failCount) }
$launched = $false
if ($failCount -ge 2) {
    Write-Host ('[6] GUARD: Launch fail มาแล้ว ' + $failCount + ' ครั้งติด — ไม่ยิงอีก (ตามคำสั่ง OWNER) — ตรวจ CaptureCore m_pSettings/CreateSettings ก่อนรันใหม่')
} else {
    try {
        $r = Invoke-WebRequest -Uri $LaunchUrl -Method POST -Body '{"launch":true}' -ContentType 'application/json' -UseBasicParsing -TimeoutSec 15
        Write-Host ('[6] Launch → ' + $r.StatusCode)
        $launched = $true
        Set-Content -Path $guardFile -Value '0' -Encoding UTF8
    } catch {
        $we = $_.Exception.Response
        $code = ''
        if ($we) { $code = [int]$we.StatusCode }
        Write-Host ('[6] Launch → FAIL (' + $code + ')')
        $failCount++
        Set-Content -Path $guardFile -Value ([string]$failCount) -Encoding UTF8
        Write-Host ('[6] GUARD: fail count = ' + $failCount + $(if ($failCount -ge 2) { ' — ครั้งถัดไปจะหยุด (ไม่วน kill)' } else { '' }))
    }
}
Start-Sleep -Seconds 8

# หมุด A1: CreateSettings (ต้องเกิดแล้วหลัง Launch 200 — m_pSettings ไม่ NULL)
$ccLog = 'C:\ProgramData\NVIDIA Corporation\ShadowPlay\CaptureCore.log'
if ($launched -and (Test-Path $ccLog)) {
    $cs = @(Select-String -Path $ccLog -Pattern 'CreateSettings' -SimpleMatch -ErrorAction SilentlyContinue)
    Write-Host ('[6] หมุด CreateSettings ใน CaptureCore: ' + $cs.Count + ' ครั้ง' + $(if ($cs.Count -gt 0) { ' ✓ m_pSettings น่าจะไม่ NULL แล้ว — ทดสอบ POST /Hotkey/overlaytoggle' } else { ' — ยังไม่เกิด (m_pSettings ยัง NULL)' }))
    if ($cs.Count -gt 0) {
        try {
            $req2 = [Net.WebRequest]::Create('http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/overlaytoggle')
            $req2.Method = 'POST'; $req2.ContentType = 'application/json'; $req2.Timeout = 15000
            $b2 = [Text.Encoding]::UTF8.GetBytes('{"keys":[18,90]}')
            $req2.ContentLength = $b2.Length
            $s2 = $req2.GetRequestStream(); $s2.Write($b2, 0, $b2.Length); $s2.Close()
            try { $r2 = $req2.GetResponse(); $rd2 = New-Object IO.StreamReader($r2.GetResponseStream()); Write-Host ('[6] overlaytoggle SET → ' + [int]$r2.StatusCode + ' ' + $rd2.ReadToEnd()) } catch { $w2 = $_.Exception.Response; if ($w2) { $rd2 = New-Object IO.StreamReader($w2.GetResponseStream()); Write-Host ('[6] overlaytoggle SET → ' + [int]$w2.StatusCode + ' ' + $rd2.ReadToEnd()) } else { Write-Host ('[6] overlaytoggle SET err: ' + $_.Exception.Message) } }
        } catch { Write-Host ('[6] overlaytoggle err: ' + $_.Exception.Message) }
    }
}

$containersFinal = @(Get-CimInstance Win32_Process -Filter "Name='nvcontainer.exe'" -ErrorAction SilentlyContinue)
Write-Host '=== รายงานสรุป ==='
Write-Host ('service: ' + (Get-Service NvContainerLocalSystem).Status +
            ' · containers: ' + $containersFinal.Count +
            ' · node 200: ' + (Test-Http200 $NodeProbe) +
            ' · Share: ' + @(Get-Process 'NVIDIA Share' -ErrorAction SilentlyContinue).Count +
            ' · helper: ' + @(Get-Process nvsphelper64 -ErrorAction SilentlyContinue).Count)
foreach ($c in $containersFinal) { Write-Host ('  container PID ' + $c.ProcessId + ' จาก ' + $c.ExecutablePath) }
$cc = 'C:\ProgramData\NVIDIA Corporation\ShadowPlay\CaptureCore.log'
if (Test-Path $cc) {
    Write-Host '--- CaptureCore.log (tail gate) ---'
    Get-Content $cc -Tail 5
}
Write-Host '=== จบบูต — กด Alt+Z เพื่อเปิด Overlay ==='
Stop-Transcript | Out-Null
