#define AppName "Keybinderr"
#define AppVersion "0.1.0"
#ifndef PublishDir
  #define PublishDir "..\artifacts\publish\Keybinderr"
#endif
#ifndef OutputDir
  #define OutputDir "..\artifacts\installer"
#endif

[Setup]
AppId={{48C6C8DE-3CC2-4D05-84BF-FAB6C42191B9}
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={localappdata}\Programs\{#AppName}
DefaultGroupName={#AppName}
OutputDir={#OutputDir}
OutputBaseFilename=KeybinderrSetup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\Keybinderr.exe

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Keybinderr"; Filename: "{app}\Keybinderr.exe"
Name: "{group}\Keybinderr Settings"; Filename: "{app}\Keybinderr.exe"; Parameters: "--settings"
Name: "{group}\Uninstall Keybinderr"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\Keybinderr.exe"; Description: "Launch Keybinderr"; Flags: nowait postinstall skipifsilent

