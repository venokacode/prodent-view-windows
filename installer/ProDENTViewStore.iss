; Microsoft Store Win32 installer for ProDENT View.
; Build from the repository root after publishing win-x64:
;   scripts\publish-exe.cmd
;   scripts\build-store-installer.ps1
;
; Store-specific behavior:
; - supports silent install with /VERYSILENT /SUPPRESSMSGBOXES /NORESTART
; - supports silent uninstall with /VERYSILENT /SUPPRESSMSGBOXES /NORESTART
; - does not create HKLM/HKCU startup Run values
; - does not launch the app after installation

#define MyAppVersion "1.0.0.0"
#define MyAppName "ProDENT View"
#define MyAppPublisher "VENOKA USA INC"
#define MyAppExeName "ProDENT View.exe"
#define PublishDir "..\artifacts\windows-exe\win-x64"

[Setup]
AppId={{8B0E640E-B544-45C0-93D9-39A41357A7F4}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://www.prodentshop.com/
AppSupportURL=https://www.prodentshop.com/pages/contact-us
AppUpdatesURL=https://www.prodentshop.com/
DefaultDirName={autopf64}\ProDENT View
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=ProDENTView-1.0.0.0-Store-Setup
; The self-contained single-file EXE is already compressed. ZIP keeps Store
; builds deterministic and fast without materially improving runtime behavior.
Compression=zip
SolidCompression=no
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
; The outer Store installer and application executable are Authenticode signed
; by scripts/build-store-installer.ps1. The embedded Inno uninstaller remains
; unsigned to avoid certificate-provider deadlocks inside the compiler.
SignedUninstaller=no
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

[UninstallRun]
; Store-triggered silent uninstall must also succeed while the app is open.
; Patient data is not held in the installation directory and remains retained.
Filename: "{cmd}"; Parameters: "/C taskkill /IM ""{#MyAppExeName}"" /F >nul 2>&1"; Flags: runhidden waituntilterminated; RunOnceId: "StopProDENTView"

; Deliberately no [Run] or [Registry] startup entries.
; User-created patient records, images, and diagnostics under LocalAppData are
; retained during uninstall to avoid deleting customer data without consent.
