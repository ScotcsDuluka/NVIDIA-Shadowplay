// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 367
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div class=osc-content layout=row ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{controller.title}} icon-font={{controller.icon}} status={{controller.status}} /> </div> <div class=osc-central-div layout=column> <div id=topLevel class=preferences-root-panel> <div class=preferences-root-label> <p translate=l10n.videoCapture /> </div> <div ng-if=controller.settingsDisabled class=settings-text-warning> <md-icon class="share-icon icon32 icon-normal icon-notify_warning mta-icon-warning settings-warning-icon-color"></md-icon> <span translate=l10n.settingsVideoCaptureDisable></span> </div> <settings-customize nv-change-settings=controller.settingsData on-customize-close-complete=controller.customizeCloseComplete(data) flex layout=column></settings-customize> </div> </div> <div class=osc-side-buttons layout=column> <div ng-if=controller.fromMainMenu()> <button class="osc-button osc-button-green" translate=l10n.save ng-click=controller.save() tabindex=6 hover-focus></button> </div> <button class="osc-button osc-button-gray" translate=l10n.back ng-click=controller.back() tabindex=6 hover-focus></button> </div> </div> ';
}
