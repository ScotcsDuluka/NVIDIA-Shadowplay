; NVIDIA ShadowPlay — first-party-style installer (Inno Setup 6.7+)
;
; Visual design: dark premium wizard, NVIDIA-class green accent, brand panel
; on the left (welcome/finished), hand-laid controls on every page. All
; wizard chrome is restyled in [Code]; no external UI plugins.
;
; Flow: Welcome -> Installation location -> Options -> Installing -> Finished
; (ready page and program-group page disabled).
;
; Version is read from the actual build output (Overlay\NVIDIA ShadowPlay.exe).

#define AppName "NVIDIA ShadowPlay"
#define AppVersion GetVersionNumbersString("..\dist\NVIDIA ShadowPlay\Overlay\NVIDIA ShadowPlay.exe")
#define AppPublisher "ScotcsDuluka"
#define SourceRoot "..\dist\NVIDIA ShadowPlay"
#define OutputDir "..\dist-installer"
#define Assets "assets"

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
DisableReadyPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
WizardSizePercent=100
DisableWelcomePage=no
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
Name: "desktopicon"; Description: "Create a desktop shortcut"; \
    GroupDescription: "Shortcuts:"; Flags: unchecked

[Dirs]
; runtime-writable folders the app writes into while installed under
; Program Files (standard users must be able to record / log / configure)
Name: "{app}\Config"; Permissions: users-modify
Name: "{app}\Data"; Permissions: users-modify
Name: "{app}\Data\NVIDIA_Shadowplay_Data"; Permissions: users-modify
Name: "{app}\Logs"; Permissions: users-modify
Name: "{app}\Flags"; Permissions: users-modify

[Files]
Source: "{#SourceRoot}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; wizard artwork — extracted to {tmp} at runtime and painted by [Code]
Source: "assets\welcome-bg.bmp"; Flags: dontcopy
Source: "assets\NVIDIASans_Rg.ttf"; Flags: dontcopy
Source: "assets\NVIDIASans_Md.ttf"; Flags: dontcopy
Source: "assets\NVIDIASans_Bd.ttf"; Flags: dontcopy

[Icons]
Name: "{autoprograms}\{#AppName}\{#AppName}"; Filename: "{app}\Launcher.exe"; WorkingDir: "{app}"; IconFilename: "{app}\Overlay\NVIDIA ShadowPlay.ico"
Name: "{autoprograms}\{#AppName}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\Launcher.exe"; WorkingDir: "{app}"; IconFilename: "{app}\Overlay\NVIDIA ShadowPlay.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\Launcher.exe"; Description: "Launch NVIDIA ShadowPlay"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; session diagnostics only — Config\ (settings) and Data\ (recordings,
; replay state) are deliberately preserved on uninstall
Type: filesandordirs; Name: "{app}\Logs"
Type: filesandordirs; Name: "{app}\Flags"

[Code]
// ─────────────────────────────────────────────────────────────────────────
// Dark premium wizard — palette (TColor = $00BBGGRR). Colors and fonts are
// the REAL NVIDIA App installer values: #333333 surface (Main_BG.png),
// #76B900 accent + #898989 secondary (theme.cfg), NVIDIA Sans (NVI2 fonts).
// ─────────────────────────────────────────────────────────────────────────

function AddFontResource(lpszFilename: String): Integer;
external 'AddFontResourceW@gdi32.dll stdcall';

const
  ClBackDark  = $00333333;   // #333333 — real Main_BG.png surface
  ClInputDark = $00242424;   // input fields on #333333
  ClTextMain  = $00FFFFFF;   // #FFFFFF (SideBarCurrentTextColor)
  ClTextDim   = $00898989;   // #898989 (SideBarNotDoneTextColor)
  ClAccent    = $0000B976;   // #76B900 (SideBarDoneTextColor)

var
  BgWelcome, BgFinished, BgFinishedTop: TBitmapImage;
  HeadLocation, HeadOptions: TNewStaticText;
  SubLocation, SubOptions: TNewStaticText;

procedure StyleLabel(C: TNewStaticText; AColor: TColor);
begin
  if C <> nil then
  begin
    C.Font.Color := AColor;
    C.Color := ClBackDark;
  end;
end;

