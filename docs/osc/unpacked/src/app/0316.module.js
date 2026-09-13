// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 316
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div class=confirmation-directive> <div class=osc-content layout=row ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{confirmation.title}} icon-font={{confirmation.icon}} status={{confirmation.status}}></nv-osc-tile> </div> <div class="osc-central-div confirmation-panel"> <div class=confirmation-contents layout=column flex> <h2 class=confirmation-text translate={{confirmation.question}}></h2> <p></p> <h2 class=confirmation-text translate={{confirmation.footnote}}></h2> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-green" translate={{confirmation.topButton}} ng-click=confirmation.topButtonFunction() hover-focus></button> <button class="osc-button osc-button-gray" translate={{confirmation.bottomButton}} ng-click=confirmation.bottomButtonFunction() ng-if=!confirmation.hideBottomButton hover-focus></button> </div> </div> </div> ';
}
