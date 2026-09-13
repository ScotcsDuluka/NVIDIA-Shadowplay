// ─────────────────────────────────────────────────────────────
// APP MODULE 359
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports =
    '<div class=osc-content layout=row ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{controller.title}} icon-font={{controller.icon}} status={{controller.status}} /> </div> <div class=osc-central-div layout=column> <div class=preferences-root-panel> <div class=preferences-root-label> <p translate=l10n.anselMods /> </div> <div layout=row> <span class=settings-switch-label translate=l10n.captureEnhance></span> <md-switch md-invert class=settings-switch ng-model=controller.modsStatus ng-change=controller.setModsStatus() focus=true></md-switch> </div> <div layout=row> <span class=settings-describe translate=l10n.describeAnselMods></span> </div> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-gray" translate=l10n.back ng-click=controller.done() tabindex=3 hover-focus></button> </div> </div> '
}
