# ProDENT View Windows PRD

## Product Summary

ProDENT View for Windows is a lightweight intraoral camera preview and image capture app. It is a small patient/date image organizer for demonstration, support, and simple capture workflows.

It is not ProDENT Capture. It must not own dental software shortcut matrices, global hotkeys, or `SendInput` automation.

## Target User

- ProDENT operators who need a simple IOC preview and photo tool.
- Sales, demo, and support staff who need to show camera image quality quickly.
- Customers who do not want to operate a full dental management system for basic image capture.

## V1 Goals

- Open a generic UVC intraoral camera quickly.
- Default preview to 1280 x 720 when available.
- Capture still photos using the UVC still image path where the camera exposes it.
- Save, import, delete, review, and export images by patient and date.
- Keep the interface minimal and AMCap-inspired.
- Ship toward Microsoft Store readiness.

## Current Implementation Status

Status date: 2026-06-06

Implemented:

- WPF/.NET 8 desktop app shell.
- Three-column AMCap-inspired workspace:
  - left patient form and list
  - center large live preview
  - right selected-patient image list
- Patient name required.
- Optional patient fields currently exposed:
  - chart ID
  - phone
  - email
  - notes
- Optional patient fields currently modeled but not yet exposed in the UI:
  - birth date
  - sex
- Local JSON patient persistence.
- Local patient/date image storage.
- Import images with same-name collision protection.
- Multi-select, select-all, and recycle-bin delete.
- Double-click image preview window.
- Folder export for selected patient image set.
- Diagnostics text export.
- DirectShow UVC device enumeration.
- DirectShow live preview hosted through Windows Forms interop.
- DirectShow still pin capture attempt through `PIN_CATEGORY_STILL`.
- `IAMVideoControl::SetMode(..., VideoControlFlag_Trigger)` trigger path.
- Preview-window JPEG fallback.
- Windows x64 self-contained EXE publish script.
- Public Windows-only Apache-2.0 repository.

Not yet verified on Windows hardware:

- UVC camera enumeration on ProDENT intraoral camera hardware.
- 1280 x 720 preview selection on real hardware.
- Still pin capture success.
- Preview-frame fallback image quality.
- Disconnect/reconnect behavior.
- Microsoft Store MSIX packaging.

## V1 Non-Goals

- ProDENT Capture bridge behavior.
- Dental software shortcut mappings.
- PC/MC coexistence detection.
- Background hotkey or hardware-button routing into third-party software.
- Clinic CRM, remote sync, registration, licensing, or fake backend behavior.
- Advanced image editing.

## Camera Requirements

### Preview

- Enumerate UVC video devices.
- Prefer MJPEG 1280 x 720 at 30 fps when available.
- Support YUY2 fallback.
- Keep preview responsive during capture.
- Recover gracefully after camera disconnect/reconnect.
- Current implementation direction is DirectShow-first so preview and still capture share one camera graph.

### Still Capture

Primary path:

- Use DirectShow still image pin capture when the device exposes `PIN_CATEGORY_STILL`.
- Build and run the capture graph before triggering.
- Trigger capture using `IAMVideoControl::SetMode` with `VideoControlFlag_Trigger` when supported.

Fallback path:

- If no still pin is exposed or trigger fails, capture the current preview frame and encode JPEG.
- Mark diagnostics with the active capture route: `Still pin`, `Preview frame fallback`, or `Unavailable`.

## Patient And Image Management

- Patient name is required.
- Optional fields: patient ID, birth date, sex, phone, email, note.
- Images are grouped by capture date.
- Local storage path should be deterministic and user-readable.
- Same-name imports must not overwrite existing images.
- Delete should use recycle bin where possible.
- Export supports selected images and full patient/date groups.

## Test Acceptance Criteria

### Launch And Camera

- App launches on Windows 10/11 without administrator permission.
- App automatically enumerates attached UVC video devices on launch.
- If no camera is attached, app does not crash and shows a clear no-camera state.
- User can refresh camera list.
- Selecting a camera starts live preview.
- Preview resizes with the application window and remains visible.

### Capture

- Capture button requires a selected patient.
- When still pin is supported, app saves a JPEG through the still pin path.
- When still pin is unavailable or trigger fails, app saves a JPEG through preview-frame fallback.
- Diagnostics identifies the active capture route.
- Captured files are saved under the selected patient/date folder.

### Patient And Image Flows

- Patient name-only record can be created.
- Patient edits persist after app restart.
- Import supports multiple image files.
- Importing duplicate filenames does not overwrite existing files.
- Image list updates after capture/import/delete.
- Multi-select and select-all delete use Recycle Bin.
- Double-click opens a larger preview window.
- Export copies images without overwriting same-name files in the export folder.

### Diagnostics

- Diagnostics export creates a text file.
- Diagnostics include:
  - generated timestamp
  - app version
  - OS description and architecture
  - .NET runtime
  - machine name
  - selected camera name/path
  - detected camera count
  - last capture route
  - last camera status
  - image storage root
  - patient/image counts

### Stability

- Closing the app stops and disposes the camera service.
- Camera enumeration failure shows an error without crashing.
- Preview startup failure shows an error without crashing.
- Capture failure shows an error without crashing.

## UI Direction

- AMCap-inspired simple workspace.
- Left: patient list and search.
- Center: large adaptive camera preview, capture controls below.
- Right: compact image list/grid for the selected patient/date.
- Captured thumbnails remain small; double-click opens a larger preview.
- Avoid marketing-style hero screens.

## Diagnostics

Diagnostics export should include:

- OS version.
- App version.
- Camera device name and symbolic link.
- Selected preview format.
- Still pin availability.
- Capture route and last capture error.
- Storage root.

## Release Gates

- Windows build succeeds on a Windows 10/11 development machine.
- At least one UVC IOC camera previews at 1280 x 720.
- Still pin capture succeeds on supported hardware.
- Preview-frame fallback succeeds on a camera without still pin.
- Import, delete, export, and patient/date grouping pass manual QA.
- Microsoft Store packaging path is documented.

## Reference

Microsoft documents that cameras supporting still images can expose a `PIN_CATEGORY_STILL` pin, and that software triggering can use `IAMVideoControl::SetMode` with `VideoControlFlag_Trigger` while the graph is running.

- https://learn.microsoft.com/en-us/windows/win32/directshow/capturing-an-image-from-a-still-image-pin
- https://learn.microsoft.com/en-us/windows/win32/api/strmif/nf-strmif-iamvideocontrol-setmode
