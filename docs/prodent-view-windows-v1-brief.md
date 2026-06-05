# ProDENT View Windows V1 Brief

## Product Boundary

ProDENT View for Windows is a lightweight intraoral camera viewer and patient/date image organizer.

It is not ProDENT Capture, PC, or MC. It must not own the large dental-software hotkey matrix.

## Core V1 Scope

- UVC intraoral camera preview.
- Photo capture from the app UI, using the UVC still image path first.
- Image save/import/delete/export.
- Patient name required; other patient fields optional.
- Date-based image grouping.
- Basic camera format and performance settings.
- Diagnostics export for support.

## Windows Camera Direction

The Windows product should treat ProDENT View as its own foreground UVC viewer
and photo capture app. It should not detect or coordinate with ProDENT Capture,
PC, or MC.

Preferred capture order:

1. DirectShow still image pin: use `PIN_CATEGORY_STILL` when the device exposes it.
2. Software trigger: call `IAMVideoControl::SetMode` with `VideoControlFlag_Trigger` when supported by the running graph.
3. Fallback: capture the current preview frame and save it as JPEG when the device has no still pin or trigger support.

The fallback is a compatibility path, not the main quality target.

## Suggested Windows Architecture

- UI: WPF on .NET 8.
- Camera: DirectShow-first for UVC enumeration, preview, and still pin capture.
- Storage: local app data folder plus user-selected export folder.
- Data: lightweight local SQLite or file-backed metadata.
- Button behavior: local app capture only. Do not implement ProDENT Capture shortcut routing.

## First Windows Spike

1. Enumerate UVC cameras with DirectShow `SystemDeviceEnum`.
2. Open live preview at 1280 x 720 default when available.
3. Detect whether the selected device exposes a DirectShow still pin.
4. Capture one JPEG through still pin trigger.
5. Fall back to preview-frame JPEG capture if still pin is unavailable.
6. Save to patient/date folder.
7. Import same-name files without overwriting existing images.

## Non-Goals

- Dental software shortcut matrix.
- SendInput automation.
- PC/MC coexistence detection or bridge ownership.
- Licensing, registration, remote sync, or fake backend behavior.
- Broad clinic CRM.
