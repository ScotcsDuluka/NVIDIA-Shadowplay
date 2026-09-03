Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class KeySim {
    [DllImport("user32.dll")]
    public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
}
'@
[KeySim]::keybd_event(0x12, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 80
[KeySim]::keybd_event(0x78, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 80
[KeySim]::keybd_event(0x78, 0, 2, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 80
[KeySim]::keybd_event(0x12, 0, 2, [UIntPtr]::Zero)
