// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 321
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div class=osc-content layout=row ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{nvOauthMenu.title}} icon-font={{nvOauthMenu.icon}} status={{nvOauthMenu.status}}></nv-osc-tile> </div> <div id=oauth class="gallery-central-div-upload oauth-menu-popup-div"> <nv-oauth-dialogue dialogue-params=nvOauthMenu.oauthDialogueParams dialogue-closed-callback=nvOauthMenu.onPopupClose()></nv-oauth-dialogue> </div> <div class=osc-side-buttons> <button class="osc-button osc-button-gray" translate=l10n.cancel ng-click=nvOauthMenu.cancel() hover-focus></button> </div> </div> ';
}
