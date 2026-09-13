// ─────────────────────────────────────────────────────────────
// APP MODULE 358
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports =
    '<div class=osc-content layout=row ng-init=preferencesMenu.initPreferencesMenu() ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{preferencesMenu.title}} icon-font={{preferencesMenu.icon}} status={{preferencesMenu.status}} /> </div> <div class=osc-central-div layout=column> <div class=preferences-root-panel> <div class=preferences-root-label> <p translate=l10n.preferencesHome /> </div> <div class=preferences-item-container> <div class="preferences-item hover-list-item" layout=row ng-repeat="tile in preferencesMenu.tiles" ng-if=tile.visible ng-click=preferencesMenu.goToPreferenceState(tile) ng-mouseover="preferencesMenu.mouseOver($index, $event)" ng-keydown="preferencesMenu.keyDown($index, $event)" ng-class="{\'hover-list-item-focus\': preferencesMenu.isActiveTile($index)}" focus1={{$index}},{{preferencesMenu.tileIndex}}> <md-icon class="share-icon icon-normal preferences-item-icon {{tile.icon}}"></md-icon> <div layout=column layout-align="center start"> <div class=preferences-item-label> <span translate={{tile.title}} /> </div> <div ng-if=tile.subtitle class="preferences-item-label preferences-item-sublabel"> <span translate={{tile.subtitle}} /> </div> </div> </div> </div> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-green" translate=l10n.done ng-click=preferencesMenu.back() tabindex=20 hover-focus/> </div> </div> '
}
