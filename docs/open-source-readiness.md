# ProDENT View Windows Open Source Readiness

## Recommended Open Source Boundary

Open source the Windows app as a Windows-only release under Apache-2.0.

The current full repository also contains macOS vendor SDK binary artifacts for hardware-button support. Those artifacts are not automatically covered by the repository Apache-2.0 license and should not be presented as open-source code unless the vendor grants explicit redistribution rights.

Recommended options:

1. Create a separate public repository for the Windows app.
2. Or publish a Windows-only branch that contains:
   - `src/`
   - `docs/`
   - `LICENSE`
   - `README.md`
   - `CONTRIBUTING.md`
   - `THIRD_PARTY_NOTICES.md`

Do not publish the macOS vendor SDK binary as part of a Windows-only open-source release.

## Recommended GitHub Strategy

Keep `venokacode/prodent-view` private.

Create a new public repository for the Windows source, for example:

- `venokacode/prodent-view-windows`
- or `venokacode/prodent-view-win`

Reason:

- The private repo currently tracks macOS vendor SDK binary artifacts.
- Making the existing repo public would expose its existing file history, not only the current tree.
- A clean Windows-only repository gives the public project a simpler license boundary and contributor story.

## Export Command

This repository is already the exported Windows-only public tree.

It intentionally excludes:

- macOS source and project files
- vendor SDK binaries
- `.framework`, `.xcframework`, and `.dSYM` artifacts
- App Store Connect keys and provisioning files
- certificates and package artifacts
- build outputs

## Current Windows Dependency Posture

The Windows app currently uses:

- .NET 8
- WPF
- Windows Forms host for DirectShow preview interop
- Windows/DirectShow COM interfaces declared in source
- local JSON and file storage

No proprietary Windows SDK binary is currently required by the Windows source tree.

## License

Use Apache-2.0 for the Windows source.

Required files:

- `LICENSE`
- `THIRD_PARTY_NOTICES.md`
- `CONTRIBUTING.md`
- Windows-specific README

The top-level `THIRD_PARTY_NOTICES.md` must keep the distinction between:

- Apache-2.0 source code
- platform SDKs supplied by Microsoft
- macOS vendor binaries that are not part of the Windows open-source release

## Public README Checklist

The public Windows README should include:

- Product boundary: ProDENT View is not ProDENT Capture.
- Supported scope: UVC intraoral camera preview, capture, local image organization, export.
- Non-goals: shortcut matrices, SendInput routing, clinic CRM, licensing, remote sync.
- Build requirements: Windows 10/11, Visual Studio 2022, .NET 8 SDK, Windows SDK.
- Store packaging note: MSIX packaging project should be created on Windows.
- Hardware note: still pin capture depends on what the UVC camera exposes.
- Diagnostics note: support exports are local text files.

## Public Repository Hygiene

Before making the Windows repository public:

- Remove build outputs and generated packages.
- Start from the exported Windows-only tree, not from the private repo history.
- Do not commit `.pfx`, `.cer`, `.msix`, `.appx`, `.msixupload`, `.appxupload`, or Store credentials.
- Do not commit patient images, test images with patient information, or diagnostics from real customers.
- Keep sample data synthetic.
- Make sure screenshots do not show real patient information.

## Public Repository Creation Flow

1. Keep this repository Windows-only.
2. Do not copy files from the private macOS repository except through the reviewed Windows export.
3. Keep generated packages and test patient data out of git.
4. Add or maintain repository topics:
   - `intraoral-camera`
   - `uvc-camera`
   - `directshow`
   - `wpf`
   - `dental-imaging`
   - `apache-2-0`
5. Create GitHub Releases only after Windows hardware smoke testing.

## Git Ignore Additions

The public repo should ignore:

- `bin/`
- `obj/`
- `.vs/`
- `*.user`
- `*.suo`
- `*.pfx`
- `*.cer`
- `*.msix`
- `*.appx`
- `*.msixupload`
- `*.appxupload`
- `AppPackages/`

## Release Tagging

Suggested tag format:

- `windows-v0.1.0`
- `windows-v1.0.0-store-candidate`

Suggested branch names:

- `main`
- `windows/store-candidate`
- `windows/public`

## Remaining Before Public Launch

- Build and run on Windows.
- Confirm DirectShow preview and still pin capture on ProDENT hardware.
- Add final icon and screenshots.
- Add a Windows packaging project or document exact Visual Studio packaging steps.
- Run a license scan after the Windows-only public tree is prepared.
