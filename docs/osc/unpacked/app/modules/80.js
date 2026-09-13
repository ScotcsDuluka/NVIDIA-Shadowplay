// ─────────────────────────────────────────────────────────────
// APP MODULE 80
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  n(414);
  for (var i = n(21), o = n(27), r = n(41), a = n(16)("toStringTag"), l =
      "CSSRuleList,CSSStyleDeclaration,CSSValueList,ClientRectList,DOMRectList,DOMStringList,DOMTokenList,DataTransferItemList,FileList,HTMLAllCollection,HTMLCollection,HTMLFormElement,HTMLSelectElement,MediaList,MimeTypeArray,NamedNodeMap,NodeList,PaintRequestList,Plugin,PluginArray,SVGLengthList,SVGNumberList,SVGPathSegList,SVGPointList,SVGStringList,SVGTransformList,SourceBufferList,StyleSheetList,TextTrackCueList,TextTrackList,TouchList"
      .split(","), s = 0; s < l.length; s++) {
    var d = l[s],
      c = i[d],
      u = c && c.prototype;
    u && !u[a] && o(u, a, d), r[d] = r.Array
  }
}
