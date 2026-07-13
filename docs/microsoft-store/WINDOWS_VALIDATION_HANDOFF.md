# ProDENT View Windows Store Validation Handoff

## Target

- Branch: `feature/store-submission`
- App: ProDENT View
- Version: 1.0.0.0
- Architecture: x64
- Store installer: `installer/ProDENTViewStore.iss`
- Publisher: VENOKA USA INC

## Current Signed Candidate

- File: `ProDENTView-1.0.0.0-Store-Setup.exe`
- SHA256: `01C866A38A48117B1005A97CCC4E79BDE7E93A243288087CFE70353F8F627A6A`
- App Authenticode: verified, 0 warnings, 0 errors
- Installer Authenticode: verified, 0 warnings, 0 errors
- Silent install: passed
- Silent uninstall while the app was running: passed; the app closed and installed files were removed
- Startup Run registry values: none

## Ordered Validation

1. Pull the target branch and record the commit SHA.
2. Run `dotnet build src/ProDentView.Win/ProDentView.Win.csproj --configuration Release`.
3. Publish the self-contained x64 app with `scripts/publish-exe.cmd`.
4. Build `installer/ProDENTViewStore.iss` with Inno Setup 6.
5. Sign the published app and outer installer with the VENOKA USA INC code-signing certificate.
6. Run `signtool verify /pa /all /v` on the installer and installed app.
7. Record the installer SHA256.
8. Test silent install: `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART`.
9. Confirm installation creates an Apps & Features entry named `ProDENT View` with publisher `VENOKA USA INC`.
10. Confirm installation does not launch the app.
11. Confirm no startup values are written under HKLM/HKCU `Software\Microsoft\Windows\CurrentVersion\Run`.
12. Launch as a normal non-admin user with no camera attached.
13. Attach a ProDENT UVC intraoral camera and verify enumeration, preview, capture, disconnect, and reconnect.
14. Test still-pin capture and preview-frame fallback where supported.
15. Create only fictional test patient data; verify save/reload, import, review, Recycle Bin deletion, and export.
16. Export diagnostics and verify that no file is transmitted automatically.
17. Verify the privacy policy and support links are public.
18. Run Microsoft Defender/SmartScreen checks and retain evidence.
19. Launch the app, then test silent uninstall through the registered quiet uninstall command; confirm the running app closes and the installation directory is removed.
20. Confirm uninstall does not delete user-created patient records or images.

## Go / No-Go

Go only when the signed installer passes silent install/uninstall, camera regression, local-data/privacy checks, and Store listing assets are complete. Any crash, data loss, invalid signature, unwanted startup entry, automatic app launch, or unexpected network request is a No-Go.
