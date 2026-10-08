// PluginHost.cs — โหลด/ปล่อย plugin DLL จากโฟลเดอร์ plugins\ (ฝั่งเราเปิดให้โหลด)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;

namespace NvPlugins
{
    public class Host : INvHost
    {
        public Action<string> Sink = _ => { };
        public void Log(string msg) => Sink("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg);
        public string ConfigDir() => @"C:\My Project\NVIDIA-Shadowplay\build\NvConfig";
        public string PayloadDir() => P.PAYLOAD;
    }

    public class LoadedPlugin
    {
        public string File = "";
        public string Name = "";
        public INvPlugin Instance;
        public AssemblyLoadContext Ctx;   // null = built-in
        public bool Running;
    }

    public class PluginHost
    {
        public Host Host = new();
        public List<LoadedPlugin> Loaded = new();
        public string PluginsDir;

        public PluginHost(string pluginsDir)
        {
            PluginsDir = pluginsDir;
            try { Directory.CreateDirectory(pluginsDir); } catch { }
        }

        // --- built-in plugins (โค้ดในตัว — ผ่าน contract เดียวกับข้างนอก) ---
        public void RegisterBuiltIn(INvPlugin p, string name)
        {
            var lp = new LoadedPlugin { File = "(built-in)", Name = name, Instance = p, Running = false };
            p.Init(Host);
            Loaded.Add(lp);
        }

        // --- โหลดจากโฟลเดอร์ plugins\*.dll ---
        public List<string> LoadFromDir()
        {
            var errors = new List<string>();
            if (!Directory.Exists(PluginsDir)) return errors;
            foreach (var dll in Directory.GetFiles(PluginsDir, "*.dll"))
            {
                if (Loaded.Any(l => string.Equals(l.File, dll, StringComparison.OrdinalIgnoreCase))) continue;
                try { LoadFile(dll); }
                catch (Exception ex) { errors.Add(Path.GetFileName(dll) + " → " + ex.Message); }
            }
            return errors;
        }

        public LoadedPlugin LoadFile(string dll)
        {
            var ctx = new AssemblyLoadContext(Path.GetFileNameWithoutExtension(dll), isCollectible: true);
            var asm = ctx.LoadFromAssemblyPath(dll);
            INvPlugin found = null;
            foreach (var t in asm.GetTypes())
            {
                if (typeof(INvPlugin).IsAssignableFrom(t) && !t.IsAbstract)
                {
                    found = (INvPlugin)Activator.CreateInstance(t);
                    if (found != null) break;
                }
            }
            if (found == null) { ctx.Unload(); throw new Exception("ไม่มี class ที่ implement INvPlugin"); }
            found.Init(Host);
            var lp = new LoadedPlugin { File = dll, Name = found.GetInfo(), Instance = found, Ctx = ctx };
            Loaded.Add(lp);
            return lp;
        }

        public void Start(LoadedPlugin lp)
        {
            if (!lp.Running) { lp.Instance.Start(); lp.Running = true; Host.Log("plugin start: " + lp.Name); }
        }

        public void Stop(LoadedPlugin lp)
        {
            if (lp.Running) { try { lp.Instance.Stop(); } catch { } lp.Running = false; Host.Log("plugin stop: " + lp.Name); }
        }

        public void Unload(LoadedPlugin lp)
        {
            Stop(lp);
            Loaded.Remove(lp);
            if (lp.Ctx != null)
            {
                lp.Ctx.Unload();
                Host.Log("plugin unloaded: " + lp.Name);
            }
        }

        public void StartAllRunningDefaults()
        {
            foreach (var lp in Loaded) Start(lp);
        }
    }
}
