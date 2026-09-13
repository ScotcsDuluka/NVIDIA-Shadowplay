// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 330
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports =
    '<div class=filter-directive layout=row> <div layout=row> <h3 class=gallery-filter-pretext ng-style="{width: leftWidth}">{{filterMenu.preText}}</h3> <md-input-container class=gallery-file-dropdown ng-style="{width: rightWidth}" ng-keydown=filterMenu.keyDownDropdown($event)> <md-select id=button_filter class="osc-select gallery-filetype-select" ng-style="{width: rightWidth}" ng-model=filterMenu.item ng-model-options="{trackBy: \'$value.id\'}" placeholder="{{filterMenu.item.title | translate }}" md-on-open=filterMenu.selectionOpen() md-on-close=filterMenu.selectionClose() ng-class="{\'gallery-item-disabled\': !filterMenu.enableFilter, \'gallery-item-enabled\': filterMenu.enableFilter}" ng-disabled=!filterMenu.enableFilter hover-focus> <md-option md-ink-ripple=false ng-value=item ng-repeat-start="item in Items" hover-focus ng-if="item.id !== \'divider\' && item.hidden !== true"> <div ng-if=item.image layout=row layout-align="start center"> <md-icon class="share-icon icon-normal gallery-icons {{item.image}}"></md-icon> <span>{{item.title | translate}}</span> </div> <div ng-if=!item.image class=gallery-spacing>{{item.title | translate}}</div> </md-option> <md-divider ng-repeat-end ng-if="item.id === \'divider\' && item.hidden !== true" class=sub-menu-divider></md-divider> </md-select> </md-input-container> </div> </div> ';
}
