// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 350
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div class="osd-general osd-comments osd-viewer-count-center osd-viewer-count-text" layout=row ng-if=cont.visible> <div ng-if="cont.viewerCount >= 0" class="osd-viewer-count-margin osd-viewer-count-center"> <md-icon class="share-icon icon-osd_viewer_count icon24 icon-normal"></md-icon> <span>{{cont.viewerCount}}</span> </div> <div ng-if="cont.reactionCount > 0" class="osd-viewer-count-margin-reactions osd-viewer-count-center"> <div class=osd-reactions> <div class=osd-reaction-icon-holder ng-repeat="item in cont.reactions"> <md-icon md-svg-icon={{item}} class=osd-reaction-icon></md-icon> </div> </div> <span class=osd-reaction-count-margin>{{cont.reactionCount}}</span> </div> <div ng-if="cont.commentCount > 0" class="osd-viewer-count-margin osd-viewer-count-center"> <md-icon class="share-icon icon-osd_comments icon24 icon-normal osd-comment-icon-offset"></md-icon> <span>{{cont.commentCount}}</span> </div> <div class=osd-viewer-count-margin-reactions /> </div> ';
}
