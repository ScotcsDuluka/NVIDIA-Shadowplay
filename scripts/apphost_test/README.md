# apphost_test — PROOF of the Application\ -> ..\Services\ host mechanism

P13-era layout work (2026-08-28). This tiny project verified, on the Linux
build machine, the three facts the root-fixed layout depends on:

1. The MSBuild `CreateAppHost` task (SDK 8.0.424, Microsoft.NET.Build.Tasks)
   accepts `AppBinaryName="..\Services\hosttest.dll"` and embeds the
   parent-relative path VERBATIM into the host binary.
2. The pristine template must come from the restored host pack
   (`~/.dotnet/packs/Microsoft.NETCore.App.Host.<rid>/<ver>/runtimes/<rid>/native/apphost`).
   Re-writing an ALREADY-BUILT host fails with NETSDK1029 — the placeholder
   byte sequence is consumed by the first stamping.
3. At runtime hostfxr resolves the embedded relative path against the
   HOST's own directory and normalizes `..` — the test run printed:

       The application to execute does not exist:
       '<hostdir>/../Services/hosttest.dll'

   i.e. with the real tree: Application\..\Services\NVIDIA API.dll =
   Services\NVIDIA API.dll. QED.

The task also copies win32 resources (the ICON) from the managed dll into
the host (`assemblyToCopyResourcesFrom`), so the Application\ hosts keep
the ShadowPlay icon.

Files: hosttest.csproj (minimal app), rewrite.targets (the two CreateAppHost
invocations used in the experiment).
