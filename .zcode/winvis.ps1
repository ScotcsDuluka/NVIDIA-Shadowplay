Add-Type @"
using System;
using System.Runtime.InteropServices;
public class W3 {
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
    public struct RECT { public int L, T, R, B; }
}
"@
$p = Get-Process -Name 'NVIDIA Share' -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 }
if ($p) {
    $p | ForEach-Object {
        $r = New-Object W3+RECT
        [W3]::GetWindowRect($_.MainWindowHandle, [ref]$r) | Out-Null
        $vis = [W3]::IsWindowVisible($_.MainWindowHandle)
        Write-Host ('pid=' + $_.Id + ' visible=' + $vis + ' rect=(' + $r.L + ',' + $r.T + ')-(' + $r.R + ',' + $r.B + ')')
    }
} else {
    Write-Host 'no visible window handle — screen clear'
}
