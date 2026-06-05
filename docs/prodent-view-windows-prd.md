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
