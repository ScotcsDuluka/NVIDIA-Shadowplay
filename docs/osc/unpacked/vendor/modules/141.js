// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 141
// role       : provider $sanitize | filter linky
// defines    : angular.module("ngSanitize")
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  /**
   * @license AngularJS v1.5.5
   * (c) 2010-2016 Google, Inc. http://angularjs.org
   * License: MIT
   */
  ! function(e, t) {
    "use strict";

    function n() {
      var e = !1;
      this.$get = ["$$sanitizeUri", function(n) {
        return e && t.extend($, E),
          function(e) {
            var t = [];
            return o(e, c(t, function(e, t) {
              return !/^unsafe:/.test(n(e, t))
            })), t.join("")
          }
      }], this.enableSvg = function(n) {
        return t.isDefined(n) ? (e = n, this) : e
      }
    }

    function r(e) {
      var n = [],
        r = c(n, t.noop);
      return r.chars(e), n.join("")
    }

    function i(e, n) {
      var r, i = {},
        o = e.split(",");
      for (r = 0; r < o.length; r++) i[n ? t.lowercase(o[r]) : o[r]] = !0;
      return i
    }

    function o(t, n) {
      null === t || void 0 === t ? t = "" : "string" != typeof t && (t = "" + t), l.innerHTML = t;
      var r = 5;
      do {
        if (0 === r) throw d("uinput", "Failed to sanitize html because the input is unstable");
        r--, e.document.documentMode && u(l), t = l.innerHTML, l.innerHTML = t
      } while (t !== l.innerHTML);
      for (var i = l.firstChild; i;) {
        switch (i.nodeType) {
          case 1:
            n.start(i.nodeName.toLowerCase(), a(i.attributes));
            break;
          case 3:
            n.chars(i.textContent)
        }
        var o;
        if (!(o = i.firstChild) && (1 == i.nodeType && n.end(i.nodeName.toLowerCase()), o = i.nextSibling, !o))
          for (; null == o && (i = i.parentNode, i !== l);) o = i.nextSibling, 1 == i.nodeType && n.end(i.nodeName
            .toLowerCase());
        i = o
      }
      for (; i = l.firstChild;) l.removeChild(i)
    }

    function a(e) {
      for (var t = {}, n = 0, r = e.length; n < r; n++) {
        var i = e[n];
        t[i.name] = i.value
      }
      return t
    }

    function s(e) {
      return e.replace(/&/g, "&amp;").replace(f, function(e) {
        var t = e.charCodeAt(0),
          n = e.charCodeAt(1);
        return "&#" + (1024 * (t - 55296) + (n - 56320) + 65536) + ";"
      }).replace(h, function(e) {
        return "&#" + e.charCodeAt(0) + ";"
      }).replace(/</g, "&lt;").replace(/>/g, "&gt;")
    }

    function c(e, n) {
      var r = !1,
        i = t.bind(e, e.push);
      return {
        start: function(e, o) {
          e = t.lowercase(e), !r && _[e] && (r = e), r || $[e] !== !0 || (i("<"), i(e), t.forEach(o, function(r, o) {
            var a = t.lowercase(o),
              c = "img" === e && "src" === a || "background" === a;
            x[a] !== !0 || w[a] === !0 && !n(r, c) || (i(" "), i(o), i('="'), i(s(r)), i('"'))
          }), i(">"))
        },
        end: function(e) {
          e = t.lowercase(e), r || $[e] !== !0 || p[e] === !0 || (i("</"), i(e), i(">")), e == r && (r = !1)
        },
        chars: function(e) {
          r || i(s(e))
        }
      }
    }

    function u(t) {
      if (t.nodeType === e.Node.ELEMENT_NODE)
        for (var n = t.attributes, r = 0, i = n.length; r < i; r++) {
          var o = n[r],
            a = o.name.toLowerCase();
          "xmlns:ns1" !== a && 0 !== a.indexOf("ns1:") || (t.removeAttributeNode(o), r--, i--)
        }
      var s = t.firstChild;
      s && u(s), s = t.nextSibling, s && u(s)
    }
    var l, d = t.$$minErr("$sanitize"),
      f = /[\uD800-\uDBFF][\uDC00-\uDFFF]/g,
      h = /([^\#-~ |!])/g,
      p = i("area,br,col,hr,img,wbr"),
      m = i("colgroup,dd,dt,li,p,tbody,td,tfoot,th,thead,tr"),
      v = i("rp,rt"),
      g = t.extend({}, v, m),
      y = t.extend({}, m, i(
        "address,article,aside,blockquote,caption,center,del,dir,div,dl,figure,figcaption,footer,h1,h2,h3,h4,h5,h6,header,hgroup,hr,ins,map,menu,nav,ol,pre,section,table,ul"
        )),
      b = t.extend({}, v, i(
        "a,abbr,acronym,b,bdi,bdo,big,br,cite,code,del,dfn,em,font,i,img,ins,kbd,label,map,mark,q,ruby,rp,rt,s,samp,small,span,strike,strong,sub,sup,time,tt,u,var"
        )),
      E = i(
        "circle,defs,desc,ellipse,font-face,font-face-name,font-face-src,g,glyph,hkern,image,linearGradient,line,marker,metadata,missing-glyph,mpath,path,polygon,polyline,radialGradient,rect,stop,svg,switch,text,title,tspan"
        ),
      _ = i("script,style"),
      $ = t.extend({}, p, y, b, g),
      w = i("background,cite,href,longdesc,src,xlink:href"),
      T = i(
        "abbr,align,alt,axis,bgcolor,border,cellpadding,cellspacing,class,clear,color,cols,colspan,compact,coords,dir,face,headers,height,hreflang,hspace,ismap,lang,language,nohref,nowrap,rel,rev,rows,rowspan,rules,scope,scrolling,shape,size,span,start,summary,tabindex,target,title,type,valign,value,vspace,width"
        ),
      C = i(
        "accent-height,accumulate,additive,alphabetic,arabic-form,ascent,baseProfile,bbox,begin,by,calcMode,cap-height,class,color,color-rendering,content,cx,cy,d,dx,dy,descent,display,dur,end,fill,fill-rule,font-family,font-size,font-stretch,font-style,font-variant,font-weight,from,fx,fy,g1,g2,glyph-name,gradientUnits,hanging,height,horiz-adv-x,horiz-origin-x,ideographic,k,keyPoints,keySplines,keyTimes,lang,marker-end,marker-mid,marker-start,markerHeight,markerUnits,markerWidth,mathematical,max,min,offset,opacity,orient,origin,overline-position,overline-thickness,panose-1,path,pathLength,points,preserveAspectRatio,r,refX,refY,repeatCount,repeatDur,requiredExtensions,requiredFeatures,restart,rotate,rx,ry,slope,stemh,stemv,stop-color,stop-opacity,strikethrough-position,strikethrough-thickness,stroke,stroke-dasharray,stroke-dashoffset,stroke-linecap,stroke-linejoin,stroke-miterlimit,stroke-opacity,stroke-width,systemLanguage,target,text-anchor,to,transform,type,u1,u2,underline-position,underline-thickness,unicode,unicode-range,units-per-em,values,version,viewBox,visibility,width,widths,x,x-height,x1,x2,xlink:actuate,xlink:arcrole,xlink:role,xlink:show,xlink:title,xlink:type,xml:base,xml:lang,xml:space,xmlns,xmlns:xlink,y,y1,y2,zoomAndPan",
        !0),
      x = t.extend({}, w, C, T);
    ! function(e) {
      var t;
      if (!e.document || !e.document.implementation) throw d("noinert", "Can't create an inert html document");
      t = e.document.implementation.createHTMLDocument("inert");
      var n = t.documentElement || t.getDocumentElement(),
        r = n.getElementsByTagName("body");
      if (1 === r.length) l = r[0];
      else {
        var i = t.createElement("html");
        l = t.createElement("body"), i.appendChild(l), t.appendChild(i)
      }
    }(e), t.module("ngSanitize", []).provider("$sanitize", n), t.module("ngSanitize").filter("linky", ["$sanitize",
      function(e) {
        var n = /((ftp|https?):\/\/|(www\.)|(mailto:)?[A-Za-z0-9._%+-]+@)\S*[^\s.;,(){}<>"\u201d\u2019]/i,
          i = /^mailto:/i,
          o = t.$$minErr("linky"),
          a = t.isString;
        return function(s, c, u) {
          function l(e) {
            e && v.push(r(e))
          }

          function d(e, n) {
            var r;
            if (v.push("<a "), t.isFunction(u) && (u = u(e)), t.isObject(u))
              for (r in u) v.push(r + '="' + u[r] + '" ');
            else u = {};
            !t.isDefined(c) || "target" in u || v.push('target="', c, '" '), v.push('href="', e.replace(/"/g,
              "&quot;"), '">'), l(n), v.push("</a>")
          }
          if (null == s || "" === s) return s;
          if (!a(s)) throw o("notstring", "Expected string but received: {0}", s);
          for (var f, h, p, m = s, v = []; f = m.match(n);) h = f[0], f[2] || f[4] || (h = (f[3] ? "http://" :
            "mailto:") + h), p = f.index, l(m.substr(0, p)), d(h, f[0].replace(i, "")), m = m.substring(p + f[0]
            .length);
          return l(m), e(v.join(""))
        }
      }
    ])
  }(window, window.angular)
}
