// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 42
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  n(204);
  for (var r = n(4), i = n(11), o = n(16), a = n(5)("toStringTag"), s =
      "CSSRuleList,CSSStyleDeclaration,CSSValueList,ClientRectList,DOMRectList,DOMStringList,DOMTokenList,DataTransferItemList,FileList,HTMLAllCollection,HTMLCollection,HTMLFormElement,HTMLSelectElement,MediaList,MimeTypeArray,NamedNodeMap,NodeList,PaintRequestList,Plugin,PluginArray,SVGLengthList,SVGNumberList,SVGPathSegList,SVGPointList,SVGStringList,SVGTransformList,SourceBufferList,StyleSheetList,TextTrackCueList,TextTrackList,TouchList"
      .split(","), c = 0; c < s.length; c++) {
    var u = s[c],
      l = r[u],
      d = l && l.prototype;
    d && !d[a] && i(d, a, u), o[u] = o.Array
  }
}
