// ─────────────────────────────────────────────────────────────
// APP MODULE 324
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports =
    '<div class=osc-content layout=row ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{coplayGuestControls.title}} icon-font={{coplayGuestControls.icon}} status={{coplayGuestControls.status}} /> </div> <div class=osc-central-div layout=column> <div class=coplay-guest-panel> <div ng-repeat="tile in coplayGuestControls.tiles"> <input type=radio ng-model=coplayGuestControls.selection id={{tile.name}} value={{tile.name}}> <label for={{tile.name}}> <div layout=row class="guest-item hover-list-item" tabindex=1 hover-focus ng-keyup="coplayGuestControls.keyUp($event, tile)" focus={{tile.initialFocus}}> <md-icon class="share-icon icon-normal guest-icon {{tile.icon}}"></md-icon> <p class=guest-label translate={{tile.title}} /> </div> </label> </div> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-green" translate=l10n.done ng-click=coplayGuestControls.done() tabindex=4 hover-focus></button> <button class="osc-button osc-button-gray" translate=l10n.back ng-click=coplayGuestControls.back() tabindex=5 hover-focus></button> </div> </div> '
}
