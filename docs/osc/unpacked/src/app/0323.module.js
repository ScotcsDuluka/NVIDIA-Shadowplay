// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 323
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div flex class=progress-indicator> <div layout=row layout-align="space-between start"> <p translate={{nvMessage}}></p> </div> <div class=progress-bar> <md-progress-linear class=md-accent flex md-mode=determinate value={{nvValue}} ng-cloak></md-progress-linear> </div> <button ng-disabled=nvProgressCanceled class=progress-indicator-button translate={{nvButton}} ng-click=onButtonClick() ng-if=!hideButton></button> </div> ';
}
