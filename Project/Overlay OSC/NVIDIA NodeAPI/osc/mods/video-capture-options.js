(function () {
  'use strict';

  var endpoint = '/DulukaCapture/v.1.0/settings';
  var panelId = 'nv-video-capture-options';
  var panelVersion = '7';
  var state = null;
  var statusNode = null;
  var apiSelect = null;
  var apiScope = null;
  var encoderSelect = null;
  var presetSelect = null;
  var cursorButton = null;
  var previewNode = null;
  var modeButtons = {};
  var encoderNames = {};
  var presetChoices = {
    NVENC_H264: ['P1', 'P2', 'P3', 'P4', 'P5', 'P6', 'P7'],
    NVENC_HEVC: ['P1', 'P2', 'P3', 'P4', 'P5', 'P6', 'P7'],
    NVENC_AV1: ['P1', 'P2', 'P3', 'P4', 'P5', 'P6', 'P7'],
    QuickSync_H264: ['veryslow', 'slower', 'slow', 'medium', 'fast', 'faster', 'veryfast'],
    QuickSync_HEVC: ['veryslow', 'slower', 'slow', 'medium', 'fast', 'faster', 'veryfast'],
    AMF_H264: [{ index: 2, label: 'quality' }, { index: 4, label: 'balanced' }, { index: 6, label: 'speed' }],
    AMF_HEVC: [{ index: 2, label: 'quality' }, { index: 4, label: 'balanced' }, { index: 6, label: 'speed' }],
    LibX264: ['slow', 'medium', 'fast', 'faster', 'veryfast', 'superfast', 'ultrafast'],
    LibX265: ['slow', 'medium', 'fast', 'faster', 'veryfast', 'superfast', 'ultrafast']
  };
  var encoderChoices = [
    ['NVENC_H264', 'NVIDIA NVENC H.264'],
    ['NVENC_HEVC', 'NVIDIA NVENC HEVC'],
    ['NVENC_AV1', 'NVIDIA NVENC AV1'],
    ['QuickSync_H264', 'Intel Quick Sync H.264'],
    ['QuickSync_HEVC', 'Intel Quick Sync HEVC'],
    ['AMF_H264', 'AMD AMF H.264'],
    ['AMF_HEVC', 'AMD AMF HEVC'],
    ['LibX264', 'Software x264'],
    ['LibX265', 'Software x265']
  ];

  function videoPage() {
    return /#\/base\/preferences\/video(?:[/?]|$)/.test(window.location.hash);
  }

  function option(value, label, disabled) {
    var item = document.createElement('option');
    item.value = value;
    item.textContent = label;
    item.disabled = Boolean(disabled);
    return item;
  }

  function makeSelect(id, label) {
    var wrap = document.createElement('label');
    wrap.className = 'nv-video-engine-field';
    wrap.setAttribute('for', id);
    var caption = document.createElement('span');
    caption.textContent = label;
    var select = document.createElement('select');
    select.className = 'nv-video-engine-select';
    select.id = id;
    wrap.appendChild(caption);
    wrap.appendChild(select);
    return { wrap: wrap, select: select };
  }

  function makeApiSelect(id, label) {
    var inputContainer = document.createElement('md-input-container');
    inputContainer.className = 'fps flex';
    var heading = document.createElement('div');
    heading.className = 'customize-header-text';
    heading.setAttribute('layout', 'row');
    var caption = document.createElement('span');
    caption.textContent = label;
    heading.appendChild(caption);
    var select = document.createElement('md-select');
    select.id = id;
    select.className = 'osc-select resolution-fps-select';
    select.setAttribute('ng-model', 'api');
    select.setAttribute('ng-change', 'apiChanged(api)');
    select.setAttribute('aria-label', label);
    select.setAttribute('hover-focus', '');
    var item = document.createElement('md-option');
    item.setAttribute('ng-repeat', 'item in apiOptions');
    item.setAttribute('ng-value', 'item.value');
    item.setAttribute('ng-disabled', 'item.disabled');
    item.setAttribute('md-ink-ripple', 'false');
    item.setAttribute('hover-focus', '');
    var text = document.createElement('span');
    text.className = 'md-text';
    text.textContent = '{{item.label}}';
    item.appendChild(text);
    select.appendChild(item);
    inputContainer.appendChild(heading);
    inputContainer.appendChild(select);
    return { container: inputContainer, select: select };
  }

  function linkApiSelect(apiControl) {
    var injector = window.angular.element(document.documentElement).injector();
    if (!injector) throw new Error('NVIDIA Angular injector is not available');
    apiScope = injector.get('$rootScope').$new();
    apiScope.apiOptions = [];
    apiScope.apiChanged = function (value) {
      if (!state) return;
      state.apiCapture = value;
      save();
    };
    injector.get('$compile')(apiControl.container)(apiScope);
    apiSelect = apiControl.select;
  }

  function destroyApiSelectScope() {
    if (apiScope) apiScope.$destroy();
    apiScope = null;
    apiSelect = null;
  }

  function makeSection(title) {
    var section = document.createElement('section');
    section.className = 'nv-video-engine-section';
    var heading = document.createElement('h2');
    heading.className = 'nv-video-engine-section-heading';
    var icon = document.createElement('span');
    icon.className = 'nv-video-engine-chevron';
    icon.textContent = '';
    var text = document.createElement('span');
    text.textContent = title;
    heading.appendChild(icon);
    heading.appendChild(text);
    section.appendChild(heading);
    return section;
  }

  function save() {
    if (!state) return Promise.resolve();
    statusNode.textContent = 'Saving…';
    return fetch(endpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(state)
    }).then(function (response) {
      if (!response.ok) {
        return response.json().then(function (error) {
          throw new Error(error.message || 'Unable to save capture settings');
        });
      }
      return response.json();
    }).then(function (saved) {
      state = saved;
      statusNode.classList.remove('nv-video-engine-error');
      statusNode.textContent = 'Saved';
      render();
    }).catch(function (error) {
      statusNode.textContent = error.message;
      statusNode.classList.add('nv-video-engine-error');
    });
  }

  function apiOptions() {
    var choices = state.engineMode === 'Duluka'
      ? [
        ['dxgi_desktop_duplication', 'DXGI Desktop Duplication', false],
        ['windows_graphics_capture', 'Windows Graphics Capture', true],
        ['d3d11_native', 'Direct3D 11 Native Capture', true],
        ['window_capture', 'Window Capture', true],
        ['region_capture', 'Region Capture', true],
        ['native_game_capture', 'Native Game Capture', true]
      ]
      : [
        ['ddagrab', 'ddagrab — Direct3D 11 Desktop Capture', false],
        ['gdigrab', 'gdigrab — GDI Desktop Capture', false],
        ['gfxcapture', 'gfxcapture — Graphics Capture', false]
      ];
    if (choices.some(function (choice) {
      return choice[0] === state.apiCapture && !choice[2];
    })) {
      apiScope.api = state.apiCapture;
    } else {
      state.apiCapture = choices[0][0];
      apiScope.api = state.apiCapture;
    }
    apiScope.apiOptions = choices.map(function (choice) {
      return { value: choice[0], label: choice[1], disabled: choice[2] };
    });
    apiScope.$applyAsync();
  }

  function presetOptions() {
    var choices = presetChoices[state.encoder] || ['medium'];
    presetSelect.textContent = '';
    choices.forEach(function (choice, index) {
      var value = typeof choice === 'string' ? index + 1 : choice.index;
      var label = typeof choice === 'string' ? choice : choice.label;
      presetSelect.appendChild(option(String(value), label));
    });
    var selected = choices.some(function (choice, index) {
      return (typeof choice === 'string' ? index + 1 : choice.index) === state.encoderPreset;
    });
    if (!selected) state.encoderPreset = typeof choices[0] === 'string' ? 1 : choices[0].index;
    presetSelect.value = String(state.encoderPreset);
  }

  function render() {
    Object.keys(modeButtons).forEach(function (mode) {
      var selected = state.engineMode === mode;
      modeButtons[mode].classList.toggle('nv-video-engine-active', selected);
      modeButtons[mode].classList.toggle('osc-button-green', selected);
      modeButtons[mode].setAttribute('aria-pressed', String(selected));
    });
    apiOptions();
    encoderSelect.value = state.encoder;
    presetOptions();
    cursorButton.classList.toggle('nv-video-engine-active', state.captureCursor);
    cursorButton.setAttribute('aria-pressed', String(state.captureCursor));
    cursorButton.textContent = state.captureCursor ? 'On' : 'Off';
    var presetLabel = presetSelect.options[presetSelect.selectedIndex].textContent;
    var captureApi = state.engineMode === 'Duluka' || state.apiCapture === 'ddagrab'
      ? 'ddagrab (built-in)' : state.apiCapture;
    if (Number.isFinite(state.fps) && Number.isFinite(state.bitrateKbps) &&
        typeof state.useNativeResolution === 'boolean' &&
        Number.isFinite(state.width) && Number.isFinite(state.height)) {
      var resolution = state.useNativeResolution ? 'native' : state.width + 'x' + state.height;
      previewNode.textContent = 'Requested (config.json): ' + state.encoder + ' · ' +
        state.fps + ' fps (live per record) · ' + state.bitrateKbps +
        ' kbps (engine restart) · ' + resolution + ' (engine restart) · preset ' +
        presetLabel + ' (engine restart) · Capture API: ' + captureApi;
    } else {
      previewNode.textContent = 'Engine: ' + state.engineMode + ' · ' + captureApi +
        ' · ' + encoderNames[state.encoder] + ' · ' + presetLabel;
    }
  }

  function buildPanel(host) {
    var panel = document.createElement('section');
    panel.id = panelId;
    panel.className = 'nv-video-engine-panel';

    var engineSection = makeSection('Engine:');
    var engineControls = document.createElement('div');
    engineControls.className = 'nv-video-engine-row nv-video-engine-engine-controls';
    var modes = document.createElement('div');
    modes.className = 'nv-video-engine-row nv-video-engine-modes';
    [['FFmpeg', 'FFmpeg Capture'], ['Duluka', 'Duluka Capture']].forEach(function (item) {
      var button = document.createElement('button');
      button.type = 'button';
      button.className = 'osc-button nv-video-engine-mode';
      button.textContent = item[1];
      button.addEventListener('click', function () {
        state.engineMode = item[0];
        if (item[0] === 'Duluka') state.apiCapture = 'dxgi_desktop_duplication';
        else if (state.apiCapture === 'dxgi_desktop_duplication') state.apiCapture = 'ddagrab';
        render();
        save();
      });
      modeButtons[item[0]] = button;
      modes.appendChild(button);
    });
    var obsButton = document.createElement('button');
    obsButton.type = 'button';
    obsButton.className = 'osc-button nv-video-engine-mode nv-video-engine-mode-unavailable';
    obsButton.textContent = 'OBS Capture';
    obsButton.title = 'OBS capture is not connected';
    obsButton.addEventListener('click', function () {
      statusNode.textContent = 'OBS Capture is not connected to the runtime engine yet.';
    });
    modes.appendChild(obsButton);
    engineControls.appendChild(modes);

    var api = makeApiSelect('nv-video-engine-api', 'Capture API');
    engineControls.appendChild(api.container);
    engineSection.appendChild(engineControls);
    panel.appendChild(engineSection);

    var encoder = makeSelect('nv-video-engine-encoder', 'Encoder');
    encoderSelect = encoder.select;
    encoderChoices.forEach(function (choice) {
      encoderNames[choice[0]] = choice[1];
      encoderSelect.appendChild(option(choice[0], choice[1]));
    });
    encoder.wrap.firstChild.textContent = 'Codec Encoder';
    var preset = makeSelect('nv-video-engine-preset', 'Encoder preset');
    presetSelect = preset.select;

    var advancedSection = makeSection('Advanced:');
    var advanced = document.createElement('div');
    advanced.className = 'nv-video-engine-advanced';
    var preview = document.createElement('div');
    preview.className = 'nv-video-engine-preview';
    var previewTitle = document.createElement('span');
    previewTitle.className = 'nv-video-engine-preview-title';
    previewTitle.textContent = 'Preview Build Arguments';
    previewNode = document.createElement('code');
    previewNode.className = 'nv-video-engine-preview-value';
    preview.appendChild(previewTitle);
    preview.appendChild(previewNode);
    var actions = document.createElement('div');
    actions.className = 'nv-video-engine-actions';
    var reloadButton = document.createElement('button');
    reloadButton.type = 'button';
    reloadButton.className = 'nv-video-engine-action';
    reloadButton.textContent = 'Reload';
    reloadButton.addEventListener('click', function () {
      statusNode.textContent = 'Loading…';
      fetch(endpoint).then(function (response) {
        if (!response.ok) throw new Error('Unable to load capture settings (' + response.status + ')');
        return response.json();
      }).then(function (settings) {
        state = settings;
        render();
        statusNode.classList.remove('nv-video-engine-error');
        statusNode.textContent = 'Ready';
      }).catch(function (error) {
        statusNode.textContent = error.message;
        statusNode.classList.add('nv-video-engine-error');
      });
    });
    var copyButton = document.createElement('button');
    copyButton.type = 'button';
    copyButton.className = 'nv-video-engine-action';
    copyButton.textContent = 'Copy';
    copyButton.addEventListener('click', function () {
      if (!navigator.clipboard || !navigator.clipboard.writeText) {
        statusNode.textContent = 'Clipboard access is unavailable';
        statusNode.classList.add('nv-video-engine-error');
        return;
      }
      navigator.clipboard.writeText(previewNode.textContent).then(function () {
        statusNode.classList.remove('nv-video-engine-error');
        statusNode.textContent = 'Copied';
      }).catch(function () {
        statusNode.textContent = 'Clipboard access is unavailable';
        statusNode.classList.add('nv-video-engine-error');
      });
    });
    actions.appendChild(reloadButton);
    actions.appendChild(copyButton);
    var encoderFields = document.createElement('div');
    encoderFields.className = 'nv-video-engine-advanced-fields';
    encoderFields.appendChild(encoder.wrap);
    encoderFields.appendChild(preset.wrap);
    advanced.appendChild(preview);
    advanced.appendChild(actions);
    advanced.appendChild(encoderFields);
    advancedSection.appendChild(advanced);
    panel.appendChild(advancedSection);

    var footer = document.createElement('div');
    footer.className = 'nv-video-engine-row nv-video-engine-footer';
    var cursorLabel = document.createElement('span');
    cursorLabel.className = 'nv-video-engine-cursor-label';
    cursorLabel.textContent = 'Capture cursor';
    cursorButton = document.createElement('button');
    cursorButton.type = 'button';
    cursorButton.className = 'nv-video-engine-toggle';
    cursorButton.addEventListener('click', function () {
      state.captureCursor = !state.captureCursor;
      render();
      save();
    });
    statusNode = document.createElement('span');
    statusNode.className = 'nv-video-engine-status';
    footer.appendChild(cursorLabel);
    footer.appendChild(cursorButton);
    footer.appendChild(statusNode);
    panel.appendChild(footer);
    host.appendChild(panel);
    linkApiSelect(api);

    encoderSelect.addEventListener('change', function () {
      state.encoder = encoderSelect.value;
      presetOptions();
      state.encoderPreset = Number(presetSelect.value);
      save();
    });
    presetSelect.addEventListener('change', function () {
      state.encoderPreset = Number(presetSelect.value);
      save();
    });
  }

  function initialize() {
    if (!videoPage()) return;
    var host = document.querySelector('nv-preferences-video #topLevel');
    if (!host) return;
    if (!window.angular || !window.angular.element(document.documentElement).injector()) return;
    var existing = document.getElementById(panelId);
    if (existing && existing.getAttribute('data-nv-ui-version') === panelVersion) return;
    destroyApiSelectScope();
    var oldMenu = document.getElementById('nv-video-engine-api-menu');
    if (oldMenu) oldMenu.parentNode.removeChild(oldMenu);
    if (existing) existing.parentNode.removeChild(existing);
    modeButtons = {};
    buildPanel(host);
    document.getElementById(panelId).setAttribute('data-nv-ui-version', panelVersion);
    statusNode.textContent = 'Loading…';
    fetch(endpoint).then(function (response) {
      if (!response.ok) return response.json().then(function (error) {
        throw new Error(error.message || 'Unable to load capture settings');
      });
      return response.json();
    }).then(function (settings) {
      state = settings;
      render();
      statusNode.classList.remove('nv-video-engine-error');
      statusNode.textContent = 'Ready';
    }).catch(function (error) {
      statusNode.textContent = error.message;
      statusNode.classList.add('nv-video-engine-error');
    });
  }

  var attempts = 0;
  var timer = window.setInterval(function () {
    initialize();
    attempts += 1;
    if (document.getElementById(panelId) || attempts >= 120) window.clearInterval(timer);
  }, 500);
  window.addEventListener('hashchange', function () {
    if (videoPage()) initialize();
    else destroyApiSelectScope();
  });
})();
