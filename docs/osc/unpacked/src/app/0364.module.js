// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 364
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div class=osc-content layout=row ng-init=controller.initPreferencesRecordings() ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{controller.title}} icon-font={{controller.icon}} status={{controller.status}} /> </div> <div class=osc-central-div layout=column> <div class=preferences-root-panel> <div class=preferences-root-label> <p translate=l10n.recordings /> </div> <div ng-if=controller.settingsDisabled class=settings-text-warning> <md-icon class="share-icon icon32 icon-normal icon-notify_warning mta-icon-warning settings-warning-icon-color"></md-icon> <span translate=l10n.settingsRecordingsDisable></span> </div> <div layout=column ng-class="{\'item-disabled\': controller.settingsDisabled}"> <div> <span class=recordings-labels translate=l10n.tempFilesLabel></span> </div> <div class=recordings-row layout=row> <div class=recordings-paths-outer flex> <div class=recordings-paths-inner> <input class=recordings-paths type=text ng-readonly=true ng-model=controller.tempFilesPath ng-class="{\'item-disabled\': controller.settingsDisabled}" ng-disabled=controller.settingsDisabled /> </div> </div> <div class="recordings-ellipsis-outer hover-list-item" ng-click=controller.getTempFilesPath() hover-focus focus=true ng-disabled=controller.settingsDisabled> <div class=recordings-ellipsis-nested> <span>...</span> </div> </div> </div> <div class=recordings-separator-2></div> <div> <span class=recordings-labels translate=l10n.videosLabel></span> </div> <div class=recordings-row layout=row> <div class=recordings-paths-outer flex> <div class=recordings-paths-inner> <input class=recordings-paths type=text ng-readonly=true ng-model=controller.videosPath ng-class="{\'item-disabled\': controller.settingsDisabled}" ng-disabled=controller.settingsDisabled /> </div> </div> <div class="recordings-ellipsis-outer hover-list-item" ng-click=controller.getVideosPath() hover-focus ng-disabled=controller.settingsDisabled> <div class=recordings-ellipsis-nested> <span>...</span> </div> </div> </div> </div> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-gray" translate=l10n.back ng-click=controller.done() hover-focus></button> </div> </div> ';
}
