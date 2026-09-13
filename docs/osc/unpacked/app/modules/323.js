// ─────────────────────────────────────────────────────────────
// APP MODULE 323
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports =
    '<div flex class=progress-indicator> <div layout=row layout-align="space-between start"> <p translate={{nvMessage}}></p> </div> <div class=progress-bar> <md-progress-linear class=md-accent flex md-mode=determinate value={{nvValue}} ng-cloak></md-progress-linear> </div> <button ng-disabled=nvProgressCanceled class=progress-indicator-button translate={{nvButton}} ng-click=onButtonClick() ng-if=!hideButton></button> </div> '
}
