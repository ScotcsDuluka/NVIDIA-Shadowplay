// launcher_script.h - augmentation JavaScript injected at context
// creation in every frame (document-start equivalent). Installs the
// host-push dispatcher the supervisor uses to deliver 1s state snapshots:
//   window.__LauncherState(<json object literal>)
//   window.__onLauncherState(fn)   // page subscribes
#ifndef LAUNCHER_SCRIPT_H_
#define LAUNCHER_SCRIPT_H_

inline const char* LauncherAugmentationSource() {
  return "(function(){"
         "if (window.__LauncherAugmented) return;"
         "window.__LauncherAugmented = true;"
         "var handlers = [];"
         "window.__onLauncherState = function(fn) {"
         "  if (typeof fn === 'function') handlers.push(fn);"
         "  if (window.__LauncherLastState) try { fn(window.__LauncherLastState); } catch (e) {}"
         "};"
         "window.__LauncherState = function(s) {"
         "  window.__LauncherLastState = s;"
         "  for (var i = 0; i < handlers.length; i++) {"
         "    try { handlers[i](s); } catch (e) {}"
         "  }"
         "};"
         "})();";
}

#endif  // LAUNCHER_SCRIPT_H_
