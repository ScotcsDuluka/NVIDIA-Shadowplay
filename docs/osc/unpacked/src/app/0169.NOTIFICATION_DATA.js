// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 169
// constant NOTIFICATION_DATA
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.NOTIFICATION_DATA = void 0;
  var o = require(1) /* app/1 — main (module) */,
    r = require(307),
    a = i(r),
    l = o.ngMainModule.constant("NOTIFICATION_DATA", [{
      id: "OPEN_SHARE",
      img: a.default,
      message: "l10n.notificationOpenShare"
    }, {
      id: "INSTANT_REPLAY_STARTED",
      icon: "icon-replay icon-highlighted",
      message: "l10n.notificationInstantReplayStarted"
    }, {
      id: "INSTANT_REPLAY_STOPPED",
      icon: "icon-replay icon-normal",
      message: "l10n.notificationInstantReplayStopped"
    }, {
      id: "INSTANT_REPLAY_SAVED",
      icon: "icon-replay icon-highlighted",
      message: "l10n.notificationInstantReplaySaved"
    }, {
      id: "INSTANT_REPLAY_SAVED_TO_GALLERY",
      icon: "icon-replay icon-highlighted",
      message: "l10n.notificationInstantReplaySavedToGallery"
    }, {
      id: "RECORD_STARTED",
      icon: "icon-record icon-highlighted",
      message: "l10n.notificationManualRecordStarted"
    }, {
      id: "RECORD_STOPPED",
      icon: "icon-record icon-normal",
      message: "l10n.notificationManualRecordStopped"
    }, {
      id: "RECORD_STOPPED_AND_SAVED_TO_GALLERY",
      icon: "icon-record icon-normal",
      message: "l10n.notificationManualRecordStoppedAndSavedToGallery"
    }, {
      id: "BROADCAST_STARTED",
      icon: "icon-broadcast icon-highlighted",
      message: "l10n.notificationBroadcastStarted"
    }, {
      id: "BROADCAST_STOPPED",
      icon: "icon-broadcast icon-normal",
      message: "l10n.notificationBroadcastStopped"
    }, {
      id: "BROADCAST_STREAMER_STOPPED",
      icon: "assets/img/btn_broadcast_white_120x120.png",
      message: "l10n.notificationBroadcastStoppedByStreamer"
    }, {
      id: "BROADCAST_PAUSED",
      icon: "icon-broadcast icon-highlighted",
      message: "l10n.notificationBroadcastPaused"
    }, {
      id: "BROADCAST_RESUMED",
      icon: "icon-broadcast icon-highlighted",
      message: "l10n.notificationBroadcastResumed"
    }, {
      id: "BROADCAST_LOGIN",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationBroadcastLogIn"
    }, {
      id: "BROADCAST_YOUTUBE_LIVE_STREAMING",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationBroadcastEnableStreaming"
    }, {
      id: "BROADCAST_FAILED",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationBroadcastFailed"
    }, {
      id: "BROADCAST_TIMEOUT_N_MINUTES",
      icon: "icon-broadcast icon-highlighted",
      message: "l10n.notificationBroadcastDurationLeft"
    }, {
      id: "BROADCAST_TIMEOUT_1_MINUTE",
      icon: "icon-broadcast icon-highlighted",
      message: "l10n.notificationBroadcast1MinLeft"
    }, {
      id: "BROADCAST_ENDED_TIMEOUT",
      icon: "icon-broadcast icon-normal",
      message: "l10n.notificationBroadcastDurationEnds"
    }, {
      id: "CUSTOMOVERLAY_NOT_ASSIGNED",
      icon: "icon-broadcast icon-normal",
      message: "l10n.notificationCustomOverlayFileNotFound"
    }, {
      id: "CUSTOMOVERLAY_TURNED_OFF",
      icon: "icon-broadcast icon-normal",
      message: "l10n.notificationCustomOverlayTurnedOff"
    }, {
      id: "CUSTOMOVERLAY_ALL_SLOT_EMPTY",
      icon: "icon-broadcast icon-normal",
      message: "l10n.notificationCustomOverlayAllSlotEmpty"
    }, {
      id: "SCREENSHOT_SAVED",
      icon: "icon-gallery icon-normal",
      message: "l10n.notificationScreenShotSaved"
    }, {
      id: "SCREENSHOT_SAVED_TO_GALLERY",
      icon: "icon-gallery icon-normal",
      message: "l10n.notificationScreenshotSavedToGallery"
    }, {
      id: "PHOTOGRAPHIC_SCREENSHOT_SAVED_TO_GALLERY",
      icon: "icon-gallery icon-normal",
      message: "l10n.notificationPhotographicScreenshotSavedToGallery"
    }, {
      id: "WARNING_PHOTOGRAPHY_NOT_ALLOWED",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationWarningPhotographyNotAllowed"
    }, {
      id: "WARNING_FULLSCREEN_GAME_REQUIRED",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationWarningFullscreenGameRequired"
    }, {
      id: "WARNING_BROADCAST_STOP_TO_CUSTOMIZE",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationWarningStopBroadcastingToCustomize"
    }, {
      id: "WARNING_RECORDING_STOP_TO_CUSTOMIZE",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationWarningStopRecordingToCustomize"
    }, {
      id: "WARNING_HIGHLIGHTS_STOP_TO_CUSTOMIZE",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationWarningStopHighlightsToCustomize"
    }, {
      id: "WARNING_BROADCAST_STOP_TO_USE_FEATURE",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationWarningStopBroadcasting"
    }, {
      id: "WARNING_RECORDING_STOP_TO_USE_FEATURE",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationWarningStopRecording"
    }, {
      id: "WARNING_INSTANT_REPLAY_STOP_TO_USE_FEATURE",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationWarningStopInstantReplay"
    }, {
      id: "WARNING_HIGHLIGHTS_STOP_TO_USE_BROADCAST",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationWarningTurnOffHighlights"
    }, {
      id: "WARNING_COPLAY_STOP_TO_USE_FEATURE",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationCoplayStopGameShare"
    }, {
      id: "WARNING_DELETE_FAILED",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationWarningFailedToDeleteNotification"
    }, {
      id: "WARNING_SUPPORTED_GAME_REQUIRED",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationWarningGameRequired"
    }, {
      id: "WARNING_DESKTOP_CAPTURE_DISABLED",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationWarningDesktopCaptureDisabled"
    }, {
      id: "WARNING_NVIDIA_GPU_REQUIRED",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationWarningNvidiaGpuRequired"
    }, {
      id: "WARNING_INTERNET_CONNECTION_REQUIRED",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationCoplayNetworkUnavailable"
    }, {
      id: "COPLAY_5_MINUTES",
      icon: "icon-stream icon-highlighted",
      message: "l10n.notificationCoplay5MinutesLeft"
    }, {
      id: "COPLAY_1_MINUTE",
      icon: "icon-stream icon-highlighted",
      message: "l10n.notificationCoplay1MinuteLeft"
    }, {
      id: "COPLAY_EXPIRED",
      icon: "icon-stream icon-highlighted",
      message: "l10n.notificationCoplayTimeIsUp"
    }, {
      id: "COPLAY_NOW_PLAYING",
      icon: "icon-stream icon-highlighted",
      message: "l10n.notificationCoplayNowPlayingWith"
    }, {
      id: "COPLAY_INVITETO_CANCELLED",
      icon: "icon-stream icon-highlighted",
      message: "l10n.notificationCoplayInvitationToCancelled"
    }, {
      id: "COPLAY_COPYING_INVITE",
      icon: "icon-stream icon-highlighted",
      message: "l10n.notificationCoplayCopyingInvite"
    }, {
      id: "COPLAY_SENDING_INVITE",
      icon: "icon-stream icon-highlighted",
      message: "l10n.notificationCoplaySendingInvite"
    }, {
      id: "COPLAY_PAUSED",
      icon: "icon-stream icon-highlighted",
      message: "l10n.notificationCoplayPausedPlayingWith"
    }, {
      id: "COPLAY_STOPPEDPLAYING",
      icon: "icon-stream icon-highlighted",
      message: "l10n.notificationCoplayStoppedPlayingWith"
    }, {
      id: "COPLAY_DONE",
      icon: "icon-stream icon-highlighted",
      message: "l10n.notificationCoplayDonePlaying"
    }, {
      id: "UPLOAD_SUCCESS",
      icon: "icon-gallery icon-normal",
      message: "l10n.notificationUploadSuccess"
    }, {
      id: "UPLOAD_FAILED",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationUploadFailed"
    }, {
      id: "UPLOAD_STARTED",
      icon: "icon-gallery icon-normal",
      message: "l10n.notificationUploadStarted"
    }, {
      id: "UPLOAD_URL_COPIED",
      icon: "icon-gallery icon-normal",
      message: "l10n.notificationUploadURLCopied"
    }, {
      id: "NO_YOUTUBE_CHANNEL",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationYoutubeUploadNoChannel"
    }, {
      id: "HIGHLIGHTS_SAVED",
      icon: "icon-highlights icon-normal",
      message: "l10n.notificationHighlightsSaved"
    }, {
      id: "HIGHLIGHT_SAVED_TO_GALLERY",
      icon: "icon-highlights icon-normal",
      message: "l10n.notificationHighlightSavedToGallery"
    }, {
      id: "HIGHLIGHT_MANUAL_RECORD_RUNNING",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationHighlightManualRecordRunning"
    }, {
      id: "HIGHLIGHT_BROADCAST_RUNNING",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.notificationHighlightBroadcastRunning"
    }, {
      id: "ANSEL_READY_APP_STARTED",
      icon: "icon-camera icon-normal",
      message: "l10n.notificationAnselReadyAppStarted"
    }, {
      id: "ERROR_NVCAMERA_LAUNCH_FAILED",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.ansel.launchFailed"
    }, {
      id: "ENABLE_MODS",
      icon: "icon-freestyle icon-normal",
      message: "l10n.enableMods"
    }, {
      id: "ENABLE_ANSEL",
      icon: "icon-camera icon-normal",
      message: "l10n.enableAnsel"
    }, {
      id: "ENABLE_ANSEL_EXPERIENTAL",
      icon: "icon-camera icon-normal",
      message: "l10n.ansel.enableExperimental"
    }, {
      id: "FEATURE_UPDATE_DRIVER",
      icon: "icon-camera icon-normal",
      message: "l10n.featureUpdateDriver"
    }, {
      id: "HDR_ERROR_SCREENSHOT",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.ScreenshotHDRError"
    }, {
      id: "HDR_ERROR_RECORD",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.RecordHDRError"
    }, {
      id: "HDR_ERROR_BROADCAST",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.BroadcastHDRError"
    }, {
      id: "HDR_ERROR_HL",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.HighlightsHDRError"
    }, {
      id: "WARNING_NVCAMERA_FILTER_DISPLAY_INFO",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.warningNvcamraFilterDisplayInfo"
    }, {
      id: "WHISPER_MODE_ENABLED",
      img: "wm-on-icon.svg",
      message: "l10n.whisperModeEnabled",
      messageSubtext: "l10n.whisperModeGameRestartNotification"
    }, {
      id: "WHISPER_MODE_DISABLED",
      img: "wm-off-icon.svg",
      message: "l10n.whisperModeDisabled",
      messageSubtext: "l10n.whisperModeGameRestartNotification"
    }, {
      id: "WHISPER_MODE_ENABLED_GAMESTART",
      img: "wm-on-icon.svg",
      message: "l10n.whisperModeEnabled",
      messageSubtext: "l10n.whisperModeSettings"
    }, {
      id: "CONNECT_LOGIN_TO_GFE",
      img: a.default,
      message: "l10n.loginToGeForceExperience"
    }, {
      id: "CONNECT_EXIT_FULL_SCREEN",
      img: a.default,
      message: "l10n.exitFullScreenToLogin"
    }, {
      id: "LOGIN_COMPLETED_WINDOWED_GAME_BROADCAST",
      img: a.default,
      message: "l10n.goToGameLaunchBroadcastMenu"
    }, {
      id: "PERFMON_RECTALIGNMENT_SUPPORT",
      icon: "icon-notify_warning icon-normal",
      message: "l10n.perfmonoc.autoRectAlignmentWarning"
    }, {
      id: "PERFMON_RFI_SUPPORT",
      icon: "icon-audio_mixer icon-normal",
      message: "l10n.perfmonoc.rfiOption"
    }]);
  exports.NOTIFICATION_DATA = l;
}
