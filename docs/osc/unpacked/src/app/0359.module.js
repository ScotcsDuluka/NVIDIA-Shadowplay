// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 359
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div class=osc-content layout=row ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{controller.title}} icon-font={{controller.icon}} status={{controller.status}} /> </div> <div class=osc-central-div layout=column> <div class=preferences-root-panel> <div class=preferences-root-label> <p translate=l10n.anselMods /> </div> <div layout=row> <span class=settings-switch-label translate=l10n.captureEnhance></span> <md-switch md-invert class=settings-switch ng-model=controller.modsStatus ng-change=controller.setModsStatus() focus=true></md-switch> </div> <div layout=row> <span class=settings-describe translate=l10n.describeAnselMods></span> </div> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-gray" translate=l10n.back ng-click=controller.done() tabindex=3 hover-focus></button> </div> </div> ';
}
