// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 360
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    ' <div class=osc-content layout=row ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{controller.title}} icon-font={{controller.icon}} status={{controller.status}} /> </div> <div class=osc-central-div layout=column> <div class=preferences-root-panel> <div class=notif-root-label layout=row layout-align="space-between center"> <span translate=l10n.notifications></span> <md-switch md-invert class=settings-switch ng-model=controller.globalToggle ng-change=controller.globalChanged() focus=true></md-switch> </div> <div class=notif-item-container> <div class=notif-row layout=row ng-repeat="item in controller.notifiers"> <div class=notif-header ng-if="item.header && item.viewHeader"> <span translate={{item.header}}></span> </div> <div class=notif-item-row ng-if="item.name && item.available" layout=row layout-align="space-between center"> <span class=notif-text translate={{item.name}}></span> <md-switch md-invert class=settings-switch ng-model=item.enabled ng-change=controller.notifChanged() hover-focus></md-switch> </div> </div> </div> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-gray" translate=l10n.back ng-click=controller.done() tabindex=3 hover-focus></button> </div> </div> ';
}
