# Project Structure

This repository is split into several app and support modules:

- `API/`
  Windows capture API host and related WinForms UI files.
- `App Experience/`
  Main desktop app experience and helper UI.
- `CaptureEngine/`
  Reusable recording engine code such as audio and screen capture.
- `Notifier/`
  Notification app and overlay-related notifier flow.
- `Overlay/`
  Overlay UI, resources, local runtime data, and the main overlay solution.
- `Web/`
  Static website assets and pages for the project site.
- `.github/workflows/`
  GitHub Actions workflow configuration.

## Notes

- `bin/`, `obj/`, `.vs/`, restored packages, and local runtime data are intentionally ignored.
- Some folders contain designer-generated WinForms files inside bracketed directories such as `[Forms - Project Files]`. Those are source files and should stay tracked.
- Current uncommitted work exists in `Overlay/`; avoid bulk cleanup there unless you intend to review those changes first.
