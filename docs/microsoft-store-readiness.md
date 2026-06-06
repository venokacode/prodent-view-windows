# ProDENT View Windows Microsoft Store Readiness

## Release Direction

Ship the Windows app as a packaged desktop app using MSIX.

Reasons:

- ProDENT View Windows is a WPF desktop app.
- Microsoft documents WPF/desktop apps as eligible for MSIX packaging through a Windows Application Packaging Project.
- Microsoft Store distribution re-signs MSIX packages after certification, so we do not need to buy and manage a production code-signing certificate for the Store path.
- MSI/EXE Store submission is possible, but the installer remains the publisher's signing responsibility and is not the preferred first release path.

Official references:

- https://learn.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-packaging-dot-net
- https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options
- https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msix/create-app-submission
- https://learn.microsoft.com/en-us/windows/apps/publish/publish-your-app/msix/add-and-edit-store-listing-info

## Current App Package Baseline

- App name: ProDENT View
- Product: lightweight intraoral camera viewer and photo capture utility.
- UI framework: WPF
- Runtime target: `.NET 8`, `net8.0-windows10.0.19041.0`
- Camera path: DirectShow-first UVC enumeration, live preview, still pin capture, preview-frame fallback.
- Local data: `%LocalAppData%\ProDENT\ProDENT View`
- Network/backend: none for V1.
- Elevation/admin requirement: none.

## Windows Development Machine Requirements

- Windows 10/11 development machine.
- Visual Studio 2022.
- Workloads/components:
  - .NET desktop development
  - MSIX Packaging Tools
  - Windows 10/11 SDK
- .NET 8 SDK.
- At least one UVC intraoral camera for final validation.

## Packaging Plan

1. Open `src/ProDentView.Win/ProDentView.Win.csproj` in Visual Studio.
2. Build `Release|x64` and confirm the app runs without elevation.
3. Add a Windows Application Packaging Project to the Windows solution.
4. Reference `ProDentView.Win` from the packaging project.
5. Set package identity from the reserved Partner Center app name.
6. Add Store visual assets:
   - Square 44x44 logo
   - Square 150x150 logo
   - Square 310x310 logo
   - Wide 310x150 logo
   - Store logo
7. In `Package.appxmanifest`, verify capabilities:
   - webcam/camera access for UVC preview and capture
   - no broad capabilities unless a real feature needs them
8. Use Visual Studio `Publish > Create App Packages`.
9. Select Microsoft Store distribution and create `.msixupload`/`.appxupload`.
10. Install and run the package locally before upload.

## Store Listing Draft

Short description:

> ProDENT View is a lightweight intraoral camera preview and photo capture app for simple image review, organization, and export.

Description:

> ProDENT View helps operators, support teams, and demo users preview a UVC intraoral camera, capture still images, organize images by patient and date, and export files for review. It is intentionally small and focused: no clinic CRM, no dental software bridge, no shortcut routing, and no remote sync in V1.

Keywords:

- intraoral camera
- dental camera
- UVC camera
- dental imaging
- photo capture

Category:

- Medical or Productivity. Choose final category after reviewing current Partner Center options.

Privacy:

- The app stores patient/image data locally.
- The app does not upload images or patient data in V1.
- A public privacy policy URL is still required for Store submission.

## Manual QA Gate

Before Store upload, pass these tests on a physical Windows machine:

- Fresh install opens without admin permission.
- App auto-detects an attached UVC intraoral camera.
- Preview starts at 1280 x 720 when available.
- Capture succeeds through still pin on supported hardware.
- Capture fallback saves a usable JPEG on a camera without still pin.
- Add patient with name-only data.
- Save optional patient fields.
- Import images with duplicate names without overwrite.
- Select all images and delete to Recycle Bin.
- Double-click image opens full preview.
- Export selected patient images.
- Diagnostics export includes OS, app version, selected camera, capture route, and storage path.
- Disconnect/reconnect camera does not crash the app.

## Submission Notes

Use Partner Center notes to explain:

- This app is for UVC intraoral camera preview and photo capture.
- Hardware pairing is USB camera connection; there is no account login.
- Images are stored locally by patient/date.
- The app does not include ProDENT Capture bridge behavior.

## Open Items

- Create final Windows app icon and Store screenshots.
- Add packaging project on a Windows machine.
- Confirm final package identity after reserving the Store app name.
- Publish privacy policy URL.
- Run final QA with ProDENT UVC intraoral camera hardware.
