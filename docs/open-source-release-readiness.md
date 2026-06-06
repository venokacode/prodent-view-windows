# Open Source Release Readiness

## Decision

ProDENT View for Windows is feasible to release as an open-source Windows-only
project under Apache-2.0.

Recommended release target:

- Repository: `venokacode/prodent-view-windows`
- Visibility: public
- License: Apache-2.0
- Source boundary: Windows-only WPF/.NET source
- Binary release: publish only after Windows hardware smoke testing

## Why This Is Feasible

- The repository contains Windows source only.
- The app uses .NET 8, WPF, Windows Forms interop, and DirectShow COM
  declarations in source form.
- No macOS source is included.
- No macOS vendor SDK binary is included.
- No `eMPSnapshotKit`, `.framework`, `.xcframework`, or `.dSYM` artifacts are
  included.
- No App Store Connect API keys, certificates, provisioning profiles, packaged
  Store artifacts, or patient sample data are intentionally included.
- The repository has Apache-2.0 license text, contribution guidance, security
  guidance, third-party notices, and trademark/brand notices.

## Verification Performed

Date: 2026-06-06

Repository build:

```powershell
dotnet build .\src\ProDentView.Win\ProDentView.Win.csproj --configuration Release
```

Result:

- Build succeeded.
- 0 warnings.
- 0 errors.

Tracked-file hygiene checks:

- No tracked `eMPSnapshotKit` paths.
- No tracked `.framework`, `.xcframework`, or `.dSYM` artifacts.
- No tracked `.p8`, `.pfx`, `.cer`, `.mobileprovision`, or `.provisionprofile`
  files.
- No tracked `bin`, `obj`, `AppPackages`, `.msix`, `.appx`, `.msixupload`, or
  `.appxupload` release artifacts.

Expected text matches:

- The source contains patient/image management code because that is part of the
  product.
- Documentation contains warnings not to publish patient-identifying data.
- Documentation contains code-signing examples, but no real signing material.

## What Is Included

- `src/ProDentView.Win`: WPF app source.
- `scripts/publish-exe.cmd`: Windows EXE publishing entrypoint.
- `scripts/publish-exe.ps1`: PowerShell publish script.
- `docs/prodent-view-windows-prd.md`: Windows PRD.
- `docs/prodent-view-windows-v1-brief.md`: Windows V1 brief.
- `docs/exe-release-readiness.md`: EXE release path.
- `docs/microsoft-store-readiness.md`: Microsoft Store path.
- `docs/open-source-readiness.md`: open-source boundary.
- `LICENSE`: Apache-2.0 license.
- `NOTICE`: brand and trademark notice.
- `THIRD_PARTY_NOTICES.md`: dependency notice.
- `SECURITY.md`: security reporting guidance.
- `CODE_OF_CONDUCT.md` and `CONTRIBUTING.md`: public collaboration guidance.
- `.github`: CI, issue templates, and pull request template.

## What Is Excluded

- macOS app source.
- macOS App Store submission artifacts.
- macOS vendor SDK binaries.
- hardware vendor SDK binaries.
- signing certificates and private keys.
- build outputs.
- Store packages.
- real patient data, customer diagnostics, support uploads, and screenshots
  containing patient information.

## First Public Release Plan

Use source release first. Defer public binary release until Windows hardware
validation passes.

Suggested first tag:

```text
windows-v0.1.0-source
```

Suggested release title:

```text
ProDENT View Windows v0.1.0 Source Preview
```

Suggested release notes:

```markdown
## ProDENT View Windows v0.1.0 Source Preview

This is the first Windows-only open-source source preview of ProDENT View.

Included:

- WPF/.NET 8 app source
- DirectShow UVC camera enumeration
- DirectShow live preview hosting
- UVC still pin capture attempt
- preview-frame JPEG fallback
- local patient/date image organization
- image import, multi-select delete, export, and diagnostics export
- EXE publish script

Not yet production-certified:

- Windows hardware smoke test is still required.
- UVC still pin behavior must be verified on ProDENT intraoral camera hardware.
- Public EXE distribution should wait for code signing or explicit unsigned
  test-build labeling.
```

## Binary Release Gate

Before attaching EXE/ZIP assets to a public GitHub Release, complete:

- Windows 10/11 launch test.
- UVC camera auto-detection test.
- 1280 x 720 preview test.
- DirectShow still pin capture test.
- Preview-frame fallback capture test.
- import/export/delete/diagnostics test.
- no real patient data in screenshots or logs.
- binary signing decision:
  - signed public EXE, or
  - clearly labeled unsigned test build.

## Microsoft Store Gate

Before Microsoft Store submission:

- confirm app icon and Store screenshots.
- create MSIX packaging project on Windows.
- confirm package identity in Partner Center.
- publish privacy policy URL.
- run physical hardware demo workflow.
- keep Store package artifacts out of git.

## References

- Apache License 2.0: https://www.apache.org/licenses/LICENSE-2.0.html
- GitHub repository visibility: https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/managing-repository-settings/setting-repository-visibility
- GitHub Releases: https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases
- Microsoft MSIX desktop packaging: https://learn.microsoft.com/en-us/windows/msix/desktop/desktop-to-uwp-packaging-dot-net
