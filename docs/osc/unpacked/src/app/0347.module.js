// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 347
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    "<div ng-show=osd.anythingVisible ng-if=osd.enabled> <div class=osd-container ng-repeat=\"(key,class) in osd.quadrants\"> <div ng-class=class ng-style=\"{'transform': 'scale(' + osd.scaleFactor + ')'}\" layout=column> <div layout=row flex=none> <div ng-if=\"osd.visibility['STATUS'] === key || osd.visibility['FPS'] === key\"> <nv-shadowplay-status/> </div> <div ng-if=\"osd.visibility['VIEWERS'] === key\"> <nv-osd-viewer-count/> </div> </div> <div layout=row flex=none> <div ng-if=\"osd.visibility['WEBCAM'] === key\"> <nv-osd-webcam/> </div> </div> <div ng-if=\"osd.visibility['COMMENTS'] === key\"> <div layout=row flex=grow> <nv-osd-comments/> </div> </div> <div ng-if=\"osd.visibility['PERFORMANCE'] === key\" ng-style=\"{'transform': 'scale(' + 1 / osd.scaleFactor + ')', 'transform-origin':'inherit'}\"> <div layout=row flex=grow> <nv-osd-perf-stats class=perf-stat-div /> </div> </div> </div> </div> </div> ";
}
