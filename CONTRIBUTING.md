# Contributing

Thanks for helping improve ProDENT View for Windows.

## Scope

This repository is for the Windows intraoral camera preview, capture, and local
image organization utility.

In scope:

- UVC camera enumeration, preview, and still capture
- local patient/date image organization
- image import, preview, delete, and export
- Windows packaging and hardware validation
- accessibility and reliability improvements

Out of scope:

- ProDENT Capture bridge behavior
- dental software shortcut matrices
- global hotkey injection
- licensing, heartbeat, or fake backend flows
- macOS-specific SDK integration

## Development

Build:

```powershell
dotnet build .\src\ProDentView.Win\ProDentView.Win.csproj
```

Run:

```powershell
dotnet run --project .\src\ProDentView.Win\ProDentView.Win.csproj
```

## Pull Requests

Before opening a PR:

- run `dotnet build`
- test camera enumeration if hardware is available
- test capture on a real or virtual camera when changing camera code
- keep patient-identifying data out of commits, screenshots, logs, and issues

