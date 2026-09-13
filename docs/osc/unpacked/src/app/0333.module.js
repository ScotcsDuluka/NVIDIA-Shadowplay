// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 333
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div class=osc-content layout=row> <div class=osc-bread-crumb> <nv-osc-tile title={{removeMenu.title}} icon-font={{removeMenu.icon}} status={{removeMenu.status}}></nv-osc-tile> </div> <div class=gallery-central-div-remove-panel layout=column> <div layout=row> <div class=gallery-inside-central-div> <md-content layout-padding layout-xs=column layout=row layout-wrap class=gallery-files-content-all> <md-card class=gallery-files-thumbnail> <img ng-src={{removeMenu.fileToRemove.data}} class=md-card-image alt="image caption"> <md-card-footer class=gallery-card-footer>{{removeMenu.fileToRemove.folder}} <br/>{{removeMenu.fileToRemove.date}}</md-card-footer> </md-card> </md-content> </div> <h3 ng-if="removeMenu.fileToRemove.file.type == \'video\'" class=gallery-remove-panel-text translate=l10n.confirmRemoveRecording></h3> <h3 ng-if="removeMenu.fileToRemove.file.type == \'image\'" class=gallery-remove-panel-text translate=l10n.confirmRemoveScreenshot></h3> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-gray" translate=l10n.remove ng-click=removeMenu.remove() hover-focus></button> <button class="osc-button osc-button-gray" translate=l10n.back ng-click=removeMenu.back() hover-focus></button> </div> </div> ';
}
