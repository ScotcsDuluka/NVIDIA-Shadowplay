# inject.ps1 — inject a native DLL into a CONSENTED game process.
# Usage: powershell -ExecutionPolicy Bypass -File inject.ps1 -Exe Dungeons.exe -Dll C:\path\NvidiaShareHook.dll
param(
    [Parameter(Mandatory=$true)][string]$Exe,
    [Parameter(Mandatory=$true)][string]$Dll
)

Add-Type @'
using System;
using System.Runtime.InteropServices;
public class Injector {
    [DllImport("kernel32.dll", SetLastError=true)]
    public static extern IntPtr OpenProcess(uint access, bool inherit, uint pid);
    [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Unicode)]
    public static extern IntPtr GetModuleHandleW(string name);
    [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Ansi)]
    public static extern IntPtr GetProcAddress(IntPtr mod, string name);
    [DllImport("kernel32.dll", SetLastError=true)]
    public static extern IntPtr VirtualAllocEx(IntPtr proc, IntPtr addr, UIntPtr size, uint type, uint protect);
    [DllImport("kernel32.dll", SetLastError=true)]
    public static extern bool WriteProcessMemory(IntPtr proc, IntPtr addr, byte[] buf, UIntPtr size, IntPtr written);
    [DllImport("kernel32.dll", SetLastError=true)]
    public static extern IntPtr CreateRemoteThread(IntPtr proc, IntPtr attr, UIntPtr size,
        IntPtr start, IntPtr param, uint flags, IntPtr tid);

    public static int Inject(uint pid, string dllPath) {
        IntPtr h = OpenProcess(0x1F0FFF, false, pid);
        if (h == IntPtr.Zero) return 1;
        IntPtr k32 = GetModuleHandleW("kernel32.dll");
        IntPtr loadLib = GetProcAddress(k32, "LoadLibraryW");
        if (loadLib == IntPtr.Zero) return 2;
        byte[] path = System.Text.Encoding.Unicode.GetBytes(dllPath + "\0");
        IntPtr addr = VirtualAllocEx(h, IntPtr.Zero, (UIntPtr)path.Length, 0x3000, 0x40);
        if (addr == IntPtr.Zero) return 3;
        if (!WriteProcessMemory(h, addr, path, (UIntPtr)path.Length, IntPtr.Zero)) return 4;
        IntPtr thread = CreateRemoteThread(h, IntPtr.Zero, UIntPtr.Zero, loadLib, addr, 0, IntPtr.Zero);
        if (thread == IntPtr.Zero) return 5;
        return 0; // injected
    }
}
'@

$proc = Get-Process -Name ($Exe -replace '\.exe$','') -ErrorAction Stop
$full = (Resolve-Path $Dll).Path
if (-not (Test-Path $full)) { Write-Host "DLL not found: $full"; exit 1 }

$rc = [Injector]::Inject([uint32]$proc.Id, $full)
switch ($rc) {
    0 { Write-Host "INJECTED into $($proc.Id) ($Exe)" }
    1 { Write-Host "FAIL OpenProcess (run as admin?)"; exit 1 }
    2 { Write-Host "FAIL GetProcAddress LoadLibraryW"; exit 1 }
    3 { Write-Host "FAIL VirtualAllocEx"; exit 1 }
    4 { Write-Host "FAIL WriteProcessMemory"; exit 1 }
    5 { Write-Host "FAIL CreateRemoteThread"; exit 1 }
}
