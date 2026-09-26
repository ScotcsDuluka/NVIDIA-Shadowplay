// share_script.h - scripts the render process injects into every frame.
// 1) AugmentationSource: port of the NON-cefQuery half of
//    Overlay.Engine\CefQueryBridge.vb PolyfillSource (host-owned backdrop
//    + feature-unlock shim + osc open/close hooks). The cefQuery polyfill
//    itself is intentionally NOT ported: on this host window.cefQuery is
//    NATIVE (CEF message router), and the polyfill's
//    `if(window.cefQuery) return;` guard would abort the augmentation.
// 2) ProofDriverSource: proof-of-life driver (DOM probe + synthetic
//    cefQuery round-trip whose JS callback echoes the observed response
//    back through a __PROOF_ECHO query — closing the loop host->page->host).
#ifndef SHARE_SCRIPT_H_
#define SHARE_SCRIPT_H_

inline const char* AugmentationSource() {
  return
  "(function(){"
  // Host-owned backdrop (backdrop div + CSS; page paints nothing itself).
  "try{window.__bdStatus='registered';var bdfn=function(){"
  "try{if(document.getElementById('oscengine-backdrop')){window.__bdStatus='already';return;}"
  "var st=document.createElement('style');st.id='oscengine-backdrop-css';"
  "st.textContent='html.oscengine-open #oscengine-backdrop{display:block}html:not(.oscengine-open) .base{visibility:hidden!important}html.oscengine-toast #oscengine-backdrop{display:block;background:rgba(8,8,8,0.35)}html.oscengine-toast .base{visibility:visible!important}';"
  "document.head.appendChild(st);"
  "var bd=document.createElement('div');bd.id='oscengine-backdrop';"
  "bd.style.cssText='position:fixed;left:0;top:0;width:100%;height:100%;background:rgba(8,8,8,0.35);z-index:-1;pointer-events:none;display:none';"
  "document.body.appendChild(bd);window.__bdStatus='created';"
  "}catch(e){window.__bdStatus='err:'+e.message;}};"
  "if(document.body){bdfn();}else{document.addEventListener('DOMContentLoaded',bdfn);}}catch(e){window.__bdStatus='reg-err:'+e.message;}"
  // Feature unlock shim (port; page-side only, no host behavior change).
  "try{window.__unlockStatus='reg';"
  "var unlockFn=function(){"
  "try{if(!window.angular) return false;"
  "var inj=window.angular.element(document.body).injector(); if(!inj) return false;"
  "var q=inj.get('$q'); var nv=inj.get('nvCameraService'); if(!nv||!nv.isOn) return false;"
  "nv.isGfeAnselSupported=function(){return q.when(true);};"
  "nv.isModsOn=function(){return true;};"
  "nv.isOn=function(){return true;};"
  "try{var st=inj.get('$state');"
  "nv.launchUIForNvCamera=function(){st.go('nvcamera');};"
  "nv.launchUIForMods=function(){st.go('mods');};"
  "}catch(e){}"
  "try{var ds=inj.get('oscDisplayService');"
  "window.__oscOpen=function(){ds.openOSC();};"
  "window.__oscClose=function(){ds.closeOSC();};"
  "}catch(e){}"
  "window.__unlockStatus='done'; return true;"
  "}catch(e){window.__unlockStatus='err:'+e.message; return false;}};"
  "var uTries=0;"
  "var uTimer=setInterval(function(){uTries++;"
  "if(unlockFn()||uTries>150){clearInterval(uTimer);}} ,300);"
  "}catch(e){}"
  // Auto-open: REMOVED (owner call 2026-09-26 — "Overlay ติดได้ยังไง
  // เราไม่ได้กด alt z เลย"). The overlay opens ONLY on an explicit open:
  // Alt+Z through the node, or the launcher's OPEN OVERLAY
  // (POST /ShadowPlay/v.1.0/Hotkey/Toggle). Nothing self-opens at boot.
  // Visibility bridge: the host window is created HIDDEN and the HOST is
  // the one that sets html.oscengine-open (FlipOscDisplayState inside
  // QUERY_WIN_OPEN_OSC) — so watching the class here deadlocks. Mirror the
  // ROUTE instead: $state entering main.* = menu open -> QUERY_WIN_OPEN_OSC
  // (host shows the window + sets the class); back to base = close.
  "try{window.__oscWinVis=false;"
  "var vTimer=setInterval(function(){"
  "try{if(window.angular&&document.body){"
  "var inj3=window.angular.element(document.body).injector();"
  "if(!inj3)return;"
  "var st3=inj3.get('$state').current.name;"
  "var open=(st3.indexOf('main')===0);"
  "if(open!==window.__oscWinVis){"
  "window.__oscWinVis=open;"
  "window.cefQuery({request:JSON.stringify({command:open?'QUERY_WIN_OPEN_OSC':'QUERY_WIN_CLOSE_OSC',enableInput:open}),persistent:false,onSuccess:function(){},onFailure:function(){}});}"
  "}}catch(e){}}"
  ",300);"
  "}catch(e){}"
  "})();";
}

inline const char* ProofDriverSource() {
  return
  "(function(){"
  "function q(req,onOk,onFail){try{return window.cefQuery({request:JSON.stringify(req),persistent:false,onSuccess:onOk||function(){},onFailure:onFail||function(){}});}catch(e){}}"
  "function echo(v){q({command:'__PROOF_ECHO',value:String(v)});}"
  "try{"
  "var probe={title:document.title,href:location.href,angular:!!window.angular,"
  "baseCount:document.querySelectorAll('.base').length,"
  "bodyChildren:document.body?document.body.children.length:-1,"
  "hasCefQuery:(typeof window.cefQuery==='function')};"
  "q({command:'__PROOF_DOM',probe:JSON.stringify(probe)});"
  "q({command:'QUERY_OSC_SET_EXPERIMENTAL'},function(r){echo(r);},function(e,m){echo('FAIL:'+e+':'+m);});"
  "q({command:'__PROOF_OPEN_UI'});"
  "}catch(e){echo('EXC:'+e.message);}"
  "})();";
}

#endif  // SHARE_SCRIPT_H_
