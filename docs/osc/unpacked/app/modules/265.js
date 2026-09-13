// ─────────────────────────────────────────────────────────────
// APP MODULE 265
// role       : constant EVENTS_DETAIL_DATA
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var i = n(102);
  i.ngTelemetryModule.constant("EVENTS_DETAIL_DATA", {
    EVENTS_TARCON_CLICK_INFO: {
      name: "TarCon_ClickInfo",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        clickedUrl: ""
      }
    },
    EVENTS_TARCON_PRESENTATION_INFO: {
      name: "TarCon_PresentationInfo",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        totalMs: 0
      }
    },
    EVENTS_OSC_PROVIDER_SETTINGS: {
      name: "Osc_Provider_Settings_Event",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        provider: ""
      }
    },
    EVENTS_OSC_BROADCAST_END_DATA: {
      name: "Osc_Broadcast_End_Data",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        provider: "",
        customOverlayState: "",
        micMode: "",
        quality: "",
        resolution: "",
        bitRate: 0,
        fps: 0,
        camera: "",
        gameTitle: "",
        privacy: "",
        commentsOn: "",
        DRSName: "",
        DRSProfileName: "",
        fpsOverlayPos: "",
        cameraOverlayPos: "",
        statusOverlayPos: "",
        viewersOverlayPos: "",
        freestyleActive: ""
      }
    },
    EVENTS_OSC_SESSION: {
      name: "Osc_Session",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        provider: "",
        totalMs: 0,
        screenState: ""
      }
    },
    EVENTS_OSC_BROADCAST_ERROR: {
      name: "Osc_Broadcast_Error",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        provider: "",
        error: ""
      }
    },
    EVENTS_OSC_PREFERENCES_NOTIFICATION_CHANGED: {
      name: "Osc_Preferences_Notifications_Changed",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        openShare: "",
        savedIR: "",
        savedMR: "",
        savedSS: "",
        irOnOff: "",
        mrStarted: "",
        brStarted: "",
        brPaused: "",
        savedHL: ""
      }
    },
    EVENTS_OSC_ANSEL_UI_SESSION_NAVIGATION_TYPE: {
      name: "Osc_Ansel_Ui_Session_Navigation_Type",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        totalMs: 0,
        usedKeyboard: "",
        usedMouse: "",
        usedController: "",
        controllerType: "",
        DRSName: "",
        DRSProfileName: "",
        mode: "",
        usedHideMenu: "",
        panningUsed: "",
        panningwithKB: "",
        panningwithMouse: "",
        installedDDVersion: ""
      }
    },
    EVENTS_OSC_ANSEL_TYPE: {
      name: "Osc_Ansel_Type",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        type: ""
      }
    },
    EVENTS_OSC_ANSEL_FILTER: {
      name: "Osc_Ansel_Filter",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        filter: "",
        mode: ""
      }
    },
    EVENTS_OSC_ANSEL_SCREENSHOT_TAKEN: {
      name: "Osc_Ansel_Screenshot_Taken",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        gameName: "",
        gameLaunchMode: "",
        enhanceMode: "",
        gameEngineControls: "",
        screenshotType: "",
        resolution: 0,
        screenshotResolution: "",
        superResolutionFactor: 1,
        filterID: "",
        filterAttributesValues: "",
        roll: 0,
        fov: 0,
        sketch: 0,
        colorEnhancer: 0,
        vignette: 0,
        brightness: 0,
        contrast: 0,
        vibrance: 0,
        hdrMode: "",
        grid: "",
        DRSName: "",
        DRSProfileName: "",
        stackedFilters: "",
        method: "",
        totalMs: 0,
        GFEDLISRVersion: ""
      }
    },
    EVENTS_OSC_FREESTYLE_FILTERS_APPLIED: {
      name: "Osc_FreeStyle_Filters_Applied",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        gameName: "",
        usedSlots: "",
        slot1Filters: "",
        slot2Filters: "",
        slot3Filters: "",
        activeSlot: "",
        activeFilters: "",
        DRSName: "",
        DRSProfileName: "",
        persistedFilters: "",
        usedMenu: "",
        installedDDVersion: ""
      }
    },
    EVENTS_OSC_FREESTYLE_FILTERS_ADDED: {
      name: "Osc_FreeStyle_Filters_Added",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        slotID: "",
        filterName: "",
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_FREESTYLE_FILTERS_SLOT_CHANGED: {
      name: "Osc_FreeStyle_Filters_Slot_Changed",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        currentSlotID: "",
        newSlotID: "",
        currentSlotFilters: "",
        newSlotFilters: "",
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_MENU_LAUNCH: {
      name: "Osc_Menu_Launch",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        totalMs: "",
        menuName: "",
        triggerMode: "",
        DRSName: "",
        DRSProfileName: "",
        installedDDVersion: ""
      }
    },
    EVENTS_OSC_HOTKEY_SETTINGS_EVENT: {
      name: "Osc_Hotkey_Settings_Event",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        menuName: "",
        hotkeyID: "",
        hotkey: "",
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_ANSEL_ERROR: {
      name: "Osc_Ansel_Error",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        errorValue: "",
        errorString: "",
        DRSName: "",
        DRSProfileName: "",
        mode: "",
        installedDDVersion: "",
        systemType: "",
        osVersion: "",
        isOptimus: "",
        gpuName: "",
        cpuName: "",
        usedMenu: ""
      }
    },
    EVENTS_OSC_ANSEL_FILTER_CONTROL_SETTINGS: {
      name: "Osc_Ansel_Filter_Control_Settings",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        menuName: "",
        filterName: "",
        controlName: "",
        controlType: "",
        controlValue: 0,
        gameLaunchMode: "",
        DRSName: "",
        DRSProfileName: "",
        CMSId: 0
      }
    },
    EVENTS_OSC_AUTOMATED_UI_PERF: {
      name: "Osc_Automated_UI_Perf",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        totalMs: 0,
        screenState: "",
        provider: "",
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_AUTOMATED_GALLERY_PERF: {
      name: "Osc_Automated_Gallery_Perf",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        totalMs: 0,
        averageMs: 0,
        screenState: "",
        numItems: 0,
        batchType: ""
      }
    },
    EVENTS_OSC_UPLOAD_DATA: {
      name: "Osc_Upload_Data",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        id: "",
        totalMs: "",
        provider: "",
        fileSize: 0,
        fileType: "",
        fileSubType: "",
        fileSource: "",
        hlID: "",
        containsMeme: "No",
        topMemeLength: 0,
        bottomMemeLength: 0,
        message: "",
        DRSName: "",
        DRSProfileName: "",
        retryAttempts: 0
      }
    },
    EVENTS_OSC_LOGIN: {
      name: "Osc_Login",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        provider: ""
      }
    },
    EVENTS_OSC_HIGHLIGHT_EVENT: {
      name: "Osc_Highlight_Event_V2",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        type: "",
        videoLength: 0,
        gameTitle: "",
        highlightType: "",
        highlightAction: "",
        sdkVersion: "",
        DRSName: "",
        DRSProfileName: "",
        modsActive: ""
      }
    },
    EVENTS_OSC_MTA_SETTINGS: {
      name: "Osc_Mta_Settings",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        systemVolume: 0,
        micVolume: 0,
        micBoost: 0,
        micSrc: "",
        isMultiTrack: "No"
      }
    },
    EVENTS_OSC_HIGHLIGHTS_OPENED: {
      name: "Osc_Highlights_Opened",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        numImages: 0,
        numVideos: 0,
        gameName: "",
        sdkVersion: "",
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_HIGHLIGHTS_INDIVIDUAL_TOGGLE: {
      name: "Osc_Highlights_Individual_Toggle",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        gameName: "",
        highlightId: "",
        onOffState: "",
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_HIGHLIGHTS_GAME_TOGGLE: {
      name: "Osc_Highlights_Game_Toggle",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        gameName: "",
        shutoffType: "",
        onOffState: "",
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_HIGHLIGHTS_DISC_SPACE_SETTING: {
      name: "Osc_Highlights_Disc_Space_Setting",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        sizeMB: 0
      }
    },
    EVENTS_OSC_HIGHLIGHT_ERROR: {
      name: "Osc_Highlight_Error",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        action: "",
        status: "",
        internalCode: 0,
        gameName: "",
        sdkVersion: "",
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_HIGHLIGHT_CANCELED: {
      name: "Osc_Highlight_Canceled",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        gameName: "",
        highlightType: "",
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_CAPTURE_EVENT: {
      name: "Osc_Capture_Event",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        action: "",
        micMode: "",
        DRSName: "",
        DRSProfileName: "",
        fpsOverlayPos: "",
        cameraOverlayPos: "",
        statusOverlayPos: "",
        freestyleActive: ""
      }
    },
    EVENTS_OSC_SHADOWPLAY_CAPTURE_ERROR: {
      name: "Osc_ShadowPlay_Capture_Error",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        action: "",
        errorDetail: "",
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_GIF_CONVERSION_ATTEMPT: {
      name: "Osc_Gif_Conversion_Attempt",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        totalMs: "",
        conversionIterations: 0,
        convertedFileSize: 0,
        containsMeme: "No",
        topMemeLength: 0,
        bottomMemeLength: 0,
        width: 0,
        height: 0,
        errorString: "",
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_EXPERIENCE_CONTROL_INFO: {
      name: "ExperienceControlInfo",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        experienceName: "",
        experienceId: "",
        experienceTestType: "Unknown",
        state: "Disabled",
        variantType: "default",
        group: ""
      }
    },
    EVENTS_OSC_SETTINGS_EVENT: {
      name: "Osc_Settings_Event",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        menuName: "",
        controlType: "",
        toggleState: ""
      }
    },
    EVENTS_OSC_ANSEL_LITE_SCREENSHOT_TAKEN: {
      name: "Osc_Ansel_Lite_Screenshot_Taken",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        DRSProfileName: "",
        DRSName: "",
        gameLaunchMode: "",
        screenshotType: "",
        screenshotResolution: "",
        superResolutionFactor: 1,
        stackedFilters: "",
        hdrMode: "",
        grid: "",
        panningUsed: "",
        method: "",
        totalMs: 0,
        GFEDLISRVersion: ""
      }
    },
    EVENTS_OSC_ANSEL_SCREENSHOT_STARTED: {
      name: "Osc_Ansel_Screenshot_Started",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        DRSProfileName: "",
        DRSName: "",
        gameLaunchMode: "",
        screenshotType: "",
        screenshotResolution: "",
        superResolutionFactor: 1,
        hdrMode: "",
        panningUsed: "",
        method: "",
        mode: "",
        GFEDLISRVersion: ""
      }
    },
    EVENTS_OSC_ANSEL_SCREENSHOT_CANCELLED: {
      name: "Osc_Ansel_Screenshot_Cancelled",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        DRSProfileName: "",
        DRSName: "",
        gameLaunchMode: "",
        screenshotType: "",
        screenshotResolution: "",
        superResolutionFactor: 1,
        hdrMode: "",
        panningUsed: "",
        method: "",
        mode: "",
        totalMs: 0,
        GFEDLISRVersion: ""
      }
    },
    EVENTS_OSC_ANSEL_SCREENSHOT_FAILED: {
      name: "Osc_Ansel_Screenshot_Failed",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        mode: "",
        errorValue: "",
        errorString: "",
        DRSProfileName: "",
        DRSName: "",
        installedDDVersion: "",
        systemType: "",
        osVersion: "",
        isOptimus: "",
        gpuName: "",
        cpuName: "",
        gameLaunchMode: "",
        screenshotType: "",
        screenshotResolution: "",
        superResolutionFactor: 1,
        hdrMode: "",
        panningUsed: "",
        method: "",
        GFEDLISRVersion: ""
      }
    },
    EVENTS_OSC_PERFORMANCE_TOOL_SIDEBAR_SESSION: {
      name: "Osc_Performance_Tool_Sidebar_Session",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        featureSupported: "",
        appliedStatus: "No",
        automaticTuningEnabled: "No",
        disclaimerPromptOptionSelected: "",
        isFullscreen: "No",
        restoredDefaults: "No",
        gpuChevronUsed: "No",
        tuningTypeChevronUsed: "No",
        perfMetricScrollUsed: "No",
        HUDSettingUsed: "No",
        gpuCount: 0,
        GPU1: "",
        GPU2: "",
        systemType: ""
      }
    },
    EVENTS_OSC_PERFORMANCE_TOOL_OVERLAY_SESSION: {
      name: "Osc_Performance_Tool_Overlay_Session",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        overlayView: "",
        hotkeyUsed: "No",
        totalMs: 0,
        isSystemRLACapable: "",
        isMouseRLACapable: "",
        isRLAEnabled: "",
        isRFISupported: "",
        IsReflexStatsSupported: "",
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_PERFORMANCE_TOOL_LAST_SCAN_RESULTS: {
      name: "Osc_Performance_Tool_Last_Scan_Results",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        scanType: "",
        gpuName: "",
        gpuCoreClockOffset: 0,
        gpuMemoryClockOffset: 0,
        temperatureLimit: 0,
        powerLimit: 0,
        voltageLimit: 0
      }
    },
    EVENTS_OSC_PERFORMANCE_TOOL_LATENCY_METRICS: {
      name: "Osc_Performance_Tool_Latency_Metrics",
      gdprLevel: "technical",
      ts: "",
      parameters: {
        meanFPS: 0,
        meanRenderPresentLatency: 0,
        meanRenderingLatency: 0,
        meanMouseLatency: 0,
        meanPCDisplayLatency: 0,
        meanSystemLatency: 0,
        DRSName: "",
        DRSProfileName: "",
        gpuName: ""
      }
    },
    EVENTS_OSC_PERFORMANCE_TOOL_ERROR: {
      name: "Osc_Performance_Tool_Error",
      gdprLevel: "technical",
      ts: "",
      parameters: {
        errorType: "",
        reportedBy: "",
        gpuName: "",
        osVersion: "",
        installedDDVersion: "",
        systemType: ""
      }
    },
    EVENTS_OSC_PERFORMANCE_TOOL_SAMPLE_SIZE: {
      name: "Osc_Performance_Tool_Sample_Size",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        setSampleSize: 0,
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_PERFORMANCE_TOOL_RESET_AVERAGE: {
      name: "Osc_Performance_Tool_Reset_Average",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_PERFORMANCE_TOOL_LOGGING_SESSION: {
      name: "Osc_Performance_Tool_Logging_Session",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        totalMs: 0,
        DRSName: "",
        DRSProfileName: ""
      }
    },
    EVENTS_OSC_PERFORMANCE_TOOL_SETTINGS: {
      name: "Osc_Performance_Tool_Settings",
      gdprLevel: "functional",
      ts: "",
      parameters: {
        settingName: "",
        settingValue: "",
        DRSName: "",
        DRSProfileName: ""
      }
    }
  })
}
