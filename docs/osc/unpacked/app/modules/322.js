// ─────────────────────────────────────────────────────────────
// APP MODULE 322
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports =
    '<div class=notifier-green> </div> <div class=notifier-black> <div class=notifier layout=row layout-align="start center"> <img class=notifier-icon ng-src={{notifierIconPath}} ng-if=notifierIconPath> <md-icon class="share-icon notifier-icon {{notifierIcon}}" ng-if=notifierIcon></md-icon> <div class=notifier-text-container layout=column layout-align="start start"> <div class=notifier-text> <p translate={{notifierMessage}}></p> </div> <div ng-if=notifierMessageSubtext class="notifier-text notifier-subtext"> <p translate={{notifierMessageSubtext}}></p> </div> </div> </div> </div> '
}
