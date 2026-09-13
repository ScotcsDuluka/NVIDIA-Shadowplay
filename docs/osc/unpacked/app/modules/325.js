// ─────────────────────────────────────────────────────────────
// APP MODULE 325
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports =
    '<div class=osc-content layout=row ng-cloak> <div class=osc-bread-crumb> <nv-osc-tile title={{coplayInvite.title}} icon-font={{coplayInvite.icon}} status={{coplayInvite.status}} /> </div> <div class="osc-central-div coplay-invite-panel" layout=column> <div class=invite-contents layout=row> <div layout=column flex> <h2 class=invite-text translate=l10n.coplayFriendsEmail></h2> <h2 class=invite-pad></h2> <h2 class=invite-text translate=l10n.coplayYourName></h2> </div> <div layout=column> <md-input-container class=invite-inputs md-no-float> <md-autocomplete md-clear-button=false md-no-cache=true md-min-length=0 md-input-maxlength=255 md-autoselect=true md-select-on-focus md-selected-item=coplayInvite.email md-search-text=coplayInvite.emailSearch md-items="item in coplayInvite.querySearch(coplayInvite.emailSearch)" md-item-text=item md-menu-class=invite-email-list> <md-item-template> <span md-highlight-text=coplayInvite.emailSearch>{{item}}</span> </md-item-template> <md-not-found></md-not-found> </md-autocomplete> </md-input-container> <h2 class=invite-pad></h2> <md-input-container class=invite-inputs md-no-float> <input type=text ng-model=coplayInvite.name ng-maxlength=32 aria-label=name>  </md-input-container></div> </div> </div> <div class=osc-side-buttons layout=column> <button class="osc-button osc-button-green" translate=l10n.invite ng-click=coplayInvite.invite() hover-focus></button> <button class="osc-button osc-button-gray" translate=l10n.back ng-click=coplayInvite.back() hover-focus></button> </div> </div> '
}
