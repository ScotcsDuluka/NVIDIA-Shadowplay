// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 366
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div class=osc-content layout=row ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{controller.title}} icon-font={{controller.icon}} status={{controller.status}} /> </div> <div class=osc-central-div layout=column> <div class=preferences-root-panel> <div class=preferences-root-label> <p translate=l10n.stream /> </div> <div ng-if=controller.settingsDisabled class=settings-text-warning> <md-icon class="share-icon icon32 icon-normal icon-notify_warning mta-icon-warning settings-warning-icon-color"></md-icon> <span translate=l10n.settingsStreamDisable></span> </div> <div ng-class="{\'item-disabled\': controller.settingsDisabled}"> <div layout=row> <span class=settings-switch-label translate=l10n.settingsStreamSwitch></span> <md-switch md-invert class=settings-switch ng-model=controller.streamStatus ng-change=controller.setStreamStatus() ng-disabled=controller.settingsDisabled focus=true></md-switch> </div> <div layout=row> <span class=settings-describe translate=l10n.settingsStreamDescribe></span> </div> </div> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-gray" translate=l10n.back ng-click=controller.done() tabindex=3 hover-focus></button> </div> </div> ';
}
