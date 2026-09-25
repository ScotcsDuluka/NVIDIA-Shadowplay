# Window geometry check for the genuine Share overlay + listener reset.
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class W {
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
    public struct RECT { public int L, T, R, B; }
}
"@

Write-Host '--- genuine Share window ---'
Get-Process -Name 'NVIDIA Share' -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | ForEach-Object {
    $r = New-Object W+RECT
    [W]::GetWindowRect($_.MainWindowHandle, [ref]$r) | Out-Null
    $vis = [W]::IsWindowVisible($_.MainWindowHandle)
    Write-Host ('pid=' + $_.Id + ' visible=' + $vis + ' rect=(' + $r.L + ',' + $r.T + ')-(' + $r.R + ',' + $r.B + ')')
}

Write-Host '--- listener reset (kill all, start one) ---'
Get-CimInstance Win32_Process -Filter "Name='powershell.exe'" |
    Where-Object { $_.CommandLine -like '*hotkey-listener*' } |
    ForEach-Object { Stop-Process -Id $_.ProcessId -Force; Write-Host ('killed listener pid=' + $_.ProcessId) }
Start-Sleep -Seconds 2
Start-Process powershell -ArgumentList '-NoProfile', '-WindowStyle', 'Hidden', '-ExecutionPolicy', 'Bypass', '-File', 'C:\Program Files (x86)\NVIDIA Corporation\NvNode\hotkey-listener.ps1'
Start-Sleep -Seconds 3
$h = Get-CimInstance Win32_Process -Filter "Name='powershell.exe'" |
    Where-Object { $_.CommandLine -like '*hotkey-listener*' }
Write-Host ('listener now: ' + $(if ($h) { 'pid=' + $h.ProcessId } else { 'NOT-RUNNING' }))
