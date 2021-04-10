#define AppName        GetStringFileInfo('..\..\bin\BacklightShifter.exe', 'ProductName')
#define AppVersion     GetStringFileInfo('..\..\bin\BacklightShifter.exe', 'ProductVersion')
#define AppFileVersion GetStringFileInfo('..\..\bin\BacklightShifter.exe', 'FileVersion')
#define AppCompany     GetStringFileInfo('..\..\bin\BacklightShifter.exe', 'CompanyName')
#define AppCopyright   GetStringFileInfo('..\..\bin\BacklightShifter.exe', 'LegalCopyright')
#define AppBase        LowerCase(StringChange(AppName, ' ', ''))


[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppCompany}
AppPublisherURL=https://medo64.com/{#AppBase}/
AppCopyright={#AppCopyright}
VersionInfoProductVersion={#AppVersion}
VersionInfoProductTextVersion={#AppVersion}
VersionInfoVersion={#AppFileVersion}
DefaultDirName={pf}\{#AppCompany}\{#AppName}
SourceDir=..\..\bin
OutputDir=..\build
OutputBaseFilename=setup
AppId=JosipMedved_BacklightShifter
CloseApplications="yes"
RestartApplications="no"
AppMutex=Global\JosipMedved_BacklightShifter
UninstallDisplayIcon={app}\BacklightShifter.exe
AlwaysShowComponentsList=no
ArchitecturesInstallIn64BitMode=x64
DisableProgramGroupPage=yes
MergeDuplicateFiles=yes
MinVersion=6.1
PrivilegesRequired=admin
ShowLanguageDialog=no
SolidCompression=yes
ChangesAssociations=yes
DisableWelcomePage=yes
LicenseFile=..\setup\win\License.rtf


[Messages]
SetupAppTitle=Setup {#AppName} {#AppVersion}
SetupWindowTitle=Setup {#AppName} {#AppVersion}
BeveledLabel=medo64.com


[Files]
Source: "BacklightShifter.exe";  DestDir: "{app}";                            Flags: ignoreversion;
Source: "BacklightShifter.pdb";  DestDir: "{app}";                            Flags: ignoreversion;
Source: "..\README.md";          DestDir: "{app}";  DestName: "ReadMe.txt";   Flags: overwritereadonly uninsremovereadonly;  Attribs: readonly;
Source: "..\LICENSE.md";         DestDir: "{app}";  DestName: "License.txt";  Flags: overwritereadonly uninsremovereadonly;  Attribs: readonly;


[Icons]
Name: "{userstartmenu}\Backlight Shifter";  Filename: "{app}\BacklightShifter.exe"


[Run]
Filename: "{app}\BacklightShifter.exe";  Parameters: "--install";  Flags: runascurrentuser waituntilterminated;
Filename: "{app}\ReadMe.txt";                                      Flags: postinstall nowait skipifsilent runasoriginaluser unchecked shellexec;  Description: "View ReadMe.txt";


[UninstallRun]
Filename: "{app}\BacklightShifter.exe";  Parameters: "--uninstall";  Flags: runascurrentuser waituntilterminated


[Code]

procedure InitializeWizard;
begin
  WizardForm.LicenseAcceptedRadio.Checked := True;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
    ResultCode: Integer;
begin
    Exec(ExpandConstant('{app}\BacklightShifter.exe'), '--uninstall', '', SW_SHOW, ewWaitUntilTerminated, ResultCode)
    Result := Result;
end;
