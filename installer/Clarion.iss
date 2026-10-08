; Builds the Clarion setup program. Run scripts\build-installer.ps1 rather than calling this directly.
#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\artifacts\publish"
#endif

[Setup]
AppId={{6F0B2C1E-4D7A-4B9E-9C55-2E1A7C0D8F31}
AppName=Clarion
AppVersion={#AppVersion}
AppVerName=Clarion {#AppVersion}
AppPublisher=Kkthnx
AppPublisherURL=https://github.com/Kkthnx/Clarion
AppSupportURL=https://github.com/Kkthnx/Clarion/issues
AppUpdatesURL=https://github.com/Kkthnx/Clarion/releases
DefaultDirName={autopf}\Clarion
DefaultGroupName=Clarion
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\Clarion.exe
UninstallDisplayName=Clarion
SetupIconFile=..\src\Clarion.App\Assets\Clarion.ico
OutputDir=..\artifacts
OutputBaseFilename=Clarion-{#AppVersion}-setup
Compression=lzma2/max
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
PrivilegesRequired=admin
WizardStyle=modern
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Clarion"; Filename: "{app}\Clarion.exe"
Name: "{autodesktop}\Clarion"; Filename: "{app}\Clarion.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Clarion.exe"; Description: "Start Clarion"; Flags: nowait postinstall skipifsilent runascurrentuser

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  DataDir: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    DataDir := ExpandConstant('{commonappdata}\Clarion');
    if DirExists(DataDir) and (not UninstallSilent) then
      if MsgBox('Also delete Clarion history and logs?' + #13#10 + #13#10 +
                'The history is what lets you undo past changes. Keep it if you might reinstall.',
                mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
        DelTree(DataDir, True, True, True);
  end;
end;
