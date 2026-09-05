# gen-custom-widgets.ps1 — renders the custom-UI widget bitmaps (2x supersampled).
#   powershell -File installer\assets\gen-custom-widgets.ps1

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$outDir = $PSScriptRoot
$green = [Drawing.Color]::FromArgb(118, 185, 0)

# register NVIDIA Sans for this process
Add-Type -Name NF -Namespace W32 -MemberDefinition '[DllImport("gdi32.dll")] public static extern int AddFontResource(string lpszFilename);'
foreach ($f in @('NVIDIASans_Rg.ttf', 'NVIDIASans_Md.ttf', 'NVIDIASans_Bd.ttf')) {
    $p = Join-Path $outDir $f
    if (Test-Path $p) { [W32.NF]::AddFontResource($p) | Out-Null }
}

function New-ButtonBmp([string]$name, [int]$r, [int]$g2, [int]$b, [string]$text, [string]$textColor) {
    $w = 300; $h = 80   # 2x of 150x40
    $bmp2 = New-Object Drawing.Bitmap($w, $h)
    $gg = [Drawing.Graphics]::FromImage($bmp2)
    $gg.SmoothingMode = 'AntiAlias'
    $gg.TextRenderingHint = 'AntiAliasGridFit'
    $gg.Clear([Drawing.Color]::FromArgb(51, 51, 51))
    $body = New-Object Drawing.SolidBrush([Drawing.Color]::FromArgb($r, $g2, $b))
    $path = New-Object Drawing.Drawing2D.GraphicsPath
    $d = 32
    $path.AddArc(0, 0, $d, $d, 180, 90)
    $path.AddArc($w - $d, 0, $d, $d, 270, 90)
    $path.AddArc($w - $d, $h - $d, $d, $d, 0, 90)
    $path.AddArc(0, $h - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    $gg.FillPath($body, $path)
    $fnt = New-Object Drawing.Font('NVIDIA Sans Bd', 22)
    $fmt = New-Object Drawing.StringFormat
    $fmt.Alignment = 'Center'; $fmt.LineAlignment = 'Center'
    $col = if ($textColor -eq 'black') { [Drawing.Brushes]::Black } else { [Drawing.Brushes]::White }
    $gg.DrawString($text, $fnt, $col, (New-Object Drawing.RectangleF(0, 0, $w, $h)), $fmt)
    $gg.Dispose()
    $bmp2.Save((Join-Path $outDir $name), [Drawing.Imaging.ImageFormat]::Bmp)
    $bmp2.Dispose()
}

New-ButtonBmp 'btn-green-install.bmp' 118 185 0 'INSTALL' 'black'
New-ButtonBmp 'btn-green-next.bmp' 118 185 0 'NEXT' 'black'
New-ButtonBmp 'btn-green-finish.bmp' 118 185 0 'FINISH' 'black'
New-ButtonBmp 'btn-gray-cancel.bmp' 42 42 44 'CANCEL' 'white'
New-ButtonBmp 'btn-gray-back.bmp' 42 42 44 'BACK' 'white'

# checkbox states 40x40 (2x of 20x20)
foreach ($state in @('on', 'off')) {
    $cb = New-Object Drawing.Bitmap(40, 40)
    $cg = [Drawing.Graphics]::FromImage($cb)
    $cg.SmoothingMode = 'AntiAlias'
    $cg.Clear([Drawing.Color]::FromArgb(51, 51, 51))
    $box = New-Object Drawing.SolidBrush([Drawing.Color]::FromArgb(26, 26, 26))
    $cpath = New-Object Drawing.Drawing2D.GraphicsPath
    $cd = 12
    $cpath.AddArc(0, 0, $cd, $cd, 180, 90)
    $cpath.AddArc(40 - $cd, 0, $cd, $cd, 270, 90)
    $cpath.AddArc(40 - $cd, 40 - $cd, $cd, $cd, 0, 90)
    $cpath.AddArc(0, 40 - $cd, $cd, $cd, 90, 90)
    $cpath.CloseFigure()
    $cg.FillPath($box, $cpath)
    if ($state -eq 'on') {
        $check = New-Object Drawing.Pen($green, 5)
        $check.StartCap = 'Round'; $check.EndCap = 'Round'
        $cg.DrawLines($check, @([Drawing.PointF]::new(10, 21), [Drawing.PointF]::new(17, 28), [Drawing.PointF]::new(30, 12)))
    }
    $cg.Dispose()
    $cb.Save((Join-Path $outDir ('cb-' + $state + '.bmp')), [Drawing.Imaging.ImageFormat]::Bmp)
    $cb.Dispose()
}
Write-Host 'custom UI widgets written'
