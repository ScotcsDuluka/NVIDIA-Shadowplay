// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 0
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  require(136);
  var i = require(1) /* app/1 — main (module) */;
  require(135) /* app/135 — appService (service) */, require(142), i.ngMainModule.run(["appService", function(e) {
    e.main();
  }]);
}