// Load a bitmap from {tmp} (extracted at setup start) and pin it as a
// full-page background.
procedure PinBackground(Parent: TNewNotebookPage; FileName: String; var Store: TBitmapImage);
begin
  Store := TBitmapImage.Create(Parent);
  Store.Bitmap.LoadFromFile(ExpandConstant('{tmp}\') + FileName);
  Store.Stretch := True;
  Store.Parent := Parent;
  Store.SetBounds(0, 0, Parent.Width, Parent.Height);
  Store.SendToBack;
end;

procedure InitializeWizard;
var
  PageDark: TColor;
begin
  ExtractTemporaryFile('welcome-bg.bmp');
  ExtractTemporaryFile('NVIDIASans_Rg.ttf');
  ExtractTemporaryFile('NVIDIASans_Md.ttf');
  ExtractTemporaryFile('NVIDIASans_Bd.ttf');
  AddFontResource(ExpandConstant('{tmp}\\NVIDIASans_Rg.ttf'));
  AddFontResource(ExpandConstant('{tmp}\\NVIDIASans_Md.ttf'));
  AddFontResource(ExpandConstant('{tmp}\\NVIDIASans_Bd.ttf'));
  PageDark := ClBackDark;

  // ── window: large fixed-size dark canvas ──────────────────────────────
  WizardForm.InnerPage.Color := PageDark;
  WizardForm.WelcomePage.Color := PageDark;
  WizardForm.InstallingPage.Color := PageDark;
  WizardForm.FinishedPage.Color := PageDark;
  WizardForm.ReadyPage.Color := PageDark;
  WizardForm.Color := PageDark;
  WizardForm.MainPanel.Visible := False;         // chrome: our pages carry the design
  WizardForm.Bevel1.Visible := False;
  WizardForm.Bevel.Visible := False;
  WizardForm.WizardBitmapImage.Hide;             // default welcome artwork off-brand
  WizardForm.Caption := 'NVIDIA ShadowPlay Setup';
  WizardForm.ClientWidth := ScaleX(790);
  WizardForm.ClientHeight := ScaleY(530);
  WizardForm.Position := poScreenCenter;

  // ── backgrounds ───────────────────────────────────────────────────────
  PinBackground(WizardForm.WelcomePage, 'welcome-bg.bmp', BgWelcome);
  PinBackground(WizardForm.FinishedPage, 'welcome-bg.bmp', BgFinished);

  // Inno re-shows its box artwork on the Finished page above our background;
  // an identical copy painted ON TOP wins z-order — labels are brought back
  // above it in CurPageChanged.
  BgFinishedTop := TBitmapImage.Create(WizardForm.FinishedPage);
  BgFinishedTop.Bitmap.LoadFromFile(ExpandConstant('{tmp}\welcome-bg.bmp'));
  BgFinishedTop.Stretch := True;
  BgFinishedTop.Parent := WizardForm.FinishedPage;
  BgFinishedTop.SetBounds(0, 0, WizardForm.FinishedPage.Width, WizardForm.FinishedPage.Height);

  // ── inner-page headers (location / options share InnerPage) ──────────
  HeadLocation := TNewStaticText.Create(WizardForm);
  HeadLocation.Parent := WizardForm.InnerPage;
  HeadLocation.Caption := 'Where should NVIDIA ShadowPlay be installed?';
  HeadLocation.AutoSize := False;
  HeadLocation.SetBounds(ScaleX(44), ScaleY(24), ScaleX(700), ScaleY(30));
  HeadLocation.Font.Height := -20; HeadLocation.Font.Name := 'NVIDIA Sans';
  HeadLocation.Font.Style := [fsBold]; HeadLocation.Font.Color := ClTextMain;
  HeadLocation.AutoSize := False;
  HeadLocation.Visible := False;

  SubLocation := TNewStaticText.Create(WizardForm);
  SubLocation.Parent := WizardForm.InnerPage;
  SubLocation.Caption := 'The default location is recommended. Runtime folders (settings, recordings, logs) stay inside it and remain writable.';
  SubLocation.AutoSize := False;
  SubLocation.SetBounds(ScaleX(44), ScaleY(60), ScaleX(700), ScaleY(34));
  SubLocation.Font.Height := -15; SubLocation.Font.Name := 'NVIDIA Sans';
  SubLocation.Font.Color := ClTextDim;
  SubLocation.WordWrap := True;
  SubLocation.AutoSize := False;
  SubLocation.Visible := False;

  HeadOptions := TNewStaticText.Create(WizardForm);
  HeadOptions.Parent := WizardForm.InnerPage;
  HeadOptions.Caption := 'Installation options';
  HeadOptions.AutoSize := False;
  HeadOptions.SetBounds(ScaleX(44), ScaleY(24), ScaleX(700), ScaleY(30));
  HeadOptions.Font.Height := -20; HeadOptions.Font.Name := 'NVIDIA Sans';
  HeadOptions.Font.Style := [fsBold]; HeadOptions.Font.Color := ClTextMain;
  HeadOptions.AutoSize := False;
  HeadOptions.Visible := False;

  SubOptions := TNewStaticText.Create(WizardForm);
  SubOptions.Parent := WizardForm.InnerPage;
  SubOptions.Caption := 'Everything is optional and can be changed later.';
  SubOptions.AutoSize := False;
  SubOptions.SetBounds(ScaleX(44), ScaleY(60), ScaleX(700), ScaleY(24));
  SubOptions.Font.Height := -15; SubOptions.Font.Name := 'NVIDIA Sans';
  SubOptions.Font.Color := ClTextDim;
  SubOptions.AutoSize := False;
  SubOptions.Visible := False;

  // ── welcome page copy ────────────────────────────────────────────────
  WizardForm.WelcomeLabel1.Hide;
  WizardForm.WelcomeLabel2.AutoSize := False;
  WizardForm.WelcomeLabel2.SetBounds(ScaleX(44), ScaleY(150), ScaleX(700), ScaleY(220));
  WizardForm.WelcomeLabel2.Font.Height := -17; WizardForm.WelcomeLabel2.Font.Name := 'NVIDIA Sans';
  WizardForm.WelcomeLabel2.Font.Color := ClTextMain;
  WizardForm.WelcomeLabel2.Caption :=
    'A fast, hardware-accelerated screen recorder.' + #13#10#13#10 +
    'Instant replay, manual recording and screenshots with NVIDIA NVENC ' +
    'or Intel QSV encoding - packaged with the complete runtime, capture ' +
    'engines, overlay and launcher.' + #13#10#13#10 +
    'Click Install to continue.';

  // ── location page ────────────────────────────────────────────────────
  WizardForm.SelectDirBitmapImage.Hide;          // default folder artwork off-brand
  WizardForm.DirEdit.SetBounds(ScaleX(44), ScaleY(150), ScaleX(620), ScaleY(30));
  WizardForm.DirEdit.Color := ClInputDark;
  WizardForm.DirEdit.Font.Color := ClTextMain;
  WizardForm.DirEdit.Font.Name := 'NVIDIA Sans';
  WizardForm.DirBrowseButton.SetBounds(ScaleX(676), ScaleY(149), ScaleX(70), ScaleY(31));

  // ── options page (task list) ─────────────────────────────────────────
  WizardForm.TasksList.SetBounds(ScaleX(44), ScaleY(150), ScaleX(700), ScaleY(120));
  WizardForm.TasksList.Color := PageDark;
  WizardForm.TasksList.Font.Color := ClTextMain;
  WizardForm.TasksList.Font.Name := 'NVIDIA Sans';
  WizardForm.TasksList.Font.Height := -16;

  // ── installing page ──────────────────────────────────────────────────
  WizardForm.StatusLabel.SetBounds(ScaleX(44), ScaleY(238), ScaleX(700), ScaleY(20));
  WizardForm.StatusLabel.Font.Height := -16; WizardForm.StatusLabel.Font.Name := 'NVIDIA Sans';
  WizardForm.StatusLabel.Font.Color := ClTextMain;
  WizardForm.FilenameLabel.SetBounds(ScaleX(44), ScaleY(262), ScaleX(700), ScaleY(18));
  WizardForm.FilenameLabel.Font.Height := -13; WizardForm.FilenameLabel.Font.Name := 'NVIDIA Sans';
  WizardForm.FilenameLabel.Font.Color := ClTextDim;
  WizardForm.ProgressGauge.SetBounds(ScaleX(44), ScaleY(292), ScaleX(700), ScaleY(26));
  try
    // themed progress bars may ignore the classic color messages; harmless
    SendMessage(WizardForm.ProgressGauge.Handle, $0409, 0, ClAccent);  // PBM_SETBARCOLOR
  except
  end;

  // ── finished page copy ───────────────────────────────────────────────
  WizardForm.FinishedLabel.AutoSize := False;
  WizardForm.FinishedLabel.SetBounds(ScaleX(44), ScaleY(150), ScaleX(700), ScaleY(160));
  WizardForm.FinishedLabel.Font.Height := -17; WizardForm.FinishedLabel.Font.Name := 'NVIDIA Sans';
  WizardForm.FinishedLabel.Font.Color := ClTextMain;
  WizardForm.FinishedLabel.Caption :=
    'NVIDIA ShadowPlay has been installed.' + #13#10#13#10 +
    'Shortcuts are in the Start Menu' + #13#10 +
    'and on the desktop if you selected one.';
  WizardForm.RunList.SetBounds(ScaleX(44), ScaleY(330), ScaleX(700), ScaleY(40));
  WizardForm.RunList.Color := PageDark;
  WizardForm.RunList.Font.Color := ClTextMain;
  WizardForm.RunList.Font.Name := 'NVIDIA Sans';
  WizardForm.RunList.Font.Height := -15;

  // ── bottom buttons (standard order, right-aligned) ───────────────────
  WizardForm.CancelButton.SetBounds(ScaleX(790) - ScaleX(75) - ScaleX(10), ScaleY(530) - ScaleY(38), ScaleX(75), ScaleY(29));
  WizardForm.NextButton.SetBounds(ScaleX(790) - ScaleX(75) * 2 - ScaleX(20), ScaleY(530) - ScaleY(38), ScaleX(75), ScaleY(29));
  WizardForm.BackButton.SetBounds(ScaleX(790) - ScaleX(75) * 3 - ScaleX(30), ScaleY(530) - ScaleY(38), ScaleX(75), ScaleY(29));
  WizardForm.BackButton.Font.Name := 'NVIDIA Sans';
  WizardForm.NextButton.Font.Name := 'NVIDIA Sans';
  WizardForm.NextButton.Font.Style := [fsBold];
  WizardForm.CancelButton.Font.Name := 'NVIDIA Sans';
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  // Inno re-shows its own wizard artwork when entering Welcome/Finished and
  // rewrites the finished-page copy — re-apply our design on every entry.
  if (CurPageID = wpWelcome) or (CurPageID = wpFinished) then
    WizardForm.WizardBitmapImage.Visible := False;

  // headers swap on the shared InnerPage
  HeadLocation.Visible := CurPageID = wpSelectDir;
  SubLocation.Visible := CurPageID = wpSelectDir;
  HeadOptions.Visible := CurPageID = wpSelectTasks;
  SubOptions.Visible := CurPageID = wpSelectTasks;
  if CurPageID = wpWelcome then
    WizardForm.NextButton.Caption := '&Install'
  else
    WizardForm.NextButton.Caption := '&Next >';

  if CurPageID = wpFinished then
  begin
    BgFinishedTop.BringToFront;
    WizardForm.FinishedHeadingLabel.BringToFront;
    WizardForm.FinishedLabel.BringToFront;
    WizardForm.RunList.BringToFront;
    WizardForm.FinishedHeadingLabel.SetBounds(ScaleX(44), ScaleY(140), ScaleX(700), ScaleY(40));
    WizardForm.FinishedHeadingLabel.Font.Height := -24;
    WizardForm.FinishedHeadingLabel.Font.Name := 'NVIDIA Sans';
    WizardForm.FinishedHeadingLabel.Font.Style := [fsBold];
    WizardForm.FinishedHeadingLabel.Font.Color := ClTextMain;
    WizardForm.FinishedHeadingLabel.Caption := 'Installation complete';
    WizardForm.FinishedLabel.SetBounds(ScaleX(44), ScaleY(196), ScaleX(700), ScaleY(120));
    WizardForm.FinishedLabel.Font.Height := -17;
    WizardForm.FinishedLabel.Font.Name := 'NVIDIA Sans';
    WizardForm.FinishedLabel.Font.Color := ClTextMain;
    WizardForm.FinishedLabel.Caption :=
      'NVIDIA ShadowPlay has been installed.' + #13#10#13#10 +
      'Shortcuts are in the Start Menu' + #13#10 +
      'and on the desktop if you selected one.';
    WizardForm.RunList.SetBounds(ScaleX(44), ScaleY(320), ScaleX(700), ScaleY(40));
    WizardForm.RunList.Color := ClBackDark;
    WizardForm.RunList.Font.Color := ClTextMain;
    WizardForm.RunList.Font.Name := 'NVIDIA Sans';
    WizardForm.RunList.Font.Height := -15;
  end;
end;
