// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 363
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div class=osc-content layout=row ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{controller.title}} icon-font={{controller.icon}} status={{controller.status}} /> </div> <div class=osc-central-div layout=column> <div class=preferences-root-panel> <div class=preferences-root-label> <p translate=l10n.privacyControl /> </div> <div ng-if=controller.settingsDisabled class=settings-text-warning> <md-icon class="share-icon icon32 icon-normal icon-notify_warning mta-icon-warning settings-warning-icon-color"></md-icon> <span translate=l10n.settingsPrivacyDisable></span> </div> <div ng-class="{\'item-disabled\': controller.settingsDisabled}"> <div layout=row> <span class=settings-switch-label translate=l10n.settingsPrivacySwitch></span> <md-switch md-invert class=settings-switch ng-model=controller.privacyControlStatus ng-change=controller.setPrivacyControlStatus() ng-disabled=controller.settingsDisabled focus=true></md-switch> </div> <div layout=row> <span class=settings-describe translate=l10n.settingsPrivacyDescribe></span> </div> </div> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-gray" translate=l10n.back ng-click=controller.done() tabindex=1 hover-focus></button> </div> </div> ';
}
