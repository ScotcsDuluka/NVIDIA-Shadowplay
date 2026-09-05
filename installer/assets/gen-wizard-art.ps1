# gen-wizard-art.ps1 — v3: REAL NVIDIA installer skin.
# Window #333333 flat (Main_BG.png), NVIDIA Sans fonts (from NVI2), green
# #76B900 accents (theme.cfg SideBarDoneTextColor), header hairline, floor glow.
#   powershell -File installer\assets\gen-wizard-art.ps1

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$outDir = $PSScriptRoot

$W = 1580; $H = 1060  # 2x supersample - TBitmapImage stretches it down, stays crisp
$bg    = [Drawing.Color]::FromArgb(51, 51, 51)      # #333333 (real Main_BG)
$green = [Drawing.Color]::FromArgb(118, 185, 0)     # #76B900 (real theme.cfg)
$white = [Drawing.Color]::FromArgb(255, 255, 255)
$gray  = [Drawing.Color]::FromArgb(137, 137, 137)   # #898989 (real theme.cfg)

# register the real NVIDIA Sans fonts for this process so GDI+ can render them
$fonts = @('NVIDIASans_Rg.ttf', 'NVIDIASans_Md.ttf', 'NVIDIASans_Bd.ttf')
foreach ($f in $fonts) {
    $p = Join-Path $outDir $f
    if (Test-Path $p) {
        Add-Type -Name NativeFont -Namespace Win32 -MemberDefinition '[DllImport("gdi32.dll")] public static extern int AddFontResource(string lpszFilename);'
        [Win32.NativeFont]::AddFontResource($p) | Out-Null
    }
}

$bmp = New-Object Drawing.Bitmap($W, $H)
$g = [Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'
$g.TextRenderingHint = 'AntiAliasGridFit'
$g.Clear($bg)

# ── mark + wordmark row (top-left, NVIDIA Sans) ──
$mx = 128; $my = 112; $mR = 50
foreach ($i in 1..5) {
    $a = [int](5 * (6 - $i))
    $glow = New-Object Drawing.Pen([Drawing.Color]::FromArgb($a, 118, 185, 0), (4 + $i))
    $glow.StartCap = 'Round'; $glow.EndCap = 'Round'
    $g.DrawEllipse($glow, $mx - $mR - $i * 2, $my - $mR - $i * 2, ($mR + $i * 2) * 2, ($mR + $i * 2) * 2)
}
$ring = New-Object Drawing.Pen($green, 6)
$ring.StartCap = 'Round'; $ring.EndCap = 'Round'
$g.DrawEllipse($ring, $mx - $mR, $my - $mR, $mR * 2, $mR * 2)
$slit = New-Object Drawing.Pen($white, 6)
$slit.StartCap = 'Round'; $slit.EndCap = 'Round'
$g.DrawLine($slit, $mx - 19, $my, $mx + 19, $my)

$fBrand = $null; $fSub = $null; $fTiny = $null
try   { $fBrand = New-Object Drawing.Font('NVIDIA Sans Bd', 30, [Drawing.FontStyle]::Bold) }
catch { $fBrand = New-Object Drawing.Font('Segoe UI', 16, [Drawing.FontStyle]::Bold) }
try   { $fSub = New-Object Drawing.Font('NVIDIA Sans', 19) } catch { $fSub = New-Object Drawing.Font('Segoe UI', 10) }
try   { $fTiny = New-Object Drawing.Font('NVIDIA Sans', 16) } catch { $fTiny = New-Object Drawing.Font('Segoe UI', 8.5) }

$g.DrawString('NVIDIA', $fBrand, [Drawing.Brushes]::White, ($mx + 76), ($my - 46))
$g.DrawString('S H A D O W P L A Y', $fSub, (New-Object Drawing.SolidBrush($green)), ($mx + 80), ($my + 10))

# green hairline under the header row
$hair = New-Object Drawing.Pen([Drawing.Color]::FromArgb(80, 118, 185, 0), 1)
$g.DrawLine($hair, 88, 216, $W - 88, 216)

# green floor glow
$fr = [Drawing.Rectangle]::new(0, $H - 220, $W, 220)
$fbr = New-Object Drawing.Drawing2D.LinearGradientBrush($fr, [Drawing.Color]::FromArgb(0, 118, 185, 0), [Drawing.Color]::FromArgb(26, 118, 185, 0), 90)
$g.FillRectangle($fbr, $fr)

# green progress baseline hint (the real installer's bottom bar)
$bar = New-Object Drawing.Pen([Drawing.Color]::FromArgb(60, 118, 185, 0), 2)
$g.DrawLine($bar, 0, $H - 2, $W, $H - 2)

$g.DrawString('Hardware-accelerated H.264 capture - QSV / NVENC - 60 fps CFR', $fTiny, (New-Object Drawing.SolidBrush($gray)), 44, $H - 32)

$g.Dispose()
$bmp.Save((Join-Path $outDir 'welcome-bg.bmp'), [Drawing.Imaging.ImageFormat]::Bmp)
$bmp.Dispose()
Write-Host "welcome-bg v3 (real NVIDIA skin) written"
