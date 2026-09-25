# shot.ps1 - capture the launcher window (class NvLauncherHostWindow) to a PNG.
param([string]$Out = "launcher-shot.png")
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;
public class Win32Shot {
    public delegate bool CB(IntPtr h, IntPtr l);
    [DllImport("user32.dll")] public static extern bool EnumWindows(CB cb, IntPtr l);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetClassNameW(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }
}
"@
$script:hwnd = [IntPtr]::Zero
$cb = [Win32Shot+CB] {
    param($h, $l)
    $sb = New-Object System.Text.StringBuilder 256
    [Win32Shot]::GetClassNameW($h, $sb, 256) | Out-Null
    if ($sb.ToString() -eq "NvLauncherHostWindow") { $script:hwnd = $h; return $false }
    return $true
}
[Win32Shot]::EnumWindows($cb, [IntPtr]::Zero) | Out-Null
if ($script:hwnd -eq [IntPtr]::Zero) { throw "launcher window not found" }
$r = New-Object Win32Shot+RECT
[Win32Shot]::GetWindowRect($script:hwnd, [ref]$r) | Out-Null
$w = $r.R - $r.L; $ht = $r.B - $r.T
$bmp = New-Object System.Drawing.Bitmap($w, $ht)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$dc = $g.GetHdc()
# PW_RENDERFULLCONTENT (2): includes DirectComposition/CEF child content
[Win32Shot]::PrintWindow($script:hwnd, $dc, 2) | Out-Null
$g.ReleaseHdc($dc)
$g.Dispose()
$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output "saved $Out (${w}x${ht})"
