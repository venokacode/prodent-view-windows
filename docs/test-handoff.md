# Test Handoff

## Handoff Summary

Product: ProDENT View for Windows

Purpose: lightweight UVC intraoral camera preview, photo capture, and local
patient/date image organization.

Handoff status: ready for Windows QA smoke testing.

Not production-certified yet. The main goal of this test cycle is to validate
camera behavior on real Windows hardware and identify DirectShow/UVC device
compatibility gaps.

## Repository

- Public repository: https://github.com/venokacode/prodent-view-windows
- Main source project: `src/ProDentView.Win/ProDentView.Win.csproj`
- PRD: `docs/prodent-view-windows-prd.md`
- EXE release notes: `docs/exe-release-readiness.md`
- Store readiness notes: `docs/microsoft-store-readiness.md`

## Build And Package

Build from source:

```powershell
dotnet build .\src\ProDentView.Win\ProDentView.Win.csproj --configuration Release
```

Run from source:

```powershell
dotnet run --project .\src\ProDentView.Win\ProDentView.Win.csproj --configuration Release
```

Publish self-contained EXE:

```bat
scripts\publish-exe.cmd
```

Default output:

- `artifacts\windows-exe\win-x64\ProDENT View.exe`
- `artifacts\windows-exe\ProDENT-View-Windows-win-x64.zip`
- `artifacts\windows-exe\ProDENT-View-Windows-win-x64.zip.sha256`

## Required Test Environment

- Windows 10 version 2004 or later, or Windows 11.
- Normal non-admin user account.
- .NET 8 SDK for source-build testing.
- At least one ProDENT UVC intraoral camera.
- One fallback generic UVC camera if available.
- Folder where QA can safely create/delete test images.

Do not use real patient information in test data, screenshots, bug reports, or
diagnostics attachments.

## Primary Test Matrix

| Area | Required? | Notes |
| --- | --- | --- |
| App launch | Required | Launch without admin rights. |
| UVC enumeration | Required | App should auto-detect attached camera. |
| Live preview | Required | Confirm preview starts and resizes. |
| Still pin capture | Required when hardware supports it | Confirm capture route in diagnostics. |
| Preview fallback capture | Required | Test with camera/no-still-pin path when possible. |
| Patient create/edit | Required | Name required; optional fields persist. |
| Image import | Required | Duplicate names must not overwrite. |
| Image delete | Required | Multi-select and select-all should move to Recycle Bin. |
| Image export | Required | Export should avoid overwriting same-name files. |
| Diagnostics export | Required | Attach to bug reports when camera/capture fails. |
| Disconnect/reconnect | Required | App should not crash. |
| EXE package | Required | Verify ZIP extraction and direct EXE launch. |

## Smoke Test Script

1. Start Windows with no camera attached.
2. Launch ProDENT View.
3. Confirm app does not crash and shows no-camera state.
4. Attach ProDENT UVC intraoral camera.
5. Click Refresh if the camera does not appear automatically.
6. Confirm camera appears in the camera selector.
7. Confirm live preview starts.
8. Resize the app window and confirm preview remains usable.
9. Create a patient using only the Name field.
10. Click Capture.
11. Confirm a JPEG appears in the image list.
12. Double-click the image and confirm full preview opens.
13. Import two image files with the same filename.
14. Confirm both imported images are retained with no overwrite.
15. Select multiple images and delete.
16. Confirm deleted files go to Recycle Bin.
17. Export images to a test folder.
18. Export diagnostics.
19. Close and reopen app.
20. Confirm patient and image history reload.

## Camera-Specific Tests

For each camera tested, record:

- camera model
- USB VID/PID if available
- Windows version
- app build/source commit
- preview result
- capture route:
  - StillPin
  - PreviewFrameFallback
  - Unavailable
- capture success/failure
- diagnostics file path
- notes/screenshots with no patient-identifying data

## Expected Diagnostics Fields

Diagnostics should include:

- generated timestamp
- app version
- OS description
- OS architecture
- .NET runtime
- machine name
- selected camera name/path
- detected camera count
- last capture route
- last camera status
- image root
- patient count
- loaded image count

## Known Risks Before QA

- DirectShow still pin behavior is hardware-dependent.
- Preview-frame fallback uses the current preview window and may vary by video
  renderer/driver.
- Camera disconnect/reconnect has not been validated on physical Windows
  hardware.
- App icon and MSIX Store packaging are not final.
- Birth date and sex are modeled but not exposed in the current UI.

## Bug Report Requirements

For every camera/preview/capture bug, include:

- Windows version.
- Camera model and USB connection path if known.
- Whether the app was run from source or EXE package.
- Steps to reproduce.
- Expected result.
- Actual result.
- Diagnostics export.
- Screenshot or short video if useful, with no real patient data.

## Exit Criteria For This QA Cycle

- App launches and closes cleanly on Windows 10/11.
- At least one ProDENT UVC intraoral camera previews successfully.
- Capture succeeds through still pin or documented fallback.
- Patient/image/import/delete/export flows pass smoke testing.
- Diagnostics export is sufficient for support triage.
- All P0/P1 crashes or data-loss bugs are fixed or explicitly deferred.
