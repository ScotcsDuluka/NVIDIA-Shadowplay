// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 322
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div class=notifier-green> </div> <div class=notifier-black> <div class=notifier layout=row layout-align="start center"> <img class=notifier-icon ng-src={{notifierIconPath}} ng-if=notifierIconPath> <md-icon class="share-icon notifier-icon {{notifierIcon}}" ng-if=notifierIcon></md-icon> <div class=notifier-text-container layout=column layout-align="start start"> <div class=notifier-text> <p translate={{notifierMessage}}></p> </div> <div ng-if=notifierMessageSubtext class="notifier-text notifier-subtext"> <p translate={{notifierMessageSubtext}}></p> </div> </div> </div> </div> ';
}
