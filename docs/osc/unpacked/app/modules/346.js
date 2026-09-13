// ─────────────────────────────────────────────────────────────
// APP MODULE 346
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports =
    '<div class="osd-general osd-comments-container-size" ng-if=cont.isVisible> <div class="osd-comments osd-comments-size" layout=column layout-align="none none"> <div class=osd-comment-hotkey flex=none ng-if=cont.showHideMessage> <span class=osd-comment-hotkey-text>{{cont.showHideMessage}}</span> </div> <div class=osd-comments-container layout=column layout-align="none none"> <div class=osd-comment-single ng-repeat="item in cont.comments" ng-if=cont.comments flex=none layout=row layout-align="start start"> <div class=osd-comment-avatar> <img class=osd-comment-avatar ng-src={{item.from.avatarUri}} /> </div> <div class=osd-comment-comment> <span class=osd-comment-username>{{item.from.name}}</span> <span class=osd-comment-text>{{item.message}}</span> </div> </div> </div> </div> </div> '
}
