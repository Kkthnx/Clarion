; Builds the .Clarion setup program. Run scripts\build-installer.ps1 rather than calling this directly.
#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\artifacts\publish"
#endif

[Setup]
AppId={{6F0B2C1E-4D7A-4B9E-9C55-2E1A7C0D8F31}
AppName=.Clarion
AppVersion={#AppVersion}
AppVerName=.Clarion {#AppVersion}
AppPublisher=Kkthnx
AppPublisherURL=https://github.com/Kkthnx/Clarion
AppSupportURL=https://github.com/Kkthnx/Clarion/issues
AppUpdatesURL=https://github.com/Kkthnx/Clarion/releases
DefaultDirName={autopf}\Clarion
DefaultGroupName=.Clarion
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\Clarion.exe
UninstallDisplayName=.Clarion
SetupIconFile=..\src\Clarion.App\Assets\Clarion.ico
LicenseFile=..\LICENSE
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
Name: "{autoprograms}\.Clarion"; Filename: "{app}\Clarion.exe"
Name: "{autodesktop}\.Clarion"; Filename: "{app}\Clarion.exe"; Tasks: desktopicon

[InstallDelete]
; The program was called Clarion before it was called .Clarion, so the shortcuts made under the old name are removed on upgrade.
Type: files; Name: "{autoprograms}\Clarion.lnk"
Type: files; Name: "{autodesktop}\Clarion.lnk"
; Setup copies the new files over the old ones and never removes a file the new version no longer ships. Each .NET update leaves a debugging
; library named after its version, and logos taken out of the program stayed behind. These two go in any folder, since only .Clarion puts them there.
Type: files; Name: "{app}\mscordaccore_*.dll"
Type: filesandordirs; Name: "{app}\Assets\AppLogos"
; In the folder setup made for .Clarion, the program files of the old version are all cleared first, so nothing from it is left mixed in with
; the new one. Not in a folder the person picked that has another name, where other files could live. Settings, history and logs are kept
; in ProgramData and the account's own folder, never here, and the uninstaller files are left alone.
Type: files; Name: "{app}\*.dll"; Check: IsOwnFolder
Type: files; Name: "{app}\*.winmd"; Check: IsOwnFolder
Type: files; Name: "{app}\*.pri"; Check: IsOwnFolder
Type: files; Name: "{app}\*.pdb"; Check: IsOwnFolder
Type: files; Name: "{app}\*.json"; Check: IsOwnFolder
Type: files; Name: "{app}\*.png"; Check: IsOwnFolder
Type: files; Name: "{app}\createdump.exe"; Check: IsOwnFolder
Type: files; Name: "{app}\RestartAgent.exe"; Check: IsOwnFolder
Type: filesandordirs; Name: "{app}\Assets"; Check: IsOwnFolder
; Last, because IsOwnFolder looks for it.
Type: files; Name: "{app}\Clarion.exe"; Check: IsOwnFolder

[Run]
Filename: "{app}\Clarion.exe"; Description: "Start .Clarion"; Flags: nowait postinstall skipifsilent runascurrentuser
; An update started from inside .Clarion runs silently with /RELAUNCH, so the program comes back by itself when the update is done.
Filename: "{app}\Clarion.exe"; Flags: nowait runascurrentuser; Check: WantsRelaunch

[UninstallDelete]
; The one-file flag for starting without the administrator prompt lives in the account's own folder. This reaches the account that runs the uninstall.
Type: files; Name: "{localappdata}\Clarion\start-limited.flag"
Type: dirifempty; Name: "{localappdata}\Clarion"

[UninstallRun]
; The optional monthly check is a scheduled task Clarion made. Left behind it would start a program that is no longer there.
Filename: "{sys}\schtasks.exe"; Parameters: "/Delete /TN ""\Clarion\Monthly check"" /F"; Flags: runhidden; RunOnceId: "RemoveMonthlyCheck"

[Code]
{ True when the install folder is one .Clarion already lives in and it is named Clarion, as setup names it. A first install, or a folder the
  person picked under another name, keeps every file that is already there. }
function IsOwnFolder: Boolean;
var
  Dir: String;
begin
  Dir := RemoveBackslashUnlessRoot(ExpandConstant('{app}'));
  Result := FileExists(AddBackslash(Dir) + 'Clarion.exe') and (CompareText(ExtractFileName(Dir), 'Clarion') = 0);
end;

function WantsRelaunch: Boolean;
var
  I: Integer;
begin
  Result := False;
  if not WizardSilent then Exit;
  for I := 1 to ParamCount do
    if CompareText(ParamStr(I), '/RELAUNCH') = 0 then
    begin
      Result := True;
      Exit;
    end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  DataDir: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    DataDir := ExpandConstant('{commonappdata}\Clarion');
    if DirExists(DataDir) and (not UninstallSilent) then
      if MsgBox('Also delete .Clarion history and logs?' + #13#10 + #13#10 +
                'The history is what lets you undo past changes. Keep it if you might reinstall.',
                mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
        DelTree(DataDir, True, True, True);
  end;
end;
