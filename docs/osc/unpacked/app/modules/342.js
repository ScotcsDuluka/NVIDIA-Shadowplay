// ─────────────────────────────────────────────────────────────
// APP MODULE 342
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports =
    '<div class=mods-control-title-sizing> <span class=mods-control-title-label ng-class="{\'general-disabled\': aControl.enabled === false}" translate={{aControl.title}}></span> <div ng-if="aControl.textPosition === \'header-right\'" class=float-right ng-class="{\'general-disabled\': aControl.enabled === false}"> <span class=mods-slider-value-label>{{aControl.text(aControl)}}</span> </div> <div class=clearfix></div> </div> <nv-slider snap-to-default=true skip-snap-when-kb-input=true focus-only tabindex={{aControl.tabIndex}} aria-label=position-slider md-primary ng-model=aControl.value step={{aControl.step}} min={{aControl.range.min}} max={{aControl.range.max}} default-value={{aControl.default}} ng-change=aControl.onChange(aControl) ng-disabled="{{aControl.enabled === false}}"></nv-slider> <div ng-if="aControl.textPosition === \'footer-center\'"> <h2 class="mods-slider-value-label center">{{aControl.text(aControl)}}</h2> </div> <div ng-if="aControl.textPosition === \'footer-sides\'"> <span class="mods-slider-value-label float-left" translate={{aControl.footerLeft}}></span> <span class="mods-slider-value-label float-right" translate={{aControl.footerRight}}></span> <br/> </div> ';
}
