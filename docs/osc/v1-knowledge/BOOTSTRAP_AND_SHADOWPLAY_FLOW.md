# ShadowPlay V1 - Bootstrap flow
Source of truth: C:\My Project\ShadowPlay\osc
Reconstructed entry: app module 0
Angular main module: app module 1

1. Webpack entry module 0
- Requires modules 136, 1, 135, 142.
- Calls appService.main() during Angular run phase.

2. Angular main module 1
- Declares the "main" Angular module.
- Depends on main.constants and main.common plus Angular Material, translate, ui-router and animation modules.

3. UI router module 136
- Default route: /base.
- /base resolves localNodeInfo, localizedConfig and hardwareInfo.
- onEnter for /base calls appService.initializeUI().
- Main menu route uses <nv-main-menu>.
- Preferences routes map directly to nv-preferences-* directives.

4. appService module 135
- main(): records build info, sets window name, initializes online state, logging/telemetry/l10n and cold-start timing.
- initializeUI(): connects services and runs staged async initialization.
- Final stage calls setOscReady(), then ends cold-start timing.

5. ShadowPlay branch
- shadowPlayService is app module 4.
- shadowPlayEndpoints provider is app module 208.
- MainMenuController is app module 210.
- MainMenuController injects shadowPlayService as callback parameter u.
- Manual Record UI calls tryStartManualRecord() and stopAndSaveManualRecord().
- Instant Replay UI calls startInstantReplay(), stopInstantReplay(), saveInstantReplay() and uploadInstantReplay().

6. Manual Record proven endpoint chain
MainMenuController
-> shadowPlayService.tryStartManualRecord()
-> shadowPlayService.startManualRecord()
-> shadowPlayService.setManualRecording(true)
-> shadowPlayEndpoints.setManualRecording()
-> POST /Record/Enable with status=true

Stop/save chain:
MainMenuController
-> shadowPlayService.stopAndSaveManualRecord()
-> shadowPlayService.setManualRecording(false)
-> shadowPlayEndpoints.setManualRecording()
-> POST /Record/Enable with status=false

This document records observed relationships only; it does not infer undocumented backend behavior.