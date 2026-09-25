(function(){
try{window.__bdStatus='registered';var bdfn=function(){
try{if(document.getElementById('oscengine-backdrop')){window.__bdStatus='already';return;}
var st=document.createElement('style');st.id='oscengine-backdrop-css';
st.textContent='html.oscengine-open #oscengine-backdrop{display:block}html:not(.oscengine-open) .base{visibility:hidden!important}html.oscengine-toast #oscengine-backdrop{display:block;background:rgba(8,8,8,0.35)}html.oscengine-toast .base{visibility:visible!important}';
document.head.appendChild(st);
var bd=document.createElement('div');bd.id='oscengine-backdrop';
bd.style.cssText='position:fixed;left:0;top:0;width:100%;height:100%;background:rgba(8,8,8,0.35);z-index:-1;pointer-events:none;display:none';
document.body.appendChild(bd);window.__bdStatus='created';
}catch(e){window.__bdStatus='err:'+e.message;}};
if(document.body){bdfn();}else{document.addEventListener('DOMContentLoaded',bdfn);}}catch(e){window.__bdStatus='reg-err:'+e.message;}
try{window.__unlockStatus='reg';
var unlockFn=function(){
try{if(!window.angular) return false;
var inj=window.angular.element(document.body).injector(); if(!inj) return false;
var q=inj.get('$q'); var nv=inj.get('nvCameraService'); if(!nv||!nv.isOn) return false;
nv.isGfeAnselSupported=function(){return q.when(true);};
nv.isModsOn=function(){return true;};
nv.isOn=function(){return true;};
try{var st=inj.get('$state');
nv.launchUIForNvCamera=function(){st.go('nvcamera');};
nv.launchUIForMods=function(){st.go('mods');};
}catch(e){}
try{var ds=inj.get('oscDisplayService');
window.__oscOpen=function(){ds.openOSC();};
window.__oscClose=function(){ds.closeOSC();};
}catch(e){}
window.__unlockStatus='done'; return true;
}catch(e){window.__unlockStatus='err:'+e.message; return false;}};
var uTries=0;
var uTimer=setInterval(function(){uTries++;
if(unlockFn()||uTries>150){clearInterval(uTimer);}} ,300);
}catch(e){}
try{var oTries=0;
var oTimer=setInterval(function(){oTries++;
try{if(window.angular&&document.querySelector('.base')){
var inj2=window.angular.element(document.body).injector();
var st2=inj2.get('$state');
if(st2.current.name==='base'){st2.go('main.main-menu');
if(window.__oscOpen){window.__oscOpen();}
clearInterval(oTimer);}}
}catch(e){}
if(oTries>40){clearInterval(oTimer);}}
,300);
}catch(e){}
})();