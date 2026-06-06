# ProDENT View for Windows

ProDENT View for Windows is a lightweight intraoral camera preview and photo
capture utility for generic UVC-compatible dental cameras.

The app is intentionally small. It focuses on:

- live preview from a UVC intraoral camera
- still photo capture, preferring the UVC still image path when available
- local patient/date image organization
- image import, review, delete, and export

It is not ProDENT Capture. It does not send hotkeys to third-party dental
software, maintain dental software shortcut profiles, or manage PC/MC
coexistence behavior.

## Current Status

This Windows implementation is an early source release. The WPF app shell,
local patient/image storage, DirectShow camera enumeration, live preview, UVC
still-pin capture attempt, preview-frame fallback, multi-select image
management, export, and diagnostics export are implemented.

Hardware validation should be completed on a Windows 10/11 machine with real
UVC intraoral cameras before using it in production.

## Requirements

- Windows 10 version 2004 or later, or Windows 11
- .NET 8 SDK
- A UVC-compatible intraoral camera for hardware validation

## Build

```powershell
dotnet build .\src\ProDentView.Win\ProDentView.Win.csproj
```

Run from source:

```powershell
dotnet run --project .\src\ProDentView.Win\ProDentView.Win.csproj
```

Publish a self-contained Windows x64 EXE:

```bat
scripts\publish-exe.cmd
```

Default output:

- `artifacts\windows-exe\win-x64\ProDENT View.exe`
- `artifacts\windows-exe\ProDENT-View-Windows-win-x64.zip`
- `artifacts\windows-exe\ProDENT-View-Windows-win-x64.zip.sha256`

## Camera Direction

The camera path is DirectShow-first so preview and capture can share one
camera graph:

- enumerate video input devices through DirectShow
- prefer `PIN_CATEGORY_STILL` with `IAMVideoControl::SetMode(..., Trigger)`
- fall back to capturing the current preview window when still-pin capture is
  unavailable
- prefer 1280 x 720 preview where supported by the device

## Local Data

Patient records and captured images are stored locally under the user's
Windows profile:

```text
%LOCALAPPDATA%\ProDENT\ProDENT View\
```

Do not attach real patient data, support uploads, private hardware logs, or
customer-identifying screenshots to public issues.

## License

Source code is licensed under Apache-2.0. See [LICENSE](LICENSE).

The ProDENT name, logos, product marks, and related brand assets are
trademarks and are not licensed under Apache-2.0. See [NOTICE](NOTICE).

## Release Readiness

- [Open source release readiness](docs/open-source-release-readiness.md)
- [EXE release readiness](docs/exe-release-readiness.md)
- [Microsoft Store readiness](docs/microsoft-store-readiness.md)
