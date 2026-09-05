# gen-wizard-art.ps1 — renders the installer wizard artwork (System.Drawing).
# Outputs 24-bit BMPs next to this script (Inno TBitmapImage loads BMP only).
#   powershell -File installer\assets\gen-wizard-art.ps1

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$outDir = $PSScriptRoot

$W = 824; $H = 560
$panel = 300            # brand panel width
$bg = [Drawing.Color]::FromArgb(13, 13, 13)        # #0D0D0D
$panelTop = [Drawing.Color]::FromArgb(24, 24, 26)
$panelBot = [Drawing.Color]::FromArgb(10, 10, 11)
$green = [Drawing.Color]::FromArgb(118, 185, 0)    # #76B900 NVIDIA-class green
$white = [Drawing.Color]::FromArgb(245, 245, 245)
$gray  = [Drawing.Color]::FromArgb(150, 152, 155)

$bmp = New-Object Drawing.Bitmap($W, $H)
$g = [Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'
$g.TextRenderingHint = 'AntiAliasGridFit'

# right content area (flat dark)
$g.Clear($bg)

# left brand panel: vertical gradient
$rect = New-Object Drawing.Rectangle(0, 0, $panel, $H)
$brush = New-Object Drawing.Drawing2D.LinearGradientBrush($rect, $panelTop, $panelBot, 90)
$g.FillRectangle($brush, $rect)

# green accent line on the panel edge
$pen = New-Object Drawing.Pen($green, 2)
$g.DrawLine($pen, $panel - 1, 0, $panel - 1, $H)

# ── logo mark: glowing aperture ring ──
$cx = [int]($panel / 2); $cy = 150; $R = 64
# outer glow (several translucent rings)
foreach ($i in 1..7) {
    $a = [int](3 * (8 - $i))
    $glow = New-Object Drawing.Pen([Drawing.Color]::FromArgb($a, 118, 185, 0), (6 + $i * 2))
    $glow.StartCap = 'Round'; $glow.EndCap = 'Round'
    $g.DrawArc($glow, $cx - $R - $i * 2, $cy - $R - $i * 2, ($R + $i * 2) * 2, ($R + $i * 2) * 2, 115, 290)
}
# main ring (green, thick, with a gap — aperture feel)
$ring = New-Object Drawing.Pen($green, 9)
$ring.StartCap = 'Round'; $ring.EndCap = 'Round'
$g.DrawArc($ring, $cx - $R, $cy - $R, $R * 2, $R * 2, 115, 290)
# inner ring (dimmer, offset)
$inner = New-Object Drawing.Pen([Drawing.Color]::FromArgb(90, 118, 185, 0), 5)
$g.DrawArc($inner, $cx - 30, $cy - 30, 60, 60, 295, 290)
# center slit
$slit = New-Object Drawing.Pen($white, 4)
$slit.StartCap = 'Round'; $slit.EndCap = 'Round'
$g.DrawLine($slit, $cx - 16, $cy, $cx + 16, $cy)

# ── wordmark ──
$fBrand = New-Object Drawing.Font('Segoe UI', 30, [Drawing.FontStyle]::Bold)
$fSub   = New-Object Drawing.Font('Segoe UI', 15, [Drawing.FontStyle]::Regular)
$fTiny  = New-Object Drawing.Font('Segoe UI', 8.5)
$g.DrawString('NVIDIA', $fBrand, [Drawing.Brushes]::White, ($cx - 78), 250)
$g.DrawString('S H A D O W P L A Y', $fSub, (New-Object Drawing.SolidBrush($green)), ($cx - 92), 300)
$g.DrawString('Screen capture, rebuilt.', $fTiny, (New-Object Drawing.SolidBrush($gray)), ($cx - 62), 340)

# bottom-left meta
$g.DrawString('Hardware-accelerated recording', $fTiny, (New-Object Drawing.SolidBrush($gray)), 20, $H - 58)
$g.DrawString('H.264 QSV / NVENC - 60 fps CFR', $fTiny, (New-Object Drawing.SolidBrush($gray)), 20, $H - 40)

# subtle vignette on the right side (bottom gradient)
$vr = [Drawing.Rectangle]::new($panel, $H - 90, $W - $panel, 90)
$vbr = New-Object Drawing.Drawing2D.LinearGradientBrush($vr, [Drawing.Color]::FromArgb(0, 0, 0, 0), [Drawing.Color]::FromArgb(16, 118, 185, 0), 90)
$g.FillRectangle($vbr, $vr)

$g.Dispose()
$bmp.Save((Join-Path $outDir 'welcome-bg.bmp'), [Drawing.Imaging.ImageFormat]::Bmp)
$bmp.Dispose()

# ── small square logo (for inner page headers) 48x48 ──
$S = 48
$sBmp = New-Object Drawing.Bitmap($S, $S)
$sg = [Drawing.Graphics]::FromImage($sBmp)
$sg.SmoothingMode = 'AntiAlias'
$sg.Clear([Drawing.Color]::FromArgb(13, 13, 13))
$sx = 24; $sy = 24; $sR = 20
foreach ($i in 1..4) {
    $a = [int](6 * (5 - $i))
    $glow = New-Object Drawing.Pen([Drawing.Color]::FromArgb($a, 118, 185, 0), (3 + $i))
    $sg.DrawArc($glow, $sx - $sR - $i, $sy - $sR - $i, ($sR + $i) * 2, ($sR + $i) * 2, 115, 290)
}
$ring2 = New-Object Drawing.Pen($green, 5)
$ring2.StartCap = 'Round'; $ring2.EndCap = 'Round'
$sg.DrawArc($ring2, $sx - $sR, $sy - $sR, $sR * 2, $sR * 2, 115, 290)
$slit2 = New-Object Drawing.Pen($white, 3)
$slit2.StartCap = 'Round'; $slit2.EndCap = 'Round'
$sg.DrawLine($slit2, $sx - 8, $sy, $sx + 8, $sy)
$sg.Dispose()
$sBmp.Save((Join-Path $outDir 'logo-sm.bmp'), [Drawing.Imaging.ImageFormat]::Bmp)
$sBmp.Dispose()

Write-Host "assets written to $outDir"
