// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 102
// constant TELEMETRY_OSC_EVENT_NAMES | constant TELEMETRY_OSC_CAPTURE_TYPE | constant TELEMETRY_OSC_TOGGLE_STATE | constant TELEMETRY_OSC_BOOLEAN_STATUS | constant TELEMETRY_OSC_AVAILABLE_STATUS | constant OSC_SETTINGS_CONTROL_TYPES | constant TELEMETRY_OSC_FILTER_CONTROL_TYPES | constant TELEMETRY_OSC_TRIGGER_MODE | constant TELEMETRY_OSC_MENU_TYPE | constant TELEMETRY_OSC_SCREEN_STATE | constant TELEMETRY_OSC_QUALITY_SETTING | constant TELEMETRY_OSC_MIC_MODE | constant TELEMETRY_OSC_ANSEL_TYPE | constant TELEMETRY_OSC_ANSEL_FAILURE_TYPE | constant TELEMETRY_OSC_SCREENSHOT_FAILURE_TYPE | constant TELEMETRY_OSC_ANSEL_GAMEPAD_TYPE | constant TELEMETRY_OSC_VALID_PROVIDERS | constant TELEMETRY_OSC_PERF_ID | constant TELEMETRY_CAPTURE_ACTION | constant TELEMETRY_OVERLAY_LOCATION | constant OVERCLOCKING_FEATURE | constant DISCLAIMER_OPTIONS | constant SYSTEM_TYPE | constant OVERLAY_VIEW | constant SCAN_TYPE | constant MODULE | defines angular.module("main.telemetry")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var n = angular.module("main.telemetry", []);
  n.constant("TELEMETRY_OSC_EVENT_NAMES", {
    OSC_LAUNCH_TIME_COLD: {
      name: "OscLaunchTimeCold",
      type: "TarCon_PresentationInfo",
      id: 200,
      enumId: "200"
    },
    OSC_LAUNCH_TIME_WARM: {
      name: "OscLaunchTimeWarm",
      type: "TarCon_PresentationInfo",
      id: 201,
      enumId: "201"
    },
    OSC_INGAME_FPS_DEGRADATION: {
      name: "OscInGameFpsDegradation",
      type: "",
      id: 202,
      enumId: "202"
    },
    OSC_CRASH: {
      name: "OscCrash",
      type: "TarCon_ClickInfo",
      id: 203,
      enumId: "203"
    },
    OSC_LAUNCH_FAIL: {
      name: "OscLaunchFail",
      type: "",
      id: 204,
      enumId: "204"
    },
    OSC_HOTKEY_TOGGLE: {
      name: "OscHotkeyToggle",
      type: "TarCon_ClickInfo",
      id: 205,
      enumId: "205"
    },
    OSC_HOTKEY_TOGGLE_SESSION_DURATION: {
      name: "OscHotkeyeToggleSessionDuration",
      type: "TarCon_PresentationInfo",
      id: 206,
      enumId: "206"
    },
    OSC_NAVIGATION_TIME: {
      name: "OscNavigationTime",
      type: "TarCon_PresentationInfo",
      id: 207,
      enumId: "207"
    },
    OSC_NAVIGATION_KEYBOARD: {
      name: "OscNavigationKeyboard",
      type: "TarCon_ClickInfo",
      id: 208,
      enumId: "208"
    },
    OSC_NAVIGATION_MOUSE: {
      name: "OscNavigationMouse",
      type: "TarCon_ClickInfo",
      id: 209,
      enumId: "209"
    },
    OSC_BROADCAST_TWITCH: {
      name: "OscBroadcastTwitch",
      type: "TarCon_ClickInfo",
      id: 210,
      enumId: "210"
    },
    OSC_BROADCAST_TWITCH_SESSION_DURATION_FS: {
      name: "OscBroadcastTwitchSessionDurationFS",
      type: "TarCon_PresentationInfo",
      id: 211,
      enumId: "211"
    },
    OSC_BROADCAST_TWITCH_SESSION_DURATION_DT: {
      name: "OscBroadcastTwitchSessionDurationDT",
      type: "TarCon_PresentationInfo",
      id: 212,
      enumId: "212"
    },
    OSC_BROADCAST_TWITCH_CUSTOMIZE: {
      name: "OscBroadcastTwitchCustomize",
      type: "TarCon_ClickInfo",
      id: 213,
      enumId: "213"
    },
    OSC_BROADCAST_TWITCH_CAMERA: {
      name: "OscBroadcastTwitchCamera",
      type: "TarCon_ClickInfo",
      id: 214,
      enumId: "214"
    },
    OSC_BROADCAST_TWITCH_MICROPHONE: {
      name: "OscBroadcastTwitchMicrophone",
      type: "TarCon_ClickInfo",
      id: 215,
      enumId: "215"
    },
    OSC_BROADCAST_ERROR_TWITCH_BROADCAST: {
      name: "OscBroadcastErrorBroadcast",
      type: "TarCon_ClickInfo",
      id: 216,
      enumId: "216"
    },
    OSC_BROADCAST_ERROR_DISCONNECT: {
      name: "OscBroadcastErrorDisconnect",
      type: "TarCon_ClickInfo",
      id: 217,
      enumId: "217"
    },
    OSC_CAPTURE_DVR: {
      name: "OscCaptureDvr",
      type: "TarCon_ClickInfo",
      id: 218,
      enumId: "218"
    },
    OSC_CAPTURE_DVR_SESSION_DURATION_FS: {
      name: "OscCaptureDvrSessionDurationFS",
      type: "TarCon_PresentationInfo",
      id: 219,
      enumId: "219"
    },
    OSC_CAPTURE_DVR_SESSION_DURATION_DT: {
      name: "OscCaptureDvrSessionDurationDT",
      type: "TarCon_PresentationInfo",
      id: 220,
      enumId: "220"
    },
    OSC_CAPTURE_DVR_CUSTOMIZE: {
      name: "OscCaptureDvrCustomize",
      type: "TarCon_ClickInfo",
      id: 221,
      enumId: "221"
    },
    OSC_CAPTURE_DVR_MICROPHONE: {
      name: "OscCaptureDvrMicrophone",
      type: "TarCon_ClickInfo",
      id: 222,
      enumId: "222"
    },
    OSC_CAPTURE_MANUAL: {
      name: "OscCaptureManual",
      type: "TarCon_ClickInfo",
      id: 223,
      enumId: "223"
    },
    OSC_CAPTURE_MANUAL_SESSION_DURATION_FS: {
      name: "OscCaptureManualSessionDurationFS",
      type: "TarCon_PresentationInfo",
      id: 224,
      enumId: "224"
    },
    OSC_CAPTURE_MANUAL_SESSION_DURATION_DT: {
      name: "OscCaptureManualSessionDurationDT",
      type: "TarCon_PresentationInfo",
      id: 225,
      enumId: "225"
    },
    OSC_CAPTURE_MANUAL_CUSTOMIZE: {
      name: "OscCaptureManualCustomize",
      type: "TarCon_ClickInfo",
      id: 226,
      enumId: "226"
    },
    OSC_CAPTURE_MANUAL_MICROPHONE: {
      name: "OscCaptureManualMicrophone",
      type: "TarCon_ClickInfo",
      id: 227,
      enumId: "227"
    },
    OSC_CAPTURE_SP_ERROR_CRASH: {
      name: "OscCaptureSPErrorCrash",
      type: "TarCon_ClickInfo",
      id: 228,
      enumId: "228"
    },
    OSC_CAPTURE_SP_ERROR_RED_SLASH: {
      name: "OscCaptureSPErrorRedslash",
      type: "TarCon_ClickInfo",
      id: 229,
      enumId: "229"
    },
    OSC_GAMESHARE_INVITE_COPY: {
      name: "OscGameshareInviteCopy",
      type: "TarCon_ClickInfo",
      id: 230,
      enumId: "230"
    },
    OSC_GAMESHARE_INVITE_EMAIL: {
      name: "OscGameshareInviteEmail",
      type: "TarCon_ClickInfo",
      id: 231,
      enumId: "231"
    },
    OSC_GAMESHARE_STARTED: {
      name: "OscGameshareStarted",
      type: "TarCon_ClickInfo",
      id: 232,
      enumId: "232"
    },
    OSC_GAMESHARE_PAUSED: {
      name: "OscGamesharePaused",
      type: "TarCon_ClickInfo",
      id: 233,
      enumId: "233"
    },
    OSC_GAMESHARE_STOPPED: {
      name: "OscGameshareStopped",
      type: "TarCon_ClickInfo",
      id: 234,
      enumId: "234"
    },
    OSC_GAMESHARE_ERROR_CRASH: {
      name: "OscGameshareErrorCrash",
      type: "TarCon_ClickInfo",
      id: 235,
      enumId: "235"
    },
    OSC_GAMESHARE_ERROR_CANNOT_START: {
      name: "OscGameshareErrorCannotStart",
      type: "TarCon_ClickInfo",
      id: 236,
      enumId: "236"
    },
    OSC_PREFERENCES_NOTIFICATION_SETTINGS: {
      name: "OscPreferencesNotificationSettings",
      type: "TarCon_ClickInfo",
      id: 237,
      enumId: "237"
    },
    OSC_GAMESHARE_ERROR_NO_AUDIO: {
      name: "OscGameshareErrorNoAudio",
      type: "TarCon_ClickInfo",
      id: 238,
      enumId: "238"
    },
    OSC_GAMESHARE_ERROR_NO_VIDEO: {
      name: "OscGameshareErrorNoVideo",
      type: "TarCon_ClickInfo",
      id: 239,
      enumId: "239"
    },
    OSC_GAMESHARE_ERROR_SESSION_LIMIT: {
      name: "OscGameshareErrorSessionLimit",
      type: "TarCon_ClickInfo",
      id: 240,
      enumId: "240"
    },
    OSC_GAMESHARE_ERROR_SESSION_HANG: {
      name: "OscGameshareErrorSessionHang",
      type: "TarCon_ClickInfo",
      id: 241,
      enumId: "241"
    },
    OSC_GAMESHARE_ERROR_DISCONNECT: {
      name: "OscGameshareErrorDisconnect",
      type: "TarCon_ClickInfo",
      id: 242,
      enumId: "242"
    },
    OSC_LAUNCH: {
      name: "OscLaunch",
      type: "TarCon_ClickInfo",
      id: 243,
      enumId: "243"
    },
    OSC_EXIT: {
      name: "OscExit",
      type: "TarCon_ClickInfo",
      id: 244,
      enumId: "244"
    },
    OSC_GAMESHARE_FEEDBACK_RATING: {
      name: "OSCGameshareFeedbackRating",
      type: "TarCon_ClickInfo",
      id: 245,
      enumId: "245"
    },
    OSC_GAMESHARE_FEEDBACK_DETAILS: {
      name: "OSCGameshareFeedbackDetails",
      type: "TarCon_ClickInfo",
      id: 246,
      enumId: "246"
    },
    OSC_GAMESHARE_FEEDBACK_COMMENTS: {
      name: "OSCGameshareFeedbackComments",
      type: "TarCon_ClickInfo",
      id: 247,
      enumId: "247"
    },
    OSC_GALLERY_OPEN: {
      name: "OscGalleryOpen",
      type: "TarCon_ClickInfo",
      id: 248,
      enumId: "248"
    },
    OSC_GALLERY_FOLDER_OPEN: {
      name: "OscGalleryFolderOpen",
      type: "TarCon_ClickInfo",
      id: 249,
      enumId: "249"
    },
    OSC_GALLERY_FILE_OPEN: {
      name: "OscGalleryFileOpen",
      type: "TarCon_ClickInfo",
      id: 250,
      enumId: "250"
    },
    OSC_GALLERY_FILE_REMOVE: {
      name: "OscGalleryFileRemove",
      type: "TarCon_ClickInfo",
      id: 251,
      enumId: "251"
    },
    OSC_GALLERY_FILE_REMOVE_ERROR: {
      name: "OscGalleryFileRemoveError",
      type: "TarCon_ClickInfo",
      id: 252,
      enumId: "252"
    },
    OSC_VIDEO_TRIM: {
      name: "OscVideoTrim",
      type: "TarCon_ClickInfo",
      id: 253,
      enumId: "253"
    },
    OSC_VIDEO_UPLOAD_SCREEN_OPENED: {
      name: "OscVideoUpload",
      type: "TarCon_ClickInfo",
      id: 254,
      enumId: "254"
    },
    OSC_VIDEO_UPLOAD_ERROR: {
      name: "OscVideoUploadError",
      type: "TarCon_ClickInfo",
      id: 255,
      enumId: "255"
    },
    OSC_IMAGE_UPLOAD_DURATION: {
      name: "OscImageUploadDuration",
      type: "TarCon_PresentationInfo",
      id: 256,
      enumId: "256"
    },
    OSC_IMAGE_UPLOAD_ERROR: {
      name: "OscImageUploadError",
      type: "TarCon_ClickInfo",
      id: 257,
      enumId: "257"
    },
    OSC_VIDEO_TRIM_DURATION: {
      name: "OscVideoTrimDuration",
      type: "TarCon_PresentationInfo",
      id: 258,
      enumId: "258"
    },
    OSC_VIDEO_UPLOAD_DURATION: {
      name: "OscVideoUploadDuration",
      type: "TarCon_PresentationInfo",
      id: 259,
      enumId: "259"
    },
    OSC_IMAGE_EDIT: {
      name: "OscImageEdit",
      type: "TarCon_ClickInfo",
      id: 260,
      enumId: "260"
    },
    OSC_IMAGE_WATERMARK_STAT: {
      name: "OscImageWatermarkStat",
      type: "TarCon_ClickInfo",
      id: 261,
      enumId: "261"
    },
    OSC_PREFERENCES_OPEN: {
      name: "OscPreferencesOpen",
      type: "TarCon_ClickInfo",
      id: 262,
      enumId: "262"
    },
    OSC_UNHANDLED_EXCEPTION: {
      name: "OscUnhandledException",
      type: "TarCon_ClickInfo",
      id: 263,
      enumId: "263",
      immediateRequest: !0
    },
    OSC_CAPTURE_SCREENSHOT: {
      name: "OscCaptureScreenshot",
      type: "TarCon_ClickInfo",
      id: 264,
      enumId: "264"
    },
    OSC_IMAGE_UPLOAD_SCREEN_OPENED: {
      name: "OscImageUpload",
      type: "TarCon_ClickInfo",
      id: 265,
      enumId: "265"
    },
    OSC_BROADCAST_YOUTUBE: {
      name: "OscBroadcastYoutube",
      type: "TarCon_ClickInfo",
      id: 266,
      enumId: "266"
    },
    OSC_BROADCAST_YOUTUBE_ERROR_BROADCAST: {
      name: "OscBroadcastYoutube",
      type: "TarCon_ClickInfo",
      id: 267,
      enumId: "267"
    },
    OSC_BROADCAST_YOUTUBE_CAMERA: {
      name: "OscBroadcastYoutubeCamera",
      type: "TarCon_ClickInfo",
      id: 268,
      enumId: "268"
    },
    OSC_BROADCAST_YOUTUBE_MICROPHONE: {
      name: "OscBroadcastYoutubeMicrophone",
      type: "TarCon_ClickInfo",
      id: 269,
      enumId: "269"
    },
    OSC_BROADCAST_YOUTUBE_CUSTOMIZE: {
      name: "OscBroadcastYoutubeCustomize",
      type: "TarCon_ClickInfo",
      id: 270,
      enumId: "270"
    },
    OSC_BROADCAST_TWITCH_CUSTOM_OVERLAY: {
      name: "OscBroadcastTwitchCustomOverlay",
      type: "TarCon_ClickInfo",
      id: 271,
      enumId: "271"
    },
    OSC_BROADCAST_YOUTUBE_CUSTOM_OVERLAY: {
      name: "OscBroadcastYoutubeCustomOverlay",
      type: "TarCon_ClickInfo",
      id: 272,
      enumId: "272"
    },
    OSC_BROADCAST_TWITCH_QUALITY: {
      name: "OscBroadcastTwitchQuality",
      type: "TarCon_ClickInfo",
      id: 273,
      enumId: "273"
    },
    OSC_BROADCAST_YOUTUBE_QUALITY: {
      name: "OscBroadcastYoutubeQuality",
      type: "TarCon_ClickInfo",
      id: 274,
      enumId: "274"
    },
    OSC_BROADCAST_TWITCH_VIEWER_COUNT: {
      name: "OscBroadcastTwitchViewerCount",
      type: "TarCon_ClickInfo",
      id: 275,
      enumId: "275"
    },
    OSC_BROADCAST_YOUTUBE_VIEWER_COUNT: {
      name: "OscBroadcastYoutubeViewerCount",
      type: "TarCon_ClickInfo",
      id: 276,
      enumId: "276"
    },
    OSC_BROADCAST_YOUTUBE_SESSION_DURATION_DT: {
      name: "OscBroadcastYoutubeSessionDurationDT",
      type: "TarCon_PresentationInfo",
      id: 279,
      enumId: "279"
    },
    OSC_BROADCAST_YOUTUBE_SESSION_DURATION_FS: {
      name: "OscBroadcastYoutubeSessionDurationFS",
      type: "TarCon_PresentationInfo",
      id: 280,
      enumId: "280"
    },
    OSC_BROADCAST_AUTO_ADAPT_TRIGGER: {
      name: "OscBroadcastAutoAdaptTrigger",
      type: "TarCon_ClickInfo",
      id: 281,
      enumId: "281"
    },
    OSC_GALLERY_LOAD_TIME_MAIN: {
      name: "OscGalleryLoadTimeMain",
      type: "TarCon_PresentationInfo",
      id: 282,
      enumId: "282"
    },
    OSC_GALLERY_LOAD_TIME_FOLDER: {
      name: "OscGalleryLoadTimeFolder",
      type: "TarCon_PresentationInfo",
      id: 283,
      enumId: "283"
    },
    OSC_UPLOAD_ATTEMPT: {
      name: "OscUploadAttempt",
      type: "TarCon_ClickInfo",
      id: 284,
      enumId: "284"
    },
    OSC_BROADCAST_STOPPED_AUTO_ADAPT: {
      name: "OscBroadcastStoppedAutoAdapt",
      type: "TarCon_ClickInfo",
      id: 285,
      enumId: "285"
    },
    OSC_BROADCAST_FACEBOOK_CUSTOM_OVERLAY: {
      name: "OscBroadcastFacebookCustomOverlay",
      type: "TarCon_ClickInfo",
      id: 290,
      enumId: "290"
    },
    OSC_BROADCAST_CUSTOMIZE: {
      name: "OscBroadcastCutomize",
      type: "Osc_Provider_Settings_Event",
      id: "OSC_BROADCAST_CUSTOMIZE",
      enumId: "OSC_BROADCAST_CUSTOMIZE"
    },
    OSC_CAPTURE_CUSTOMIZE: {
      name: "OscBroadcastCutomize",
      type: "Osc_Provider_Settings_Event",
      id: "OSC_BROADCAST_CUSTOMIZE",
      enumId: "OSC_BROADCAST_CUSTOMIZE"
    },
    OSC_BROADCAST_END: {
      name: "OscBroadcastEnd",
      type: "Osc_Broadcast_End_Data",
      id: "OSC_BROADCAST_END",
      enumId: "OSC_BROADCAST_END"
    },
    OSC_SESSION: {
      name: "OscSession",
      type: "Osc_Session",
      id: "OSC_BROADCAST_SESSION",
      enumId: "OSC_BROADCAST_SESSION"
    },
    OSC_BROADCAST_ERROR: {
      name: "OscBroadcastError",
      type: "Osc_Broadcast_Error",
      id: "OSC_BROADCAST_QUALITY",
      enumId: "OSC_BROADCAST_QUALITY"
    },
    OSC_PREFERENCES_NOTIFICATIONS_CHANGED: {
      name: "OscPreferencesNotificationsChanged",
      type: "Osc_Preferences_Notifications_Changed",
      id: "OSC_PREFERENCES_NOTIFICATIONS_CHANGED",
      enumId: "OSC_PREFERENCES_NOTIFICATIONS_CHANGED"
    },
    OSC_ANSEL_NAVIGATION: {
      name: "OscAnselNavigation",
      type: "Osc_Ansel_Ui_Session_Navigation_Type",
      id: "OSC_ANSEL_NAVIGATION",
      enumId: "OSC_ANSEL_NAVIGATION"
    },
    OSC_ANSEL_FILTER_SELECTION: {
      name: "OscAnselFilterSelection",
      type: "Osc_Ansel_Filter",
      id: "OSC_ANSEL_FILTER_SELECTION",
      enumId: "OSC_ANSEL_FILTER_SELECTION"
    },
    OSC_ANSEL_SCREENSHOT_COMPLETED: {
      name: "OscAnselScreenShotCompleted",
      type: "Osc_Ansel_Screenshot_Taken",
      id: "OSC_ANSEL_SCREENSHOT_COMPLETED",
      enumId: "OSC_ANSEL_SCREENSHOT_COMPLETED"
    },
    OSC_ANSEL_TYPE: {
      name: "OscAnselType",
      type: "Osc_Ansel_Type",
      id: "OSC_ANSEL_SCREENSHOT_STARTED",
      enumId: "OSC_ANSEL_SCREENSHOT_STARTED"
    },
    OSC_ANSEL_ERROR: {
      name: "OscPreferencesNotificationsChanged",
      type: "Osc_Ansel_Error",
      id: "OSC_ANSEL_ERROR",
      enumId: "OSC_ANSEL_ERROR"
    },
    OSC_FREESTYLE_FILTERS_APPLIED: {
      name: "OscFreeStyleFiltersApplied",
      type: "Osc_FreeStyle_Filters_Applied",
      id: "OSC_FREESTYLE_FILTERS_APPLIED",
      enumId: "OSC_FREESTYLE_FILTERS_APPLIED"
    },
    OSC_FREESTYLE_FILTERS_ADDED: {
      name: "OscFreeStyleFiltersAdded",
      type: "Osc_FreeStyle_Filters_Added",
      enumId: "OSC_FREESTYLE_FILTERS_ADDED"
    },
    OSC_FREESTYLE_FILTERS_SLOT_CHANGED: {
      name: "OscFreeStyleFiltersSlotChanged",
      type: "Osc_FreeStyle_Filters_Slot_Changed",
      enumId: "OSC_FREESTYLE_FILTERS_SLOT_CHANGED"
    },
    OSC_ANSEL_FILTER_CONTROL_SETTINGS: {
      name: "OscAnselFilterControlSettings",
      type: "Osc_Ansel_Filter_Control_Settings",
      enumId: "OSC_ANSEL_FILTER_CONTROL_SETTINGS"
    },
    OSC_MENU_LAUNCH: {
      name: "OscMenuLaunch",
      type: "Osc_Menu_Launch",
      enumId: "OSC_MENU_LAUNCH"
    },
    OSC_HOTKEY_SETTINGS_EVENT: {
      name: "OscHotkeySettingsEvent",
      type: "Osc_Hotkey_Settings_Event",
      enumId: "OSC_HOTKEY_SETTINGS_EVENT"
    },
    OSC_SETTINGS_EVENT: {
      name: "OscSettingsEvent",
      type: "Osc_Settings_Event",
      enumId: "OSC_SETTINGS_EVENT"
    },
    OSC_AUTOMATED_UI_PERF: {
      type: "Osc_Automated_UI_Perf",
      enumId: "OSC_AUTOMATED_UI_PERF"
    },
    OSC_AUTOMATED_GALLERY_PERF: {
      type: "Osc_Automated_Gallery_Perf",
      enumId: "OSC_AUTOMATED_GALLERY_PERF"
    },
    OSC_UPLOAD_DATA: {
      name: "OscUploadData",
      type: "Osc_Upload_Data",
      id: 0,
      enumId: "OSC_UPLOAD_DATA",
      allowMultiple: !0
    },
    OSC_LOGIN: {
      name: "OscLogin",
      type: "Osc_Login",
      id: "OSC_LOGIN",
      enumId: "OSC_LOGIN"
    },
    OSC_HIGHLIGHT_EVENT: {
      name: "OscHighlightEvent",
      type: "Osc_Highlight_Event_V2",
      enumId: "OSC_HIGHLIGHT_EVENT"
    },
    OSC_MTA_SETTINGS: {
      name: "OscMtaSettings",
      type: "Osc_Mta_Settings",
      enumId: "OSC_MTA_SETTINGS"
    },
    OSC_HIGHLIGHTS_OPENED: {
      name: "OscHighlightsOpened",
      type: "Osc_Highlights_Opened",
      enumId: "OSC_HIGHLIGHTS_OPENED"
    },
    OSC_HIGHLIGHTS_INDIVIDUAL_TOGGLE: {
      name: "OscHighlightsIndividualToggle",
      type: "Osc_Highlights_Individual_Toggle",
      enumId: "EVENTS_OSC_HIGHLIGHTS_INDIVIDUAL_TOGGLE"
    },
    OSC_HIGHLIGHTS_GAME_TOGGLE: {
      name: "OscHighlightsGameToggle",
      type: "Osc_Highlights_Game_Toggle",
      enumId: "OSC_HIGHLIGHTS_GAME_TOGGLE"
    },
    OSC_HIGHLIGHTS_DISC_SPACE_SETTING: {
      name: "OscHighlightsDiscSpaceSetting",
      type: "Osc_Highlights_Disc_Space_Setting",
      enumId: "OSC_HIGHLIGHTS_DISC_SPACE_SETTING"
    },
    OSC_HIGHLIGHT_ERROR: {
      name: "OscHighlightError",
      type: "Osc_Highlight_Error",
      enumId: "OSC_HIGHLIGHT_ERROR"
    },
    OSC_HIGHLIGHT_CANCELED: {
      name: "OscHighlightCanceled",
      type: "Osc_Highlight_Canceled",
      enumId: "OSC_HIGHLIGHT_CANCELED"
    },
    OSC_CAPTURE_EVENT: {
      name: "OscCaptureEvent",
      type: "Osc_Capture_Event",
      enumId: "OSC_CAPTURE_EVENT"
    },
    OSC_SHADOWPLAY_CAPTURE_ERROR: {
      name: "OscShadowPlayCaptureError",
      type: "Osc_ShadowPlay_Capture_Error",
      enumId: "OSC_SHADOWPLAY_CAPTURE_ERROR"
    },
    OSC_TRIM_GIF: {
      name: "OscTrimGif",
      type: "Osc_Provider_Settings_Event",
      enumId: "OSC_TRIM_GIF"
    },
    OSC_GIF_CONVERSION_ATTEMPT: {
      name: "OscGifConversionAttempt",
      type: "Osc_Gif_Conversion_Attempt",
      enumId: "OSC_GIF_CONVERSION_ATTEMPT",
      allowMultiple: !0
    },
    START_EXPERIMENT: {
      name: "StartExperienceControlExperiment",
      type: "ExperienceControlInfo",
      enumId: "START_EXPERIMENT"
    },
    STOP_EXPERIMENT: {
      name: "StopExperienceControlExperiment",
      type: "ExperienceControlInfo",
      enumId: "STOP_EXPERIMENT"
    },
    OSC_ANSEL_LITE_SCREENSHOT_TAKEN: {
      name: "OscAnselLiteScreenshotTaken",
      type: "Osc_Ansel_Lite_Screenshot_Taken",
      enumId: "OSC_ANSEL_LITE_SCREENSHOT_TAKEN"
    },
    OSC_ANSEL_SCREENSHOT_STARTED: {
      name: "OscAnselScreenshotStarted",
      type: "Osc_Ansel_Screenshot_Started",
      enumId: "OSC_ANSEL_SCREENSHOT_STARTED"
    },
    OSC_ANSEL_SCREENSHOT_CANCELLED: {
      name: "OscAnselScreenshotCancelled",
      type: "Osc_Ansel_Screenshot_Cancelled",
      enumId: "OSC_ANSEL_SCREENSHOT_CANCELLED"
    },
    OSC_ANSEL_SCREENSHOT_FAILED: {
      name: "OscAnselScreenshotFailed",
      type: "Osc_Ansel_Screenshot_Failed",
      enumId: "OSC_ANSEL_SCREENSHOT_FAILED"
    },
    OSC_PERFORMANCE_TOOL_SIDEBAR_SESSION: {
      name: "OscPerformanceToolSidebarSession",
      type: "Osc_Performance_Tool_Sidebar_Session",
      enumId: "OSC_PERFORMANCE_TOOL_SIDEBAR_SESSION"
    },
    OSC_PERFORMANCE_TOOL_OVERLAY_SESSION: {
      name: "OscPerformanceToolOverlaySession",
      type: "Osc_Performance_Tool_Overlay_Session",
      enumId: "OSC_PERFORMANCE_TOOL_OVERLAY_SESSION"
    },
    OSC_PERFORMANCE_TOOL_LAST_SCAN_RESULTS: {
      name: "OscPerformanceToolLastScanResults",
      type: "Osc_Performance_Tool_Last_Scan_Results",
      enumId: "OSC_PERFORMANCE_TOOL_LAST_SCAN_RESULTS"
    },
    OSC_PERFORMANCE_TOOL_LATENCY_METRICS: {
      name: "OscPerformanceToolLatencyMetrics",
      type: "Osc_Performance_Tool_Latency_Metrics",
      enumId: "OSC_PERFORMANCE_TOOL_LATENCY_METRICS"
    },
    OSC_PERFORMANCE_TOOL_ERROR: {
      name: "OscPerformanceToolError",
      type: "Osc_Performance_Tool_Error",
      enumId: "OSC_PERFORMANCE_TOOL_ERROR"
    },
    OSC_PERFORMANCE_TOOL_SAMPLE_SIZE: {
      name: "OscPerformanceToolSampleSize",
      type: "Osc_Performance_Tool_Sample_Size",
      enumId: "OSC_PERFORMANCE_TOOL_SAMPLE_SIZE"
    },
    OSC_PERFORMANCE_TOOL_RESET_AVERAGE: {
      name: "OscPerformanceToolResetAverage",
      type: "Osc_Performance_Tool_Reset_Average",
      enumId: "OSC_PERFORMANCE_TOOL_RESET_AVERAGE"
    },
    OSC_PERFORMANCE_TOOL_LOGGING_SESSION: {
      name: "OscPerformanceToolLoggingSession",
      type: "Osc_Performance_Tool_Logging_Session",
      enumId: "OSC_PERFORMANCE_TOOL_LOGGING_SESSION"
    },
    OSC_PERFORMANCE_TOOL_SETTINGS: {
      name: "OscPerformanceToolSettings",
      type: "Osc_Performance_Tool_Settings",
      enumId: "OSC_PERFORMANCE_TOOL_SETTINGS"
    }
  }), n.constant("TELEMETRY_OSC_CAPTURE_TYPE", {
    manualRecord: "manualCapture",
    instantReplay: "irCapture"
  }), n.constant("TELEMETRY_OSC_TOGGLE_STATE", {
    on: "On",
    off: "Off"
  }), n.constant("TELEMETRY_OSC_BOOLEAN_STATUS", {
    TRUE: "Yes",
    FALSE: "No"
  }), n.constant("TELEMETRY_OSC_AVAILABLE_STATUS", {
    yes: "Yes",
    no: "No"
  }), n.constant("OSC_SETTINGS_CONTROL_TYPES", {
    globalonoff: "GlobalOnOff"
  }), n.constant("TELEMETRY_OSC_FILTER_CONTROL_TYPES", {
    slider: "OSC_ANSEL_FILTER_CONTROL_TYPE_SLIDER",
    boolean: "OSC_ANSEL_FILTER_CONTROL_TYPE_BOOLEAN",
    vector: "OSC_ANSEL_FILTER_CONTROL_TYPE_VECTOR",
    unknown: "OSC_ANSEL_FILTER_CONTROL_TYPE_UNKNOWN"
  }), n.constant("TELEMETRY_OSC_TRIGGER_MODE", {
    hotkey: "Hkey",
    ui: "Ui"
  }), n.constant("TELEMETRY_OSC_MENU_TYPE", {
    mainMenu: "MainMenu",
    ansel: "Ansel",
    freestyle: "FreeStyle",
    ansellite: "AnselLite"
  }), n.constant("TELEMETRY_OSC_SCREEN_STATE", {
    fullscreen: "Fullscreen",
    desktop: "Desktop"
  }), n.constant("TELEMETRY_OSC_QUALITY_SETTING", {
    average: "Average",
    good: "Good",
    veryGood: "VeryGood",
    ultraGood: "UltraGood",
    custom: "Custom"
  }), n.constant("TELEMETRY_OSC_MIC_MODE", {
    ptt: "ptt",
    alwayson: "alwayson",
    off: "off"
  }), n.constant("TELEMETRY_OSC_ANSEL_TYPE", {
    full: "OSC_ANSEL_FULL_INTEGRATION",
    filterOnly: "OSC_ANSEL_FILTER_ONLY"
  }), n.constant("TELEMETRY_OSC_ANSEL_FAILURE_TYPE", {
    noResponse: "OSC_ANSEL_NO_RESPONSE",
    generalFailure: "OSC_ANSEL_GENERAL_FAILURE",
    failedToStart: "OSC_ANSEL_FAILED_TO_START",
    noSpaceLeft: "OSC_ANSEL_NO_SPACE_LEFT",
    permissionDenied: "OSC_ANSEL_PERMISSION_DENIED",
    invalidRequest: "OSC_ANSEL_INVALID_REQUEST",
    failedToProcess: "OSC_ANSEL_FAILED_TO_PROCESS",
    processDeclined: "OSC_ANSEL_PROCESS_DECLINED",
    alreadyEnabled: "OSC_ANSEL_ALREADY_ENABLED",
    alreadyDisabled: "OSC_ANSEL_ALREADY_DISABLED",
    outOfRange: "OSC_ANSEL_OUT_OF_RANGE",
    alreadySet: "OSC_ANSEL_ALREADY_SET",
    incompatibleVersion: "OSC_ANSEL_INCOMPATIBLE_VERSION",
    appFatalError: "OSC_ANSEL_APP_FATAL_ERROR",
    appNonFatalError: "OSC_ANSEL_APP_NON_FATAL_ERROR",
    appNotSupported: "OSC_ANSEL_UNSUPPORTED_GAME_ERROR",
    anselUILaunchFailure: "OSC_ANSEL_MENU_LAUNCH_ERROR"
  }), n.constant("TELEMETRY_OSC_SCREENSHOT_FAILURE_TYPE", {
    ngxFeatureNotSupported: "NVSDK_NGX_RESULT_FAIL_FEATURE_NOT_SUPPORTED",
    ngxOutOfDate: "NVSDK_NGX_RESULT_FAIL_OUT_OF_DATE",
    ngxOutOfGpuMemory: "NVSDK_NGX_RESULT_FAIL_OUT_OF_GPU_MEMORY",
    timeoutFailure: "OSC_ANSEL_SCREENSHOT_TIMEOUT_FAILURE",
    generalFailure: "OSC_ANSEL_SCREENSHOT_GENERAL_FAILURE"
  }), n.constant("TELEMETRY_OSC_ANSEL_GAMEPAD_TYPE", {
    None: "None",
    XBoxOne: "XBox_One",
    XBox360: "XBox_360",
    SonyPS4: "Sony_PS4",
    NvidiaShield: "Nvidia_Shield",
    Others: "Others"
  }), n.constant("TELEMETRY_OSC_VALID_PROVIDERS", ["none", "google", "imgur", "twitch", "instagram",
    "facebook", "youku", "iqiyi", "tencent", "manualCapture", "irCapture", "sina"
  ]), n.constant("TELEMETRY_OSC_PERF_ID", {
    launchCold: "LAUNCH_COLD",
    launchWarm: "LAUNCH_WARM",
    preferencesScreen: "PREFERENCES_SCREEN",
    uploadScreen: "UPLOAD_SCREEN",
    providerTrigger: "PROVIDER_TRIGGER",
    customizeScreen: "CUSTOMIZE_SCREEN",
    filePickerChangeFolder: "FILE_PICKER_CHANGE_FOLDER",
    providerStop: "PROVIDER_STOP",
    fpsOverlayOn: "FPS_OVERLAY_ON",
    fpsOverlayOff: "FPS_OVERLAY_OFF",
    providerPause: "PROVIDER_PAUSE",
    providerResume: "PROVIDER_RESUME",
    customOverlaySwitch: "CUSTOM_OVERLAY_SWITCH",
    cameraPreviewToggle: "CAMERA_PREVIEW_ON",
    galleryPopulateFolders: "GALLERY_POPULATE_FOLDERS",
    galleryPopulateFiles: "GALLERY_POPULATE_FILES",
    galleryFirstScreen: "FIRST_SCREEN",
    galleryCached: "CACHED",
    galleryFullLoad: "FULL_LOAD",
    screenshot: "SCREENSHOT",
    closeOSC: "CLOSE_OSC",
    anselBringup: "ANSEL_BRINGUP",
    keyboardShortcutScreen: "KEYBOARD_SHORTCUT_SCREEN"
  }), n.constant("TELEMETRY_CAPTURE_ACTION", {
    startManualCapture: "START_MANUAL_CAPTURE",
    saveManualCapture: "SAVE_MANUAL_CAPTURE",
    turnOnInstantReplay: "TURN_ON_INSTANT_REPLAY",
    detectValidIRGame: "DETECT_VALID_I_R_GAME",
    saveInstantReplay: "SAVE_INSTANT_REPLAY",
    turnOffInstantReplay: "TURN_OFF_INSTANT_REPLAY",
    takeScreenshot: "TAKE_SCREENSHOT"
  }), n.constant("TELEMETRY_OVERLAY_LOCATION", {
    nowhere: "Nowhere",
    leftTop: "LeftTop",
    rightTop: "RightTop",
    leftBottom: "LeftBottom",
    rightBottom: "RightBottom",
    na: "NA"
  }), n.constant("OVERCLOCKING_FEATURE", {
    performanceMonitoring: "PerfMon",
    manualOverclocking: "ManualOC",
    automaticOverclocking: "AutoOC",
    gameReadyDriverOverclocking: "GRDOC"
  }), n.constant("DISCLAIMER_OPTIONS", {
    notPresented: "NotPresented",
    agreed: "Agreed",
    cancelled: "Cancelled",
    uacCancelled: "UacCancelled"
  }), n.constant("SYSTEM_TYPE", {
    desktop: "Desktop",
    laptop: "Laptop"
  }), n.constant("OVERLAY_VIEW", {
    off: "Off",
    fps: "Fps",
    basic: "Basic",
    advanced: "Advanced",
    latency: "Latency",
    reflexAnalyzer: "ReflexAnalyzer"
  }), n.constant("SCAN_TYPE", {
    background: "Background",
    manual: "Manual"
  }), n.constant("MODULE", {
    plugin: "Plugin",
    osc: "Osc",
    others: "Others"
  }), exports.ngTelemetryModule = n;
}
