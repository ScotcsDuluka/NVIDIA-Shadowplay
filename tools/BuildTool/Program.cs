using System.Diagnostics;
using System.Text;
using System.Text.Json;

internal static class Program
{
    static int Main(string[] args)
    {
        try { Console.OutputEncoding = Encoding.UTF8; } catch { }

        // exe lives at <repo>\Build\Build.exe -> repo root = one level up
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, ".."));
        var script = Path.Combine(repoRoot, "Scripts", "build-dev.ps1");
        if (!File.Exists(script))
        {
            repoRoot = Directory.GetCurrentDirectory();
            script = Path.Combine(repoRoot, "Scripts", "build-dev.ps1");
        }
        if (!File.Exists(script))
        {
            Console.Error.WriteLine("Build.exe: cannot locate Scripts\\build-dev.ps1 (run from <repo>\\Build\\)");
            return 2;
        }

        var counterPath = Path.Combine(repoRoot, "Build", "Build-Config", "build.txt");
        var metaPath = Path.Combine(repoRoot, "Build", "Build-Config", "dev-build.json");

        var cmd = args.Length > 0 ? args[0].ToLowerInvariant() : "build";

        return cmd switch
        {
            "build" => CmdBuild(clean: false, script, repoRoot, counterPath),
            "clean" => CmdBuild(clean: true, script, repoRoot, counterPath),
            "version" => CmdVersion(repoRoot, counterPath, metaPath),
            "bump" => CmdBump(counterPath),
            "help" or "--help" or "-h" => Help(),
            var other => Unknown(other),
        };
    }

    static int CmdBuild(bool clean, string script, string repoRoot, string counterPath)
    {
        var buildNo = ReadCounter(counterPath) + 1;
        var commit = GitShort(repoRoot);
        var mode = clean ? "CLEAN" : "incremental";

        Console.WriteLine();
        Console.WriteLine($"==== NVIDIA ShadowPlay - Build #{buildNo} ({commit}) [{mode}] ====");
        Console.WriteLine();

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -ExecutionPolicy Bypass -File \"" + script + "\"" + (clean ? " -Clean" : ""),
            WorkingDirectory = repoRoot,
            UseShellExecute = false,
        };
        using var p = Process.Start(psi);
        p.WaitForExit();

        Console.WriteLine();
        if (p.ExitCode != 0)
        {
            Console.WriteLine($"X build FAILED (exit {p.ExitCode}) - counter stays at {ReadCounter(counterPath)}");
            return p.ExitCode;
        }

        WriteCounter(counterPath, buildNo);
        Console.WriteLine($"OK  Build #{buildNo} ({DateTime.Now:yyyy-MM-dd HH:mm})  commit {commit}");
        Console.WriteLine($"    output: {Path.Combine(repoRoot, "Build", "NVIDIA ShadowPlay")}");
        return 0;
    }

    static int CmdVersion(string repoRoot, string counterPath, string metaPath)
    {
        var last = "-";
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(metaPath));
            if (doc.RootElement.TryGetProperty("buildUtc", out var u))
                last = u.GetString() ?? "-";
        }
        catch { }

        Console.WriteLine("NVIDIA ShadowPlay");
        Console.WriteLine($"  build number : {ReadCounter(counterPath)}");
        Console.WriteLine($"  commit       : {GitShort(repoRoot)}");
        Console.WriteLine($"  last build   : {last}");
        return 0;
    }

    static int CmdBump(string counterPath)
    {
        var n = ReadCounter(counterPath) + 1;
        WriteCounter(counterPath, n);
        Console.WriteLine($"build number -> {n}");
        return 0;
    }

    static int Unknown(string cmd)
    {
        Console.WriteLine($"unknown command '{cmd}' - try: build | clean | version | bump | help");
        return 1;
    }

    static int Help()
    {
        Console.WriteLine("Build.exe - one-command build + version counter (Build\\Build-Config\\build.txt)");
        Console.WriteLine();
        Console.WriteLine("  Build.exe          run incremental build, counter +1 on success");
        Console.WriteLine("  Build.exe clean    run clean build (-Clean)");
        Console.WriteLine("  Build.exe version  show build number / commit / last build time");
        Console.WriteLine("  Build.exe bump     increment build counter without building");
        return 0;
    }

    static int ReadCounter(string path)
    {
        try
        {
            if (int.TryParse(File.ReadAllText(path).Trim(), out var n)) return n;
        }
        catch { }
        return 0;
    }

    static void WriteCounter(string path, int n)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, n.ToString());
    }

    static string GitShort(string repoRoot)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse --short HEAD",
                WorkingDirectory = repoRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            using var p = Process.Start(psi);
            var outp = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExit();
            return outp.Length > 0 ? outp : "unknown";
        }
        catch
        {
            return "unknown";
        }
    }
}
