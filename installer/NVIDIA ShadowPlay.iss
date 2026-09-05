#define AppName "NVIDIA ShadowPlay"
#define AppVersion "3.41.3588.61"
#define AppPublisher "ScotcsDuluka"
#define SourceRoot "..\dist\NVIDIA ShadowPlay"
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
WizardSizePercent=115
SetupIconFile={#SourceRoot}\Overlay\NVIDIA ShadowPlay.ico
UninstallDisplayIcon={app}\Overlay\NVIDIA ShadowPlay.ico
OutputDir={#OutputDir}
OutputBaseFilename=NVIDIA-ShadowPlay-Setup-v{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
LZMAUseSeparateProcess=yes
WizardImageStretch=no
VersionInfoVersion={#AppVersion}
VersionInfoProductVersion={#AppVersion}
VersionInfoProductName={#AppName}
VersionInfoCompany={#AppPublisher}
VersionInfoDescription={#AppName} installer

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "thai"; MessagesFile: "compiler:Languages\Thai.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"; Flags: unchecked

[Dirs]
Name: "{app}\Config"; Permissions: users-modify
Name: "{app}\Data"; Permissions: users-modify
Name: "{app}\Data\NVIDIA_Shadowplay_Data"; Permissions: users-modify
Name: "{app}\Logs"; Permissions: users-modify
Name: "{app}\Flags"; Permissions: users-modify

[Files]
Source: "{#SourceRoot}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}\{#AppName}"; Filename: "{app}\Launcher.exe"; WorkingDir: "{app}"; IconFilename: "{app}\Overlay\NVIDIA ShadowPlay.ico"
Name: "{autoprograms}\{#AppName}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\Launcher.exe"; WorkingDir: "{app}"; IconFilename: "{app}\Overlay\NVIDIA ShadowPlay.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\Launcher.exe"; Description: "Launch {#AppName}"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent unchecked

[UninstallDelete]
Type: filesandordirs; Name: "{app}\Logs"
Type: filesandordirs; Name: "{app}\Flags"

[Code]
procedure InitializeWizard;
begin
  WizardForm.WelcomeLabel2.Caption :=
    'A fast, modern screen recorder for NVIDIA and Intel hardware.' + #13#10#13#10 +
    'This installer packages the validated production build with FFmpeg, ' +
    'capture engines, audio components, overlay and launcher.';
  WizardForm.FinishedLabel.Caption :=
    'NVIDIA ShadowPlay is ready.' + #13#10#13#10 +
    'Click Finish to launch the app.';
end;
