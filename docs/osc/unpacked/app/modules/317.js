// ─────────────────────────────────────────────────────────────
// APP MODULE 317
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports =
    '<div class=osc-content ng-init=errorDialog.initialize(errorDialougeParam) layout=row ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{errorDialog.title}} icon-font={{errorDialog.icon}} /> </div> <div class=osc-central-div layout=column> <div class=error-panel layout=column> <div class=error-title> <p translate={{errorDialog.error}}></p> </div> <div class=error-details> <p translate={{errorDialog.details}}></p> </div> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-gray" translate=l10n.troubleshoot ng-click=errorDialog.troubleshoot() hover-focus ng-if=!errorDialog.isCustomError></button> <button class="osc-button osc-button-gray" translate=l10n.close ng-click=errorDialog.close() hover-focus ng-if=!errorDialog.isCustomError></button> <button class="osc-button osc-button-gray" translate=l10n.gotIt ng-click=errorDialog.close() hover-focus ng-if=errorDialog.isCustomError></button> </div> </div> '
}
