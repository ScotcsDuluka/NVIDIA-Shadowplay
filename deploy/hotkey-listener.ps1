# hotkey-listener.ps1 — Alt+Z wire-through for driverless (Intel/AMD) hosts.
# The real hotkey owner is nvsphelperplugin64.dll (RegisterHotKey), spawned
# only by the NVIDIA capture stack. Without the driver Alt+Z is silently dead,
# so this listener owns Alt+Z itself and drives the osc page through its
# documented front door: POST to the node backend, which emits
# /ShadowPlay/v.1.0/WindowState {windowMsg:"overlayToggle"} — the exact
# message the real stack pushes (docs/osc/11-window-flow.md).
# RegisterHotKey fails if a native host already owns Alt+Z (NVIDIA machines)
# -> listener exits, no double-toggle.

$ErrorActionPreference = 'SilentlyContinue'
$logDir = "$env:LOCALAPPDATA\NVIDIA Corporation\NvNode"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$log = Join-Path $logDir 'hotkey-listener.log'
function Log($m) { Add-Content -Path $log -Value ("[{0}] {1}" -f (Get-Date -Format 'yyyy-MM-ddTHH:mm:ss'), $m) }

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public class HK {
    [DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr h, int id, uint mods, uint vk);
    [DllImport("user32.dll")] public static extern bool UnregisterHotKey(IntPtr h, int id);
    [DllImport("user32.dll")] public static extern int GetMessageW(out MSG m, IntPtr h, uint a, uint b);
    [StructLayout(LayoutKind.Sequential)] public struct MSG {
        public IntPtr hwnd; public uint message; public IntPtr wParam; public IntPtr lParam;
        public uint time; public int ptX; public int ptY;
    }
}
"@

# MOD_ALT=0x1, MOD_NOREPEAT=0x4000, VK_Z=0x5A
if (-not [HK]::RegisterHotKey([IntPtr]::Zero, 0xB00B, 0x4001, 0x5A)) {
    Log 'Alt+Z register FAILED (owned by another host?) - exiting'
    exit 1
}
Log 'Alt+Z registered (standalone wire-through)'

$msg = New-Object HK+MSG
while ([HK]::GetMessageW([ref]$msg, [IntPtr]::Zero, 0, 0) -gt 0) {
    if ($msg.message -eq 0x0312) {
        try {
            Invoke-WebRequest -UseBasicParsing -Method POST `
                -Uri 'http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/Toggle' `
                -TimeoutSec 3 | Out-Null
            Log 'Alt+Z -> toggle OK'
        } catch {
            Log ('Alt+Z -> toggle FAILED: ' + $_.Exception.Message)
        }
    }
}
