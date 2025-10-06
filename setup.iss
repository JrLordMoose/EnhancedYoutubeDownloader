; Enhanced YouTube Downloader - Inno Setup Script
; This script creates a Windows installer with desktop shortcut and launch options

#define MyAppName "Enhanced YouTube Downloader"
#define MyAppVersion "0.3.1"
#define MyAppPublisher "JrLordMoose"
#define MyAppURL "https://github.com/JrLordMoose/EnhancedYoutubeDownloader"
#define MyAppExeName "EnhancedYoutubeDownloader.exe"
#define MyAppIcon "favicon.ico"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
AppId={{A8F3D9E1-7B4C-4A2E-9F1D-3C5E8A6B2D4F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=LICENSE
OutputDir=release
OutputBaseFilename=EnhancedYoutubeDownloader-Setup-v{#MyAppVersion}
SetupIconFile={#MyAppIcon}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
; Create uninstaller in the installation directory for easy access
CreateUninstallRegKey=yes
; Show uninstaller in Add/Remove Programs - place in root directory
UninstallFilesDir={app}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce
Name: "desktopuninstall"; Description: "Create desktop uninstaller shortcut"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "launchafterinstall"; Description: "Launch {#MyAppName} after installation"; GroupDescription: "Post-installation:"; Flags: checkedonce

[Files]
Source: "src\Desktop\bin\Release\net9.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
; Start Menu shortcuts
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "Launch {#MyAppName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"; Comment: "Uninstall {#MyAppName}"
; Desktop shortcuts
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "Launch {#MyAppName}"
Name: "{autodesktop}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"; Tasks: desktopuninstall; Comment: "Uninstall {#MyAppName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent; Tasks: launchafterinstall

[UninstallDelete]
; Clean up application data on uninstall (optional - user data cleanup)
Type: filesandordirs; Name: "{localappdata}\{#MyAppName}"

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
  AppDataPath: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // Ask user if they want to remove application data
    if MsgBox('Do you want to remove all application data including download history and settings?' + #13#10#10 +
              'Choose "Yes" to completely remove all data.' + #13#10 +
              'Choose "No" to keep your settings and download history.',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      AppDataPath := ExpandConstant('{localappdata}\{#MyAppName}');
      if DirExists(AppDataPath) then
      begin
        DelTree(AppDataPath, True, True, True);
      end;
    end;
  end;
end;


