// ─────────────────────────────────────────────────────────────
// APP MODULE 313
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports =
    '<div class=osc-content layout=row ng-init=broadcastMenu.showData() ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{broadcastMenu.title}} icon-font={{broadcastMenu.icon}} status={{broadcastMenu.status}}></nv-osc-tile> </div> <div id=topLevel class=gallery-central-div-broadcast layout=column> <div class=gallery-inside-central-div> <destination-picker nv-change-picker=broadcastMenu.pickerData on-upload-data-changed=broadcastMenu.destinationDataChange(data) on-login-via-parent=broadcastMenu.login(uploadService)></destination-picker> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-green" translate=l10n.goLive ng-click=broadcastMenu.start() ng-class="{\'osc-button-green\': !broadcastMenu.broadcastDisabled(), \'item-disabled osc-button-gray\': broadcastMenu.broadcastDisabled()}" ng-disabled=broadcastMenu.broadcastDisabled() hover-focus></button> <button class="osc-button osc-button-gray" translate=l10n.back ng-click=broadcastMenu.back() hover-focus></button> </div> </div> '
}
