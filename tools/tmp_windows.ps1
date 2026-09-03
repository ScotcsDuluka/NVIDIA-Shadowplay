Add-Type @'
using System;
using System.Text;
using System.Runtime.InteropServices;
public static class WinScan {
    public delegate bool EnumProc(IntPtr h, IntPtr l);
    [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc p, IntPtr l);
    [DllImport("user32.dll")] public static extern bool EnumThreadWindows(uint tid, EnumProc p, IntPtr l);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
}
$pidTarget = 31184
Write-Host "WINDOWS FOR PID $pidTarget"
$cb = [WinScan+EnumProc]{ param($h,$l); [uint32]$p=0; [WinScan]::GetWindowThreadProcessId($h,[ref]$p)|Out-Null; if($p -eq $pidTarget){$s=New-Object Text.StringBuilder 256; [WinScan]::GetWindowText($h,$s,256)|Out-Null; Write-Host "HWND=$h VISIBLE=$([WinScan]::IsWindowVisible($h)) TITLE=[$($s.ToString())]"}; $true }
[WinScan]::EnumWindows($cb,[IntPtr]::Zero)|Out-Null
$p = Get-Process -Id $pidTarget
foreach($t in $p.Threads){
  $tid=[uint32]$t.Id
  $ctb=[WinScan+EnumProc]{ param($h,$l); $s=New-Object Text.StringBuilder 256; [WinScan]::GetWindowText($h,$s,256)|Out-Null; Write-Host "THREAD HWND=$h VISIBLE=$([WinScan]::IsWindowVisible($h)) TITLE=[$($s.ToString())]"; $true }
  [WinScan]::EnumThreadWindows($tid,$ctb,[IntPtr]::Zero)|Out-Null
}
