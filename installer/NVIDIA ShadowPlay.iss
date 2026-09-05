; NVIDIA ShadowPlay — installer (Inno Setup 6.7+)
;
; Standard Inno Setup wizard: Welcome -> License -> Location -> Shortcuts ->
; Ready -> Installing -> Finish.
;
; Migrated from the legacy InstallForge setup (NVSetup\config.ifp):
;   - payload: the production product tree
;     Overlay\bin\Release\net10.0-windows10.0.26100.0 (Launcher.exe,
;     .NET Deployment\, Application\, Services\, Overlay\, FFmpeg\, Redist\...)
;   - version: read from the built Overlay\NVIDIA ShadowPlay.exe — that exe
;     is the version authority, no separate one is maintained
;   - prerequisite: the payload is framework-dependent, so the bundled
;     .NET Desktop Runtime 10 installer (Redist\64bit.runtime.exe) runs
;     silently during install — legacy behavior, still required
;   - desktop shortcut "NVIDIA Launcher" -> Launcher.exe (legacy naming)
;   - license: repo-root LICENSE is shown on the license page; LICENSE and
;     LICENSE.NOTICE ship to {app} so the installed product carries the terms

#define AppName "NVIDIA ShadowPlay"
#define SourceRoot "..\Overlay\bin\Release\net10.0-windows10.0.26100.0"
#define AppVersion GetVersionNumbersString(SourceRoot + "\Overlay\NVIDIA ShadowPlay.exe")
#define AppPublisher "ScotcsDuluka"
#define OutputDir "..\dist-installer"

[Setup]
AppId={{B7E48D4A-7F83-4E3A-9F38-9D9E2A6D8F41}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://github.com/ScotcsDuluka/NVIDIA-Shadowplay
AppSupportURL=https://github.com/ScotcsDuluka/NVIDIA-Shadowplay
DefaultDirName={autopf}\NVIDIA ShadowPlay
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
DisableWelcomePage=no
LicenseFile=..\LICENSE
SetupIconFile={#SourceRoot}\Overlay\NVIDIA ShadowPlay.ico
UninstallDisplayIcon={app}\Overlay\NVIDIA ShadowPlay.ico
UninstallDisplayName={#AppName}
OutputDir={#OutputDir}
OutputBaseFilename=NVIDIA-ShadowPlay-Setup-v{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
LZMAUseSeparateProcess=yes
VersionInfoVersion={#AppVersion}
VersionInfoProductVersion={#AppVersion}
VersionInfoProductName={#AppName}
VersionInfoCompany={#AppPublisher}
VersionInfoDescription={#AppName} installer

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
; legacy InstallForge always created a desktop shortcut named "NVIDIA Launcher"
Name: "desktopicon"; Description: "Create a desktop shortcut (NVIDIA Launcher)"; \
    GroupDescription: "Shortcuts:"

[Dirs]
; runtime-writable folders the app writes into while installed under
; Program Files (standard users must be able to record / log / configure)
Name: "{app}\Config"; Permissions: users-modify
Name: "{app}\Data"; Permissions: users-modify
Name: "{app}\Data\NVIDIA_Shadowplay_Data"; Permissions: users-modify
Name: "{app}\Logs"; Permissions: users-modify
Name: "{app}\Flags"; Permissions: users-modify

[Files]
; product tree — EXCLUDES the four runtime-writable dirs' state (Config\ app
; settings, Logs\, Flags\ sentinels, Data\NVIDIA_Shadowplay_Data recordings):
; those hold dev-machine runtime state (e.g. engine.json carries an absolute
; dev FFmpegPath) and must never ship; [Dirs] recreates them with
; users-modify rights. Shipped Config defaults are re-added below.
Source: "{#SourceRoot}\*"; DestDir: "{app}"; Excludes: "Config\*,Logs\*,Flags\*,Data\NVIDIA_Shadowplay_Data\*"; Flags: ignoreversion recursesubdirs createallsubdirs
; shipped Config defaults (tracked sources, copied into the tree by the build)
Source: "{#SourceRoot}\Config\notifier_obs.json"; DestDir: "{app}\Config"; Flags: ignoreversion
; license terms — canonical sources are the repo-root files (the legacy
; installer embedded the same MIT text + third-party notices in its license
; dialog); LICENSE is also shown on the license page
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE.NOTICE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}\{#AppName}"; Filename: "{app}\Launcher.exe"; WorkingDir: "{app}"; IconFilename: "{app}\Overlay\NVIDIA ShadowPlay.ico"
Name: "{autoprograms}\{#AppName}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\NVIDIA Launcher"; Filename: "{app}\Launcher.exe"; WorkingDir: "{app}"; IconFilename: "{app}\Overlay\NVIDIA ShadowPlay.ico"; Tasks: desktopicon

[Run]
; legacy prerequisite: the payload is framework-dependent (".NET Deployment\"
; carries only deps/runtimeconfig json) — .NET Desktop Runtime 10 must exist.
; The bundled installer is idempotent; machines with the runtime already
; present finish this step quickly without changing it.
Filename: "{app}\Redist\64bit.runtime.exe"; Parameters: "/install /quiet /norestart"; Flags: waituntilterminated runhidden
Filename: "{app}\Launcher.exe"; Description: "Launch NVIDIA ShadowPlay"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; session diagnostics only — Config\ (settings) and Data\ (recordings,
; replay state) are deliberately preserved on uninstall
Type: filesandordirs; Name: "{app}\Logs"
Type: filesandordirs; Name: "{app}\Flags"
