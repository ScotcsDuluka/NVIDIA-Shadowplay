// Injector.cs — ยิง/ดึง DLL เข้า-ออก process (kernel32 classic technique)
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace NvPlugins
{
    public static class Injector
    {
        const uint PROCESS_CREATE_THREAD = 0x0002, PROCESS_QUERY_INFORMATION = 0x0400,
                   PROCESS_VM_OPERATION = 0x0008, PROCESS_VM_WRITE = 0x0020, PROCESS_VM_READ = 0x0010;
        const uint MEM_COMMIT = 0x1000, MEM_RESERVE = 0x2000, MEM_RELEASE = 0x8000;
        const uint PAGE_READWRITE = 0x04;

        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr OpenProcess(uint access, bool inherit, int pid);
        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr h, IntPtr addr, uint size, uint type, uint protect);
        [DllImport("kernel32", SetLastError = true)]
        static extern bool VirtualFreeEx(IntPtr h, IntPtr addr, uint size, uint type);
        [DllImport("kernel32", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr h, IntPtr addr, byte[] buf, uint size, out IntPtr written);
        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr h, IntPtr attr, uint stack, IntPtr start, IntPtr param, uint flags, out IntPtr tid);
        [DllImport("kernel32", SetLastError = true)]
        static extern uint WaitForSingleObject(IntPtr h, uint ms);
        [DllImport("kernel32", SetLastError = true)]
        static extern bool GetExitCodeThread(IntPtr h, out uint code);
        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr h, string name);
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr GetModuleHandleA(string name);
        [DllImport("kernel32", SetLastError = true)]
        static extern bool CloseHandle(IntPtr h);
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint pid);
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool Module32FirstW(IntPtr snap, ref MODULEENTRY32W me);
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool Module32NextW(IntPtr snap, ref MODULEENTRY32W me);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct MODULEENTRY32W
        {
            public uint dwSize, th32ModuleID, th32ProcessID, GlblcntUsage, ProccntUsage;
            public IntPtr modBaseAddr;
            public uint modBaseSize;
            public IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szExePath;
        }

        public static void Inject(int pid, string dllPath)
        {
            var hProc = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, pid);
            if (hProc == IntPtr.Zero) throw new Exception("OpenProcess ล้ม (ลองรัน NvPlugins เป็น admin) err=" + Marshal.GetLastWin32Error());
            try
            {
                var bytes = Encoding.Unicode.GetBytes(dllPath);
                var remote = VirtualAllocEx(hProc, IntPtr.Zero, (uint)(bytes.Length + 2), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                if (remote == IntPtr.Zero) throw new Exception("VirtualAllocEx ล้ม");
                if (!WriteProcessMemory(hProc, remote, bytes, (uint)bytes.Length, out _)) throw new Exception("WriteProcessMemory ล้ม");
                var loadLib = GetProcAddress(GetModuleHandleA("kernel32.dll"), "LoadLibraryW");
                if (loadLib == IntPtr.Zero) throw new Exception("หา LoadLibraryW ไม่เจอ");
                var hThread = CreateRemoteThread(hProc, IntPtr.Zero, 0, loadLib, remote, 0, out _);
                if (hThread == IntPtr.Zero) throw new Exception("CreateRemoteThread ล้ม err=" + Marshal.GetLastWin32Error());
                WaitForSingleObject(hThread, 8000);
                GetExitCodeThread(hThread, out var code);
                CloseHandle(hThread);
                if (code == 0) throw new Exception("LoadLibraryW ใน process คืน 0 (dll โหลดไม่สำเร็จ — เช็ค 32/64-bit ตรงกันมั้ย)");
            }
            finally { CloseHandle(hProc); }
        }

        public static bool Eject(int pid, string moduleName)
        {
            // 1) หา base address ของ module ใน process
            IntPtr baseAddr = IntPtr.Zero;
            var snap = CreateToolhelp32Snapshot(0x8 /*TH32CS_SNAPMODULE*/, (uint)pid);
            if (snap == IntPtr.Zero) throw new Exception("CreateToolhelp32Snapshot ล้ม (ต้องสิทธิ์เดียวกับ process)");
            try
            {
                var me = new MODULEENTRY32W { dwSize = (uint)Marshal.SizeOf<MODULEENTRY32W>() };
                for (bool ok = Module32FirstW(snap, ref me); ok; ok = Module32NextW(snap, ref me))
                    if (string.Equals(me.szModule, moduleName, StringComparison.OrdinalIgnoreCase)) { baseAddr = me.modBaseAddr; break; }
            }
            finally { CloseHandle(snap); }
            if (baseAddr == IntPtr.Zero) return false;

            // 2) FreeLibrary จาก remote thread
            var hProc = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION, false, pid);
            if (hProc == IntPtr.Zero) throw new Exception("OpenProcess ล้ม");
            try
            {
                var freeLib = GetProcAddress(GetModuleHandleA("kernel32.dll"), "FreeLibrary");
                var hThread = CreateRemoteThread(hProc, IntPtr.Zero, 0, freeLib, baseAddr, 0, out _);
                if (hThread == IntPtr.Zero) throw new Exception("CreateRemoteThread (FreeLibrary) ล้ม");
                WaitForSingleObject(hThread, 8000);
                CloseHandle(hThread);
                return true;
            }
            finally { CloseHandle(hProc); }
        }
    }
}
