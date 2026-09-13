// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 357
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div class=osc-content layout=row ng-init=controller.initPreferencesKeyboardShortcuts() ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{controller.title}} icon-font={{controller.icon}} status={{controller.status}} /> </div> <div class=osc-central-div layout=column> <div class=preferences-root-panel> <div class=preferences-root-label> <p translate=l10n.keyboardShortcuts /> </div> <div class=keyboard-shortcuts-item-container layout=column> <div class=keyboard-shortcuts-block> <div class=shortcut-category ng-repeat="(catId,category) in controller.categories" ng-if=category.visible> <span class=shortcuts-lvl1-label translate={{category.name}}></span> <div layout=row class=shortcuts-row ng-repeat="shortcut in controller.shortcuts" ng-if="shortcut.catId === catId && shortcut.visible"> <div class="shortcuts-box hover-list-item" ng-class="{\'hover-list-item-selected\': controller.isShortcutSelected(shortcut)}" ng-click=controller.selectShortcut(shortcut) ng-keydown=controller.processKeyDownEvent($event) ng-keyup=controller.processKeyUpEvent($event) hover-focus focus="{{$index === 0}}"> <div class=shortcuts-box-nested> <span class=shortcuts-lvl2-label ng-class="{\'shortcuts-none-color\': shortcut.hotkeyStr===\'None\'}" translate={{shortcut.hotkeyStr}}></span> </div> </div> <div class=shortcuts-nested> <p class=shortcuts-lvl2-label translate={{shortcut.name}} translate-values="{ minutesToSave: \'{{controller.minutesToSave}}\' }"/> </div> </div> </div> </div> </div> <p> <span ng-show=controller.showDuplicateShortcutMessage class="shortcuts-lvl1-label fadein fadeout" translate={{controller.errorTxt}}></span>&nbsp; </p> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-gray" translate=l10n.back ng-click=controller.done() hover-focus/> <button class="osc-button osc-button-gray" translate=l10n.resetToDefaults ng-click=controller.resetShortcuts() ng-disabled=controller.disableButtons ng-class="{\'item-disabled\': controller.disableButtons}" hover-focus/> </div> </div> ';
}
