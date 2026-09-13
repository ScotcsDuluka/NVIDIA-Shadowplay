// ─────────────────────────────────────────────────────────────
// APP MODULE 333
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports =
    '<div class=osc-content layout=row> <div class=osc-bread-crumb> <nv-osc-tile title={{removeMenu.title}} icon-font={{removeMenu.icon}} status={{removeMenu.status}}></nv-osc-tile> </div> <div class=gallery-central-div-remove-panel layout=column> <div layout=row> <div class=gallery-inside-central-div> <md-content layout-padding layout-xs=column layout=row layout-wrap class=gallery-files-content-all> <md-card class=gallery-files-thumbnail> <img ng-src={{removeMenu.fileToRemove.data}} class=md-card-image alt="image caption"> <md-card-footer class=gallery-card-footer>{{removeMenu.fileToRemove.folder}} <br/>{{removeMenu.fileToRemove.date}}</md-card-footer> </md-card> </md-content> </div> <h3 ng-if="removeMenu.fileToRemove.file.type == \'video\'" class=gallery-remove-panel-text translate=l10n.confirmRemoveRecording></h3> <h3 ng-if="removeMenu.fileToRemove.file.type == \'image\'" class=gallery-remove-panel-text translate=l10n.confirmRemoveScreenshot></h3> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-gray" translate=l10n.remove ng-click=removeMenu.remove() hover-focus></button> <button class="osc-button osc-button-gray" translate=l10n.back ng-click=removeMenu.back() hover-focus></button> </div> </div> '
}
