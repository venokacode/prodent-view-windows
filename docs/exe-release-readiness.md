# ProDENT View Windows EXE Release Readiness

## Release Direction

Use a self-contained Windows EXE first for fast hardware validation and early
customer testing.

This is separate from the Microsoft Store path. EXE distribution is faster, but
public unsigned EXE files can trigger Microsoft Defender SmartScreen warnings.

## Build Output

The publish script creates:

- `artifacts/windows-exe/win-x64/ProDENT View.exe`
- `artifacts/windows-exe/ProDENT-View-Windows-win-x64.zip`
- `artifacts/windows-exe/ProDENT-View-Windows-win-x64.zip.sha256`

The ZIP also includes:

- `LICENSE`
- `THIRD_PARTY_NOTICES.md`
- `README-Windows.md`

Default runtime:

- `win-x64`

Optional runtime:

- `win-arm64`

## Publish Command

Run on a Windows 10/11 development machine with the .NET 8 SDK installed:

```bat
scripts\publish-exe.cmd
```

PowerShell direct call:

```powershell
.\scripts\publish-exe.ps1
```

Publish ARM64:

```powershell
.\scripts\publish-exe.ps1 -Runtime win-arm64
```

Publish without ZIP:

```powershell
.\scripts\publish-exe.ps1 -NoZip
```

## Optional Code Signing

For internal testing, the EXE can be unsigned.

For public distribution, sign the EXE with a trusted code-signing certificate or
Microsoft Trusted Signing/Azure Artifact Signing when available.

Example with a PFX:

```powershell
.\scripts\publish-exe.ps1 `
  -CertificatePath C:\certs\prodent-view.pfx `
  -CertificatePassword "<password>"
```

Do not commit certificates, passwords, generated EXE packages, or ZIP artifacts.

## Manual Smoke Test

Run these checks on a physical Windows machine:

1. Unzip the artifact to a normal user folder.
2. Launch `ProDENT View.exe` without administrator permission.
3. Attach a UVC intraoral camera.
4. Confirm the camera is detected automatically.
5. Confirm preview starts.
6. Add a patient with only the name field filled.
7. Capture a JPEG through still pin when available.
8. Confirm preview-frame fallback captures a JPEG when still pin is unavailable.
9. Import two images with the same name and confirm no overwrite.
10. Select all images and delete to Recycle Bin.
11. Export diagnostics and confirm camera/capture/storage details are present.
12. Close and reopen the app; confirm patient and image history still load.

## Public EXE Notes

The EXE route is useful for:

- fast hardware validation
- support/demo builds
- pre-Store customer testing
- Windows-only open-source release artifacts

The Microsoft Store route should still use MSIX. See
`docs/microsoft-store-readiness.md`.
