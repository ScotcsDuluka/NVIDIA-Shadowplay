// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 317
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div class=osc-content ng-init=errorDialog.initialize(errorDialougeParam) layout=row ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{errorDialog.title}} icon-font={{errorDialog.icon}} /> </div> <div class=osc-central-div layout=column> <div class=error-panel layout=column> <div class=error-title> <p translate={{errorDialog.error}}></p> </div> <div class=error-details> <p translate={{errorDialog.details}}></p> </div> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-gray" translate=l10n.troubleshoot ng-click=errorDialog.troubleshoot() hover-focus ng-if=!errorDialog.isCustomError></button> <button class="osc-button osc-button-gray" translate=l10n.close ng-click=errorDialog.close() hover-focus ng-if=!errorDialog.isCustomError></button> <button class="osc-button osc-button-gray" translate=l10n.gotIt ng-click=errorDialog.close() hover-focus ng-if=errorDialog.isCustomError></button> </div> </div> ';
}
