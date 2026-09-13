// ─────────────────────────────────────────────────────────────
// APP MODULE 2
// role       : constant WINDOW_STYLES | constant SOCKETIO_EVENTS | constant RECORDING_STATES | constant RECORDING_PATH_TYPES | constant BROADCAST_STATES | constant OSC_MODE
// defines    : angular.module("main.common")
// requires   : (none)
// channels   : /Account/v.1.0/UserToken, /Account/v.1.0/PrivacySettings, /abHubAPI/v.0.1/Message, /abHubAPI/v.0.1/Status, /PiplConfig/v.1.0/update
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }

  function o(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.ngMainCommonModule = void 0;
  var r = n(3),
    a = o(r),
    l = n(132),
    s = i(l);
  n(471), n(466), n(468), n(265), n(88), n(469), n(134), n(133);
  var d = n(102),
    c = n(17);
  window._ = a, window.localforage = s.default;
  var u = angular.module("main.common", ["crimson", "ngSanitize", "main.config", "main.userConfig", "ngMaterial",
      "pascalprecht.translate", "ngEventAggregator", "ui.router", c.ngLocalSdkModule.name, d.ngTelemetryModule.name,
      "nvJsEvents", "ugc-lib", "events-detail", "nvAngularAccountSdk", "nvExperienceControl"
    ]).constant("WINDOW_STYLES", {
      WINDOWS: "windows",
      OTHER: "other"
    }).constant("SOCKETIO_EVENTS", {
      CONNECT: "connect",
      DISCONNECT: "disconnect",
      ERROR: "error"
    }).constant("RECORDING_STATES", {
      RECORDING: "Recording",
      NOTRECORDING: "NotRecording"
    }).constant("RECORDING_PATH_TYPES", {
      VIDEOS: "Videos",
      TEMP_FILES: "TempFiles",
      HIGHLIGHTS: "Highlights",
      FILE_LOGGING: "FileLogging"
    }).constant("BROADCAST_STATES", {
      ACTIVE: "Active",
      STOPPED: "Stopped",
      PAUSED: "Paused"
    }).constant("OSC_MODE", {
      INSTANTREPLAY: "InstantReplay",
      MANUALRECORD: "Manual",
      BROADCAST: "Broadcast"
    }).constant("LOCALHOST_ADDR", "http://localhost:").constant("NOTIFICATION_EVENT", "notification-service-event")
    .constant("LOCALHOST_PORT", 3e3).constant("TILE_STATUS_BRUSH", {
      NORMAL: "normal",
      HIGHLIGHTED: "highlighted",
      ALTERNATE: "alternate"
    }).constant("COPLAY_STATE", {
      OFF: "Off",
      ON: "On",
      PAUSED: "Paused",
      CREATINGINVITE: "CreatingInvite",
      INVITESENT: "InviteSent",
      CLIENTREADY: "ClientReady"
    }).constant("COPLAY_CONTROLLER_MAPPING", {
      BLOCKED: "blocked",
      MIRRORED: "mirrored",
      EXCLUSIVE: "exclusive",
      EXCLUSIVE_LOCAL_PRIORITY: "exclusiveLocalPriority"
    }).constant("COPLAY_EVENTS", {
      COPLAY_STATE_CHANGED: "CoplayUIStateChanged"
    }).constant("OSC_EVENTS", {
      OSC_STATE: "OSC_STATE",
      DESKTOP_STATE: "DESKTOP_STATE"
    }).constant("ACCORDION_MODES", {
      ALWAYS_ONE: "alwaysOne",
      ONLY_ONE: "onlyOne",
      MULTIPLE_OPEN: "multiple"
    }).constant("HOTKEY_EVENTS", {
      OSC_TOGGLE: "Hotkey_OSC",
      DVR: "Hotkey_DVR",
      MANUAL: "Hotkey_Manual",
      GAMECAST: "Hotkey_Gamecast",
      MIC_PTT_DOWN: "Hotkey_MicPTTDown",
      MIC_PTT_UP: "Hotkey_MicPTTUP",
      CAMERA: "Hotkey_Camera",
      PAUSERESUME: "Hotkey_PauseResume",
      FPS: "Hotkey_FPS",
      SCREENSHOT: "Hotkey_Screenshot",
      CUSTOMOVERLAY: "Hotkey_CustomOverlay",
      NVCAMERAUI: "Hotkey_NvCameraUI",
      CUSTOMOVERLAYA: "Hotkey_CustomOverlayA",
      CUSTOMOVERLAYB: "Hotkey_CustomOverlayB",
      CUSTOMOVERLAYC: "Hotkey_CustomOverlayC",
      COMMENTS_TOGGLE: "Hotkey_CommentsToggle",
      DVR_TOGGLE: "Hotkey_DVRToggle",
      MIC_TOGGLE: "Hotkey_MicToggle",
      MODS_TOGGLE: "Hotkey_ModsToggle",
      MODS_CYCLE: "Hotkey_ModsCycle",
      MODS_SHOWUI: "Hotkey_ModsShowUI",
      MODS_PRESET1: "Hotkey_ModsPreset1",
      MODS_PRESET2: "Hotkey_ModsPreset2",
      MODS_PRESET3: "Hotkey_ModsPreset3",
      ANSEL_HIDEUI: "Hotkey_AnselHideUI",
      EDGE_OSD: "Hotkey_EDGE_OSD",
      EDGE_OSC: "Hotkey_EDGE_OSC",
      OCTOOLUI_TOGGLE: "Hotkey_OcToolUI",
      PERFOVERLAY_TOGGLE: "HotKey_PerfOverlayToggle",
      PERFOVERLAY_CYCLE: "HotKey_PerfOverlayCycle",
      RESET_AVERAGES: "HotKey_ResetAverages",
      TOGGLE_LOGGING: "HotKey_ToggleLogging"
    }).constant("WINDOW_EVENTS", {
      DISPLAY_HOTKEY: "Window_DisplayHotkey"
    }).constant("SHADOWPLAY_EVENTS", {
      STATUS_CHANGE_RECORD: "StatusChangeRecord",
      STATUS_CHANGE_BROADCAST: "StatusChangeBroadcast",
      MIC_STATUS_CHANGE: "MicStateChange",
      WEBCAM_STATUS_CHANGE: "WebcamStateChange",
      IR_RECORDING_STATE_CHANGED: "IRRecordingStateChanged",
      RECORDING_SAVED: "RecordingSaved",
      COPLAY_ENABLED_CHANGED: "CoplayEnabledChanged",
      FILE_READY_TO_UPLOAD: "FileReadyToUpload",
      VIEWER_COUNT_UPDATE: "ViewerCountUpdate",
      HIGHLIGHTS_STATUS_CHANGE: "HighlightsStatusChange",
      GAME_STARTED: "GameAppStarted",
      GAME_EXITED: "GameAppExited",
      BROADCAST_DISPLAY_VIEWER_COUNT: "BroadcastDisplayViewerCount",
      HDR_SCREENSHOT: "HdrScreenshot"
    }).constant("NOTIFIER_SELECTIONS", {
      OPEN_SHARE: "OPEN_SHARE",
      ANSEL_READY_APP_STARTED: "ANSEL_READY_APP_STARTED",
      INSTANT_REPLAY_STARTED: "INSTANT_REPLAY_STARTED",
      INSTANT_REPLAY_STOPPED: "INSTANT_REPLAY_STOPPED",
      INSTANT_REPLAY_SAVED: "INSTANT_REPLAY_SAVED",
      INSTANT_REPLAY_SAVED_TO_GALLERY: "INSTANT_REPLAY_SAVED_TO_GALLERY",
      RECORD_STARTED: "RECORD_STARTED",
      RECORD_STOPPED: "RECORD_STOPPED",
      RECORD_STOPPED_AND_SAVED_TO_GALLERY: "RECORD_STOPPED_AND_SAVED_TO_GALLERY",
      BROADCAST_STARTED: "BROADCAST_STARTED",
      BROADCAST_STOPPED: "BROADCAST_STOPPED",
      BROADCAST_STREAMER_STOPPED: "BROADCAST_STREAMER_STOPPED",
      BROADCAST_PAUSED: "BROADCAST_PAUSED",
      BROADCAST_RESUMED: "BROADCAST_RESUMED",
      BROADCAST_LOGIN: "BROADCAST_LOGIN",
      BROADCAST_YOUTUBE_LIVE_STREAMING: "BROADCAST_YOUTUBE_LIVE_STREAMING",
      BROADCAST_FAILED: "BROADCAST_FAILED",
      BROADCAST_TIMEOUT_N_MINUTES: "BROADCAST_TIMEOUT_N_MINUTES",
      BROADCAST_TIMEOUT_1_MINUTE: "BROADCAST_TIMEOUT_1_MINUTE",
      BROADCAST_ENDED_TIMEOUT: "BROADCAST_ENDED_TIMEOUT",
      CUSTOMOVERLAY_NOT_ASSIGNED: "CUSTOMOVERLAY_NOT_ASSIGNED",
      CUSTOMOVERLAY_TURNED_OFF: "CUSTOMOVERLAY_TURNED_OFF",
      CUSTOMOVERLAY_ALL_SLOT_EMPTY: "CUSTOMOVERLAY_ALL_SLOT_EMPTY",
      SCREENSHOT_SAVED: "SCREENSHOT_SAVED",
      SCREENSHOT_SAVED_TO_GALLERY: "SCREENSHOT_SAVED_TO_GALLERY",
      PHOTOGRAPHIC_SCREENSHOT_SAVED_TO_GALLERY: "PHOTOGRAPHIC_SCREENSHOT_SAVED_TO_GALLERY",
      WARNING_PHOTOGRAPHY_NOT_ALLOWED: "WARNING_PHOTOGRAPHY_NOT_ALLOWED",
      WARNING_FULLSCREEN_GAME_REQUIRED: "WARNING_FULLSCREEN_GAME_REQUIRED",
      WARNING_BROADCAST_STOP_TO_CUSTOMIZE: "WARNING_BROADCAST_STOP_TO_CUSTOMIZE",
      WARNING_RECORDING_STOP_TO_CUSTOMIZE: "WARNING_RECORDING_STOP_TO_CUSTOMIZE",
      WARNING_HIGHLIGHTS_STOP_TO_CUSTOMIZE: "WARNING_HIGHLIGHTS_STOP_TO_CUSTOMIZE",
      WARNING_BROADCAST_STOP_TO_USE_FEATURE: "WARNING_BROADCAST_STOP_TO_USE_FEATURE",
      WARNING_RECORDING_STOP_TO_USE_FEATURE: "WARNING_RECORDING_STOP_TO_USE_FEATURE",
      WARNING_INSTANT_REPLAY_STOP_TO_USE_FEATURE: "WARNING_INSTANT_REPLAY_STOP_TO_USE_FEATURE",
      WARNING_HIGHLIGHTS_STOP_TO_USE_BROADCAST: "WARNING_HIGHLIGHTS_STOP_TO_USE_BROADCAST",
      WARNING_COPLAY_STOP_TO_USE_FEATURE: "WARNING_COPLAY_STOP_TO_USE_FEATURE",
      WARNING_DELETE_FAILED: "WARNING_DELETE_FAILED",
      WARNING_SUPPORTED_GAME_REQUIRED: "WARNING_SUPPORTED_GAME_REQUIRED",
      WARNING_DESKTOP_CAPTURE_DISABLED: "WARNING_DESKTOP_CAPTURE_DISABLED",
      WARNING_NVIDIA_GPU_REQUIRED: "WARNING_NVIDIA_GPU_REQUIRED",
      WARNING_INTERNET_CONNECTION_REQUIRED: "WARNING_INTERNET_CONNECTION_REQUIRED",
      WARNING_NVCAMERA_FILTER_DISPLAY_INFO: "WARNING_NVCAMERA_FILTER_DISPLAY_INFO",
      COPLAY_5_MINUTES: "COPLAY_5_MINUTES",
      COPLAY_1_MINUTE: "COPLAY_1_MINUTE",
      COPLAY_EXPIRED: "COPLAY_EXPIRED",
      COPLAY_NOW_PLAYING: "COPLAY_NOW_PLAYING",
      COPLAY_INVITETO_CANCELLED: "COPLAY_INVITETO_CANCELLED",
      COPLAY_COPYING_INVITE: "COPLAY_COPYING_INVITE",
      COPLAY_PAUSED: "COPLAY_PAUSED",
      COPLAY_STOPPEDPLAYING: "COPLAY_STOPPEDPLAYING",
      COPLAY_DONE: "COPLAY_DONE",
      COPLAY_SENDING_INVITE: "COPLAY_SENDING_INVITE",
      UPLOAD_SUCCESS: "UPLOAD_SUCCESS",
      UPLOAD_FAILED: "UPLOAD_FAILED",
      UPLOAD_STARTED: "UPLOAD_STARTED",
      UPLOAD_URL_COPIED: "UPLOAD_URL_COPIED",
      NO_YOUTUBE_CHANNEL: "NO_YOUTUBE_CHANNEL",
      HIGHLIGHTS_SAVED: "HIGHLIGHTS_SAVED",
      HIGHLIGHT_SAVED_TO_GALLERY: "HIGHLIGHT_SAVED_TO_GALLERY",
      HIGHLIGHT_MANUAL_RECORD_RUNNING: "HIGHLIGHT_MANUAL_RECORD_RUNNING",
      HIGHLIGHT_BROADCAST_RUNNING: "HIGHLIGHT_BROADCAST_RUNNING",
      ERROR_NVCAMERA_LAUNCH_FAILED: "ERROR_NVCAMERA_LAUNCH_FAILED",
      ENABLE_MODS: "ENABLE_MODS",
      ENABLE_ANSEL: "ENABLE_ANSEL",
      ENABLE_ANSEL_EXPERIENTAL: "ENABLE_ANSEL_EXPERIENTAL",
      FEATURE_UPDATE_DRIVER: "FEATURE_UPDATE_DRIVER",
      HDR_ERROR_SCREENSHOT: "HDR_ERROR_SCREENSHOT",
      HDR_ERROR_RECORD: "HDR_ERROR_RECORD",
      HDR_ERROR_BROADCAST: "HDR_ERROR_BROADCAST",
      HDR_ERROR_HL: "HDR_ERROR_HL",
      HDR_ENABLED: "HDR_ENABLED",
      SELECT_FILE_GALLERY: "SELECT_FILE_GALLERY",
      WHISPER_MODE_ENABLED: "WHISPER_MODE_ENABLED",
      WHISPER_MODE_DISABLED: "WHISPER_MODE_DISABLED",
      WHISPER_MODE_ENABLED_GAMESTART: "WHISPER_MODE_ENABLED_GAMESTART",
      CONNECT_LOGIN_TO_GFE: "CONNECT_LOGIN_TO_GFE",
      CONNECT_EXIT_FULL_SCREEN: "CONNECT_EXIT_FULL_SCREEN",
      LOGIN_COMPLETED_WINDOWED_GAME_BROADCAST: "LOGIN_COMPLETED_WINDOWED_GAME_BROADCAST",
      PERFMON_RECTALIGNMENT_SUPPORT: "PERFMON_RECTALIGNMENT_SUPPORT",
      PERFMON_RFI_SUPPORT: "PERFMON_RFI_SUPPORT"
    }).constant("OSC_KEYBOARD", {
      BACKSPACE: 8,
      TAB: 9,
      ENTER: 13,
      SHIFT: 16,
      CONTROL: 17,
      ALT: 18,
      ESCAPE: 27,
      SPACEBAR: 32,
      LEFT_ARROW: 37,
      UP_ARROW: 38,
      RIGHT_ARROW: 39,
      DOWN_ARROW: 40,
      INSERT: 45,
      DELETE: 46,
      PERIOD: 190
    }).constant("KEYBOARD_EVENTS", {
      ESCAPE: "Keyboard_Escape"
    }).constant("GAMEPAD_EVENTS", {
      LEFT_BUMPER: "Left_Bumper",
      RIGHT_BUMPER: "Right_Bumper",
      X_BUTTON: "X_button",
      Y_BUTTON: "Y_Button",
      NAVIGATION: "Navigation"
    }).constant("COMMON_EVENTS", {
      SYSTEMINFO_UPDATED: "main.common.systemInfoUpdated",
      ONLINE: "main.common.online",
      OFFLINE: "main.common.offline",
      LOCALE_CHANGED: "main.common.localeChanged",
      EXPERIMENTAL_CHANGED: "main.common.experimentalChanged",
      WINDOW_RESIZE: "main.common.windowResize",
      OSD_SETTINGS_CHANGED: "main.common.osdSettingsChanged",
      ELEMENT_FOCUSSED: "This_Element_In_Focus",
      PIPL_CONFIG_UPDATED: "main.common.localizedConfigUpdated"
    }).constant("AUDIO_STATE", {
      MAIN: "MAIN",
      PREFERENCES: "PREFERENCES"
    }).constant("VIDEO_STATE", {
      MAIN: "MAIN",
      PREFERENCES: "PREFERENCES"
    }).constant("NVCAMERA_EVENTS", {
      STOP: "stop",
      FILTERS: "filters",
      HIGHRES_RESOLUTIONS: "highResResolutions",
      SCREENSHOT_RESOLUTION: "screenshotResolution",
      CAMERA_FOV_RANGE: "cameraFOVRange",
      CAMERA_ROLL_RANGE: "cameraRollRange",
      CAMERA_FOV_VALUE: "cameraFOVValue",
      CAMERA_ROLL_VALUE_SET: "setRoll",
      CAMERA_FOV_VALUE_SET: "setFov",
      AVAILABLE: "available",
      CURRENT_FILTER_SETTINGS: "filter_settings",
      CAPTURE_CONTROL_CHANGE: "captureControlChange",
      SCREENSHOT_CAPTURE_STARTED: "screenshotStarted",
      SCREENSHOT_CAPTURE_INPROGRESS: "screenshotProgress",
      SCREENSHOT_CAPTURE_FINISHED: "screenshotFinished",
      SCREENSHOT_CAPTURE_DONE: "screenshotDone",
      SCREENSHOT_CAPTURE_FILE_WRITE_DONE: "screenshotFileWriteDone",
      SCREENSHOT_CAPTURE_FAILED: "screenshotfailed",
      AVAILABILITY_CHANGED: "availabilityChanged",
      ADD_UI_ELEMENT: "addUIElement",
      REMOVE_UI_ELEMENT: "removeUIElement",
      SET_UI_ELEMENT_VISIBILITY: "setUIElementVisibility",
      GET_UI_ELEMENT_VISIBILITY: "getUIElementVisibility",
      REMOVE_ALL_GAME_SETTINGS: "uiControlRemoveAllRequest",
      CHECK_AND_INIT_ACTION: "checkAndInitAction",
      MODS_FILTERS_LOADED: "modsFiltersLoaded",
      MODS_FILTER_SETTINGS_READY: "modsFiltersReady",
      CAPTURE_TYPES: "captureTypes",
      CAMERA_ROLL_VALUE_UPDATE: "CameraRollValueUpdate",
      FILTER_STACK_RESETTED: "EntireFilterStackResetted",
      UPDATE_UI_DATA: "UpdateUiData"
    }).constant("NVCAMERA_STATUS", {
      OK: 1,
      FAILED: 2,
      STARTED: 3,
      FAILED_TO_START: 4,
      NO_SPACE_LEFT: 5,
      PERMISSION_DENIED: 6,
      INVALID_REQUEST: 7,
      FAILED_TO_PROCESS: 8,
      PROCESS_DECLINED: 9,
      ALREADY_ENABLED: 10,
      ALREADY_DISABLED: 11,
      OUT_OF_RANGE: 12,
      ALREADY_SET: 13,
      INCOMPATIBLE_VERSION: 14,
      DISABLED: 15,
      OK_ANSEL: 16,
      OK_MODSONLY: 17,
      ERROR_FILEPARSING: 19,
      APP_FATAL_ERROR: "FatalError",
      APP_NON_FATAL_ERROR: "NonFatalError",
      FAILED_TO_SAVE_SHOT_NO_SPACE_LEFT: 25,
      NGX_FEATURE_NOT_SUPPORTED: "NgxIsrFeatureNotSupported",
      NGX_OUT_OF_DATE: "NgxIsrOutOfDate",
      NGX_OUT_OF_GPU_MEMORY: "NgxIsrOutOfGPUMem",
      SCREENSHOT_TIMEOUT_FAILURE: "NgxIsrTimeout",
      SCREENSHOT_GENERIC_FAILURE: "NgxIsrGenericFailure"
    }).constant("NVCAMERA_MODE", {
      gamePhoto: "GamePhoto",
      gameFilter: "GameFilter",
      anselLite: "AnselLite"
    }).constant("NGX_NOTIFICATIONS", {
      AI_SUPER_RES_STARTED: "AISuperResStarted",
      AI_SUPER_RES_PROGRESS: "AISuperResProgress",
      AI_SUPER_RES_DONE: "AISuperResDone",
      AI_SUPER_RES_FAILED: "AISuperResFailed"
    }).constant("HIGHLIGHTS_EVENTS", {
      MOVE_STARTED: "moveStarted",
      MOVE_INPROGRESS: "moveInProgress",
      MOVE_DONE: "moveDone",
      HIGHLIGHT_COMPLETED: "highlightCompleted"
    }).constant("HIGHLIGHT_PERMISSIONS", {
      GRANTED: "granted",
      DENIED: "denied"
    }).constant("CONFIRMATION_TYPE", {
      PERMISSION: "permission"
    }).constant("AB_STORE", {
      AB_STORE_NAME: "AB_DB",
      USER_AB_STATE: "AB_STATE"
    }).constant("CLIENT_LOCALES", [{
      LCID: 1029,
      code: "cs-CZ",
      name: "čeština (Czech)",
      isoThreeLetter: "ces"
    }, {
      LCID: 1030,
      code: "da-DK",
      name: "Dansk (Danish)",
      isoThreeLetter: "dan"
    }, {
      LCID: 1031,
      code: "de-DE",
      name: "Deutsch (German)",
      isoThreeLetter: "deu"
    }, {
      LCID: 1032,
      code: "el-GR",
      name: "ελληνικά (Greek)",
      isoThreeLetter: "ell"
    }, {
      LCID: 1033,
      code: "en-US",
      name: "English (United States)",
      isoThreeLetter: "eng"
    }, {
      LCID: 2057,
      code: "en-GB",
      name: "English (United Kingdom)",
      isoThreeLetter: "eng"
    }, {
      LCID: 1034,
      code: "es-ES",
      name: "Español (Spanish - Spain)",
      isoThreeLetter: "spa"
    }, {
      LCID: 3082,
      code: "es-ES",
      name: "Español (Spanish - Spain)",
      isoThreeLetter: "spa"
    }, {
      LCID: 2058,
      code: "es-MX",
      name: "Español (Spanish - Mexico)",
      isoThreeLetter: "spa"
    }, {
      LCID: 1035,
      code: "fi-FI",
      name: "Suomi (Finnish)",
      isoThreeLetter: "fin"
    }, {
      LCID: 1036,
      code: "fr-FR",
      name: "Français (French)",
      isoThreeLetter: "fra"
    }, {
      LCID: 1038,
      code: "hu-HU",
      name: "Magyar (Hungarian)",
      isoThreeLetter: "hun"
    }, {
      LCID: 1040,
      code: "it-IT",
      name: "Italiano (Italian - Italy)",
      isoThreeLetter: "ita"
    }, {
      LCID: 1041,
      code: "ja-JP",
      name: "日本語 (Japanese)",
      isoThreeLetter: "jpn"
    }, {
      LCID: 1042,
      code: "ko-KR",
      name: "한국어 (Korean)",
      isoThreeLetter: "kor"
    }, {
      LCID: 1043,
      code: "nl-NL",
      name: "Nederlands (Dutch - Netherlands)",
      isoThreeLetter: "nld"
    }, {
      LCID: 1044,
      code: "nb-NO",
      name: "Norsk (Norwegian)",
      isoThreeLetter: "nor"
    }, {
      LCID: 1045,
      code: "pl-PL",
      name: "Polski (Polish)",
      isoThreeLetter: "pol"
    }, {
      LCID: 2070,
      code: "pt-PT",
      name: "Português (Portuguese - Portugal)",
      isoThreeLetter: "por"
    }, {
      LCID: 1046,
      code: "pt-BR",
      name: "Português-Brasil (Portuguese - Brazil)",
      isoThreeLetter: "por"
    }, {
      LCID: 1049,
      code: "ru-RU",
      name: "Русский (Russian)",
      isoThreeLetter: "rus"
    }, {
      LCID: 1051,
      code: "sk-SK",
      name: "slovenských (Slovak)",
      isoThreeLetter: "slk"
    }, {
      LCID: 1060,
      code: "sl-SI",
      name: "slovenski (Slovenian)",
      isoThreeLetter: "slv"
    }, {
      LCID: 1053,
      code: "sv-SE",
      name: "Svenska (Swedish)",
      isoThreeLetter: "swe"
    }, {
      LCID: 1054,
      code: "th-TH",
      name: "ไทย (Thai)",
      isoThreeLetter: "tha"
    }, {
      LCID: 1055,
      code: "tr-TR",
      name: "Türkçe (Turkish)",
      isoThreeLetter: "tur"
    }, {
      LCID: 2052,
      code: "zh-CHS",
      name: "简体中文 (Chinese - Simplified)",
      isoThreeLetter: "zho"
    }, {
      LCID: 3076,
      code: "zh-CHT",
      name: "繁體中文 (Chinese - Traditional)",
      isoThreeLetter: "zho"
    }, {
      LCID: 1028,
      code: "zh-CHT",
      name: "繁體中文 (Chinese - Traditional)",
      isoThreeLetter: "zho"
    }, {
      LCID: 1058,
      code: "uk-UA",
      name: "Ukrainian (Ukraine)",
      isoThreeLetter: "ukr"
    }]).constant("ACCOUNT_SOCKET_EVENTS", {
      USER_TOKEN_CHANGED: "/Account/v.1.0/UserToken",
      USER_CONSENT_CHANGED: "/Account/v.1.0/PrivacySettings"
    }).constant("ACCOUNT_EVENTS", {
      USER_TOKEN_CHANGED: "UserTokenChanged",
      USER_CONSENT_CHANGED: "UserConsentChanged"
    }).constant("USER_CONSENT_LEVEL", {
      FULL: "Full",
      NONE: "None"
    }).constant("GDPR_CONSENT", {
      NONE: {
        functional: "None",
        technical: "None",
        behavioral: "None"
      },
      DEFAULT: {
        functional: "Full",
        technical: "None",
        behavioral: "None"
      }
    }).constant("AB_HUB_SOCKET_EVENTS", {
      AB_CONTEXT_UPDATED: "/abHubAPI/v.0.1/Message",
      AB_HUB_STATUS_UPDATED: "/abHubAPI/v.0.1/Status"
    }).constant("AB_HUB_MESSAGE_TYPES", {
      SYNC: "SYNC",
      ERROR: "ERROR",
      ADD: "ADD",
      DELETE: "DELETE",
      GET: "GET"
    }).constant("PERFTOOL_EVENTS", {
      PERF_OVERLAY_VISIBILITY_CHANGED: "PERF_OVERLAY_VISIBILITY_CHANGED",
      PERF_OCSCAN_COMPLETION_UPDATE: "PERF_OCSCAN_COMPLETION_UPDATE",
      PERF_OVERLAY_LAMSUPPORT_CHANGED: "PERF_OVERLAY_LAMSUPPORT_CHANGED",
      FEATURE_SUPPORT_STATE_CHANGE: "FEATURE_SUPPORT_STATE_CHANGED",
      PERF_OVERLAY_OTHER_OSD_VISIBILITY_CHANGED: "PERF_OVERLAY_OTHER_OSD_VISIBILITY_CHANGED",
      PERF_AUTO_TUNING_INFO_UPDATED: "PERF_AUTO_TUNING_INFO_UPDATED"
    }).constant("PERFTOOL_VIEWNAMES", {
      OFF: "Off",
      LATENCY: "Latency",
      FPS: "FPS",
      BASIC: "Basic",
      ADVANCED: "Advanced",
      REFLEX_ANALYZER: "Reflex Analyzer"
    }).constant("QUIET_MODE2_EVENTS", {
      STATE_UPDATE: "quietMode2.state.update",
      SUPPORT_UPDATE: "quietMode2.support.update",
      CHANGED: "quietMode2.changed"
    }).constant("QUIET_MODE2_SERVICE_EVENTS", {
      STATE_UPDATE: "quietMode2.service.state.update",
      SUPPORT_UPDATE: "quietMode2.service.support.update"
    }).constant("OC_SCANNER_STATUS", {
      COMPLETED_SUCCESSFULLY: 4,
      SCAN_CANCELLED: 8,
      INTERNAL_ERROR: 16,
      NO_RESULTS_AVAILABLE: 32,
      CANCELLED_USER_REQUESTED: 64,
      CANCELLED_GPU_BUSY: 128,
      CANCELLED_USER_INPUT_DETECTED: 256,
      CANCELLED_POWER_SWITCH_TO_DC: 512,
      CANCELLED_SERVICE_SUSPENDED: 1024,
      CANCELLED_SERVICE_STOPPED: 2048,
      CANCELLED_BACKGROUND_SCAN_DISABLED: 4096,
      CANCELLED_CONFIGURATION_NOT_SUPPORTED: 8192
    }).constant("OC_NOT_SUPPORTED_STATE", {
      GPU_NOT_SUPPORTED: 1,
      DRIVER_NOT_SUPPORTED: 2
    }).constant("PIPL_CONFIG_SOCKET_EVENTS", {
      LOCALIZED_CONFIG_UPDATED: "/PiplConfig/v.1.0/update"
    }).constant("PIPL_CONFIG_SERVICE_EVENTS", {
      LOCALIZED_CONFIG_UPDATED: "piplConfig.service.data.update"
    });
  t.ngMainCommonModule = u
}
