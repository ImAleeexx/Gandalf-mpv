
#define MyAppName "Gandalf-Player"
#define MyAppExeName "Gandalf-Player.exe"
#define MyAppSourceDir "..\..\MpvNet.Windows\bin\Release\win-x64"
#define MyAppVersion GetFileVersion("..\..\MpvNet.Windows\bin\Release\win-x64\Gandalf-Player.exe")

[Setup]
AppId={{9AA2B100-BEF3-44D0-B819-D8FC3C4D557E}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=ImAleex_
ArchitecturesInstallIn64BitMode=x64
Compression=lzma2
DefaultDirName={autopf}\{#MyAppName}
OutputBaseFilename=gandalf-player-setup-x64
OutputDir=E:\Repos\Gandalf-mpv\Releases
DefaultGroupName={#MyAppName}
SetupIconFile=..\..\MpvNet.Windows\mpv-icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
PrivilegesRequired=admin
ShowLanguageDialog=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
Source: "{#MyAppSourceDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppSourceDir}\*"; DestDir: "{app}"; Excludes: "win-x64,win-arm64"; Flags: ignoreversion recursesubdirs createallsubdirs;

[Registry]
Root: HKCR; Subkey: "gandalf"; ValueType: string; ValueName: ""; ValueData: "URL:Gandalf Protocol"; Flags: uninsdeletekey
Root: HKCR; Subkey: "gandalf"; ValueType: string; ValueName: "URL Protocol"; ValueData: ""; Flags: uninsdeletekey
Root: HKCR; Subkey: "gandalf\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: """{app}\Gandalf-Player.exe"",1"; Flags: uninsdeletekey
Root: HKCR; Subkey: "gandalf\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\Gandalf-Player.exe"" ""--gandalf-link=%1"""; Flags: uninsdeletekey


[Code]
procedure CreateMpvConfIfNotExists();
var
  ConfigDir: string;
  ConfigFile: string;
  ConfigContent: string;
begin
  // Get the AppData\Roaming path and append Gandalf-Player
  ConfigDir := ExpandConstant('{userappdata}\Gandalf-Player');
  ConfigFile := ConfigDir + '\mpv.conf';
  
  // Create the directory if it doesn't exist
  if not DirExists(ConfigDir) then
    ForceDirectories(ConfigDir);
  
  // Create the file only if it doesn't exist
  if not FileExists(ConfigFile) then
  begin
ConfigContent := 'alang=spa' + #13#10 + 
                 'slang=spa' + #13#10 + 
                 'osd-playing-msg=""' + #13#10 + 
                 'title=""' + #13#10 + 
                 'script-opts=''osc-title="Videoclu de Gandalf"''';
    SaveStringToFile(ConfigFile, ConfigContent, False);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    CreateMpvConfIfNotExists();
  end;
end;